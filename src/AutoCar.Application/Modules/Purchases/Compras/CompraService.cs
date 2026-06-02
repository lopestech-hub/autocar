using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Purchases.Compras;

/// <summary>
/// Casos de uso da Compra. Registra um documento de compra de um fornecedor e dá ENTRADA no estoque de
/// cada item numa transação única (delegada ao repositório). Valida o fornecedor e os produtos (FK), e
/// guarda um snapshot da descrição de cada produto. Converte invariantes do domínio e conflito de
/// concorrência em <see cref="Result"/> — a UI nunca recebe exceção. Não atualiza o custo do produto (MVP).
/// </summary>
public sealed class CompraService : ICompraService
{
    private readonly ICompraRepository _compras;
    private readonly IFornecedorRepository _fornecedores;
    private readonly IProdutoRepository _produtos;
    private readonly IValidator<CriarCompraDto> _validator;

    public CompraService(
        ICompraRepository compras,
        IFornecedorRepository fornecedores,
        IProdutoRepository produtos,
        IValidator<CriarCompraDto> validator)
    {
        _compras = compras;
        _fornecedores = fornecedores;
        _produtos = produtos;
        _validator = validator;
    }

    public async Task<Result<CompraDto>> CriarAsync(
        Guid idUsuario, CriarCompraDto dto, CancellationToken ct = default)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Result.Falhar<CompraDto>(Error.Validacao(
                validacao.Errors.Count > 0 ? validacao.Errors[0].ErrorMessage : "Dados inválidos."));

        var fornecedor = await _fornecedores.ObterPorIdAsync(dto.IdFornecedor, ct);
        if (fornecedor is null)
            return Result.Falhar<CompraDto>(Error.NaoEncontrado("Fornecedor não encontrado."));

        // Monta os itens com snapshot da descrição do produto (validando que cada produto existe).
        var itensCompra = new List<CompraItem>();
        foreach (var item in dto.Itens)
        {
            var produto = await _produtos.ObterPorIdAsync(item.IdProduto, ct);
            if (produto is null)
                return Result.Falhar<CompraDto>(Error.Validacao("Há um item com produto inexistente."));

            itensCompra.Add(new CompraItem(produto.Id, produto.Descricao, item.Qtd, item.VlrCustoUnitario));
        }

        try
        {
            var compra = new Compra(dto.IdFornecedor, idUsuario, dto.NumDocumento, dto.Observacao);
            compra.DefinirItens(itensCompra);

            await _compras.RegistrarComEntradaEstoqueAsync(compra, ct);

            return Result.Ok(new CompraDto(
                compra.Id, compra.CodCompra, compra.IdFornecedor, compra.NumDocumento, compra.VlrTotal));
        }
        catch (ConcorrenciaException)
        {
            return Result.Falhar<CompraDto>(Error.Conflito(
                "O saldo de um produto foi alterado por outro terminal. Recarregue e tente novamente."));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return Result.Falhar<CompraDto>(Error.Validacao(ex.Message));
        }
    }

    public async Task<Result<IReadOnlyList<CompraListaDto>>> ListarAsync(CancellationToken ct = default)
    {
        var compras = await _compras.ListarAsync(ct);

        // O nome do fornecedor já vem carregado por navegação no repositório.
        var lista = compras
            .Select(c => new CompraListaDto(
                c.Id,
                c.CodCompra,
                NomeFornecedor(c),
                c.Itens.Count,
                c.VlrTotal,
                c.CriadoEm))
            .ToList();

        return Result.Ok<IReadOnlyList<CompraListaDto>>(lista);
    }

    // Nome de exibição do fornecedor: fantasia quando houver, senão razão social.
    private static string NomeFornecedor(Compra compra)
    {
        var f = compra.Fornecedor;
        if (f is null) return "—";
        return string.IsNullOrWhiteSpace(f.NomeFantasia) ? f.RazaoSocial : f.NomeFantasia!;
    }
}
