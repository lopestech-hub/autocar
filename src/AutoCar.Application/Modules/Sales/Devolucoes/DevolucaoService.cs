using AutoCar.Application.Modules.Sales.Devolucoes.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Sales.Devolucoes;

/// <summary>
/// Casos de uso da Devolução. Parte de uma pré-venda FATURADA, calcula o saldo devolvível de cada
/// item (vendido − já devolvido em devoluções anteriores) e registra a devolução repondo o estoque
/// numa transação única (delegada ao repositório). Converte invariantes do domínio e conflito de
/// concorrência em <see cref="Result"/> — a UI nunca recebe exceção.
///
/// As quantidades da venda são decimais (modelo do documento), mas o estoque/devolução trabalham com
/// inteiros (autopeça não fraciona) — a conversão usa <see cref="QuantidadeEstoque.DeDocumento"/>, que
/// rejeita fração em vez de truncar (mesmo tratamento do faturamento).
/// </summary>
public sealed class DevolucaoService : IDevolucaoService
{
    private readonly IPreVendaRepository _preVendas;
    private readonly IDevolucaoRepository _devolucoes;
    private readonly IValidator<CriarDevolucaoDto> _validator;

    public DevolucaoService(
        IPreVendaRepository preVendas,
        IDevolucaoRepository devolucoes,
        IValidator<CriarDevolucaoDto> validator)
    {
        _preVendas = preVendas;
        _devolucoes = devolucoes;
        _validator = validator;
    }

    public async Task<Result<IReadOnlyList<ItemDevolvivelDto>>> ListarItensDevolviveisAsync(
        Guid idPreVenda, CancellationToken ct = default)
    {
        var preVenda = await _preVendas.ObterPorIdAsync(idPreVenda, ct);
        if (preVenda is null)
            return Result.Falhar<IReadOnlyList<ItemDevolvivelDto>>(Error.NaoEncontrado("Venda não encontrada."));
        if (preVenda.Situacao != SituacaoPreVenda.Faturada)
            return Result.Falhar<IReadOnlyList<ItemDevolvivelDto>>(
                Error.Validacao("Só é possível devolver itens de uma venda faturada."));

        var jaDevolvido = await _devolucoes.TotalDevolvidoPorProdutoAsync(idPreVenda, ct);

        // Agrupa por produto (a venda pode ter o mesmo produto em mais de uma linha) e calcula o saldo.
        var itens = preVenda.Itens
            .GroupBy(i => i.IdProduto)
            .Select(g =>
            {
                var primeiro = g.First();
                var qtdVendida = QuantidadeEstoque.DeDocumento(g.Sum(i => i.Qtd), primeiro.DescricaoProduto);
                var qtdJaDevolvida = jaDevolvido.TryGetValue(g.Key, out var d) ? d : 0;
                return new ItemDevolvivelDto(
                    g.Key, primeiro.DescricaoProduto, primeiro.VlrUnitario,
                    qtdVendida, qtdJaDevolvida, Math.Max(0, qtdVendida - qtdJaDevolvida));
            })
            .ToList();

        return Result.Ok<IReadOnlyList<ItemDevolvivelDto>>(itens);
    }

    public async Task<Result<DevolucaoDto>> CriarAsync(
        Guid idUsuario, CriarDevolucaoDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<DevolucaoDto>(Error.Validacao(
                validacao.Errors.Count > 0 ? validacao.Errors[0].ErrorMessage : "Dados inválidos."));

        var preVenda = await _preVendas.ObterPorIdAsync(dto.IdPreVenda, ct);
        if (preVenda is null)
            return Result.Falhar<DevolucaoDto>(Error.NaoEncontrado("Venda não encontrada."));
        if (preVenda.Situacao != SituacaoPreVenda.Faturada)
            return Result.Falhar<DevolucaoDto>(Error.Validacao("Só é possível devolver itens de uma venda faturada."));

        // Saldo devolvível por produto = vendido − já devolvido. Base para validar cada item.
        var jaDevolvido = await _devolucoes.TotalDevolvidoPorProdutoAsync(dto.IdPreVenda, ct);
        var vendidoPorProduto = preVenda.Itens
            .GroupBy(i => i.IdProduto)
            .ToDictionary(g => g.Key, g => (
                Vendido: QuantidadeEstoque.DeDocumento(g.Sum(i => i.Qtd), g.First().DescricaoProduto),
                g.First().DescricaoProduto,
                g.First().VlrUnitario));

        var itensDevolucao = new List<DevolucaoItem>();
        foreach (var item in dto.Itens)
        {
            if (!vendidoPorProduto.TryGetValue(item.IdProduto, out var venda))
                return Result.Falhar<DevolucaoDto>(Error.Validacao("Há um item que não pertence a esta venda."));

            var devolvivel = venda.Vendido - (jaDevolvido.TryGetValue(item.IdProduto, out var d) ? d : 0);
            if (item.Qtd > devolvivel)
                return Result.Falhar<DevolucaoDto>(Error.Validacao(
                    $"\"{venda.DescricaoProduto}\": não é possível devolver {item.Qtd} (devolvível: {devolvivel})."));

            itensDevolucao.Add(new DevolucaoItem(item.IdProduto, venda.DescricaoProduto, item.Qtd, venda.VlrUnitario));
        }

        try
        {
            var devolucao = new Devolucao(dto.IdPreVenda, idUsuario, dto.Motivo);
            devolucao.DefinirItens(itensDevolucao);

            await _devolucoes.RegistrarComEntradaEstoqueAsync(devolucao, ct);

            return Result.Ok(new DevolucaoDto(
                devolucao.Id, devolucao.CodDevolucao, devolucao.IdPreVenda, devolucao.Motivo, devolucao.VlrTotal));
        }
        catch (ConcorrenciaException)
        {
            return Result.Falhar<DevolucaoDto>(Error.Conflito(
                "O saldo de um produto foi alterado por outro terminal. Recarregue e tente novamente."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar<DevolucaoDto>(Error.Validacao(ex.Message));
        }
    }
}
