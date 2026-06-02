using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Sales.PreVendas;

/// <summary>
/// Casos de uso da Pré-venda. Valida o DTO (FluentValidation), confere o cliente (quando informado)
/// e os produtos dos itens, e delega o cálculo de totais e as invariantes ao agregado <see cref="PreVenda"/>.
/// As exceções de invariante do domínio (ex: desconto maior que o subtotal) são convertidas em
/// <see cref="Result"/> de erro — a UI nunca recebe exceção de regra de negócio.
/// </summary>
public sealed class PreVendaService : IPreVendaService
{
    private readonly IPreVendaRepository _preVendas;
    private readonly IClienteRepository _clientes;
    private readonly IProdutoRepository _produtos;
    private readonly IFaturamentoRepository _faturamento;
    private readonly IValidator<SalvarPreVendaDto> _validator;

    public PreVendaService(
        IPreVendaRepository preVendas,
        IClienteRepository clientes,
        IProdutoRepository produtos,
        IFaturamentoRepository faturamento,
        IValidator<SalvarPreVendaDto> validator)
    {
        _preVendas = preVendas;
        _clientes = clientes;
        _produtos = produtos;
        _faturamento = faturamento;
        _validator = validator;
    }

    public async Task<Result<PreVendaDto>> CriarAsync(Guid idUsuario, SalvarPreVendaDto dto, CancellationToken ct = default)
    {
        var erro = await ValidarAsync(dto, ct);
        if (erro is not null)
            return Result.Falhar<PreVendaDto>(erro);

        try
        {
            var preVenda = new PreVenda(
                idUsuario, dto.IdCliente, dto.NomeClienteAvulso,
                dto.VeiculoMontadora, dto.VeiculoModelo, dto.VeiculoAno, dto.VeiculoPlaca, dto.Observacao);
            preVenda.DefinirItens(MapearItens(dto.Itens));
            preVenda.AplicarDescontoGeral(dto.VlrDesconto);

            await _preVendas.AdicionarAsync(preVenda, ct);

            var criada = await _preVendas.ObterPorIdAsync(preVenda.Id, ct);
            return Result.Ok(Mapear(criada!));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar<PreVendaDto>(Error.Validacao(ex.Message));
        }
    }

    public async Task<Result<PreVendaDto>> AtualizarAsync(Guid id, SalvarPreVendaDto dto, CancellationToken ct = default)
    {
        var erro = await ValidarAsync(dto, ct);
        if (erro is not null)
            return Result.Falhar<PreVendaDto>(erro);

        var existe = await _preVendas.ObterPorIdAsync(id, ct);
        if (existe is null)
            return Result.Falhar<PreVendaDto>(Error.NaoEncontrado("Pré-venda não encontrada."));

        try
        {
            await _preVendas.AtualizarAsync(id, pv =>
            {
                pv.AlterarCabecalho(dto.IdCliente, dto.NomeClienteAvulso,
                    dto.VeiculoMontadora, dto.VeiculoModelo, dto.VeiculoAno, dto.VeiculoPlaca, dto.Observacao);
                pv.DefinirItens(MapearItens(dto.Itens));
                pv.AplicarDescontoGeral(dto.VlrDesconto);
            }, ct);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar<PreVendaDto>(Error.Validacao(ex.Message));
        }

        var atualizada = await _preVendas.ObterPorIdAsync(id, ct);
        return Result.Ok(Mapear(atualizada!));
    }

    public async Task<Result> FaturarAsync(Guid id, Guid idUsuario, CancellationToken ct = default)
    {
        try
        {
            // Fatura e baixa o estoque de todos os itens numa única transação (atômico). O repositório
            // carrega a pré-venda e lança se não existir — sem consulta prévia de existência (evita ler
            // a pré-venda duas vezes).
            await _faturamento.FaturarComBaixaEstoqueAsync(id, idUsuario, ct);
            return Result.Ok();
        }
        catch (NaoEncontradoException ex)
        {
            return Result.Falhar(Error.NaoEncontrado(ex.Message));
        }
        catch (ConcorrenciaException)
        {
            return Result.Falhar(Error.Conflito(
                "O saldo de um produto foi alterado por outro terminal. Recarregue e tente novamente."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            // Invariante violada: pré-venda não-Aberta, sem itens, ou item sem saldo suficiente.
            return Result.Falhar(Error.Validacao(ex.Message));
        }
    }

    public async Task<Result> CancelarAsync(Guid id, CancellationToken ct = default)
    {
        var preVenda = await _preVendas.ObterPorIdAsync(id, ct);
        if (preVenda is null)
            return Result.Falhar(Error.NaoEncontrado("Pré-venda não encontrada."));

        try
        {
            await _preVendas.AtualizarAsync(id, pv => pv.Cancelar(), ct);
            return Result.Ok();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Falhar(Error.Validacao(ex.Message));
        }
    }

    public async Task<IReadOnlyList<PreVendaListaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var preVendas = await _preVendas.ListarAsync(filtro, ct);
        return preVendas.Select(p => new PreVendaListaDto(
            p.Id, p.CodPreVenda, p.Situacao, NomeCliente(p), p.Itens.Count, p.VlrTotal, p.CriadoEm)).ToList();
    }

    public async Task<Result<PreVendaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var preVenda = await _preVendas.ObterPorIdAsync(id, ct);
        return preVenda is null
            ? Result.Falhar<PreVendaDto>(Error.NaoEncontrado("Pré-venda não encontrada."))
            : Result.Ok(Mapear(preVenda));
    }

    /// <summary>Valida o DTO e confere as FKs (cliente quando informado, produtos dos itens). Retorna o erro ou null.</summary>
    private async Task<Error?> ValidarAsync(SalvarPreVendaDto dto, CancellationToken ct)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Error.Validacao(validacao.Errors.Count > 0 ? validacao.Errors[0].ErrorMessage : "Dados inválidos.");

        if (dto.IdCliente is Guid idCliente && await _clientes.ObterPorIdAsync(idCliente, ct) is null)
            return Error.Validacao("Selecione um cliente válido.");

        // Cada item precisa apontar para um produto existente.
        foreach (var item in dto.Itens)
        {
            if (await _produtos.ObterPorIdAsync(item.IdProduto, ct) is null)
                return Error.Validacao($"Produto do item \"{item.DescricaoProduto}\" não encontrado.");
        }

        return null;
    }

    // Converte os DTOs de item em entidades. O total da linha é calculado no construtor da entidade.
    private static IEnumerable<PreVendaItem> MapearItens(IReadOnlyList<PreVendaItemDto> itens) =>
        itens.Select(i => new PreVendaItem(i.IdProduto, i.DescricaoProduto, i.Qtd, i.VlrUnitario, i.VlrDesconto));

    // Nome a exibir na listagem: cliente cadastrado, avulso, ou "Consumidor" quando nenhum.
    private static string NomeCliente(PreVenda p) =>
        p.Cliente?.RazaoSocial ?? p.NomeClienteAvulso ?? "CONSUMIDOR";

    private static PreVendaDto Mapear(PreVenda p) => new(
        p.Id, p.CodPreVenda, p.Situacao, p.IdCliente, p.NomeClienteAvulso,
        p.VeiculoMontadora, p.VeiculoModelo, p.VeiculoAno, p.VeiculoPlaca,
        p.SubtotalItens, p.VlrDesconto, p.VlrTotal, p.Observacao, p.FlgAtivo,
        p.Itens.Select(i => new PreVendaItemDetalheDto(
            i.Id, i.IdProduto, i.DescricaoProduto, i.Qtd, i.VlrUnitario, i.VlrDesconto, i.VlrTotalItem)).ToList());
}
