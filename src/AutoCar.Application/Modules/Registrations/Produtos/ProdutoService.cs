using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using AutoCar.Shared.Results;
using FluentValidation;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>
/// CRUD de Produto. Valida o DTO (FluentValidation), confere a categoria (obrigatória)
/// e garante código de barras único quando informado. Marca/fornecedor são opcionais.
/// </summary>
public sealed class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _produtos;
    private readonly ICategoriaProdutoRepository _categorias;
    private readonly IMarcaRepository _marcas;
    private readonly IFornecedorRepository _fornecedores;
    private readonly IValidator<SalvarProdutoDto> _validator;

    public ProdutoService(
        IProdutoRepository produtos,
        ICategoriaProdutoRepository categorias,
        IMarcaRepository marcas,
        IFornecedorRepository fornecedores,
        IValidator<SalvarProdutoDto> validator)
    {
        _produtos = produtos;
        _categorias = categorias;
        _marcas = marcas;
        _fornecedores = fornecedores;
        _validator = validator;
    }

    public async Task<Result<ProdutoDto>> CriarAsync(SalvarProdutoDto dto, CancellationToken ct = default)
    {
        var erro = await ValidarAsync(dto, null, ct);
        if (erro is not null)
            return Result.Falhar<ProdutoDto>(erro);

        var produto = new Produto(
            dto.IdCategoria, dto.Descricao, dto.DescricaoComplementar, dto.CodBarras,
            dto.CodFabricante, dto.Unidade, dto.Posicao, dto.Lado, dto.VlrCusto, dto.VlrVenda, dto.IdMarca, dto.IdFornecedor);
        produto.DefinirAplicacoes(MapearAplicacoes(dto.Aplicacoes));

        await _produtos.AdicionarAsync(produto, ct);

        return Result.Ok(Mapear(produto));
    }

    public async Task<Result<ProdutoDto>> AtualizarAsync(Guid id, SalvarProdutoDto dto, CancellationToken ct = default)
    {
        var erro = await ValidarAsync(dto, id, ct);
        if (erro is not null)
            return Result.Falhar<ProdutoDto>(erro);

        var existe = await _produtos.ObterPorIdAsync(id, ct);
        if (existe is null)
            return Result.Falhar<ProdutoDto>(Error.NaoEncontrado("Produto não encontrado."));

        // O repositório carrega o produto rastreado num contexto próprio, aplica a alteração e salva.
        await _produtos.AtualizarAsync(id, produto =>
        {
            produto.AlterarDados(
                dto.IdCategoria, dto.Descricao, dto.DescricaoComplementar, dto.CodBarras,
                dto.CodFabricante, dto.Unidade, dto.Posicao, dto.Lado, dto.VlrCusto, dto.VlrVenda, dto.IdMarca, dto.IdFornecedor);
            produto.DefinirAplicacoes(MapearAplicacoes(dto.Aplicacoes));
        }, ct);

        var atualizado = await _produtos.ObterPorIdAsync(id, ct);
        return Result.Ok(Mapear(atualizado!));
    }

    public async Task<Result> InativarAsync(Guid id, CancellationToken ct = default)
    {
        var produto = await _produtos.ObterPorIdAsync(id, ct);
        if (produto is null)
            return Result.Falhar(Error.NaoEncontrado("Produto não encontrado."));

        await _produtos.AtualizarAsync(id, p => p.Inativar(), ct);
        return Result.Ok();
    }

    public async Task<IReadOnlyList<ProdutoListaDto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        var produtos = await _produtos.ListarAsync(filtro, ct);
        return produtos.Select(p => new ProdutoListaDto(
            p.Id, p.CodProduto, p.Descricao, p.CodBarras,
            p.Categoria?.Descricao, p.Marca?.Descricao, p.Unidade, p.Posicao, p.Lado, p.VlrVenda, p.FlgAtivo)).ToList();
    }

    public async Task<Result<ProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var produto = await _produtos.ObterPorIdAsync(id, ct);
        return produto is null
            ? Result.Falhar<ProdutoDto>(Error.NaoEncontrado("Produto não encontrado."))
            : Result.Ok(Mapear(produto));
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarCategoriasAsync(CancellationToken ct = default)
    {
        var lista = await _categorias.ListarAsync(null, ct);
        return lista.Select(c => new OpcaoDto(c.Id, c.Descricao)).ToList();
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarMarcasAsync(CancellationToken ct = default)
    {
        var lista = await _marcas.ListarAsync(null, ct);
        return lista.Select(m => new OpcaoDto(m.Id, m.Descricao)).ToList();
    }

    public async Task<IReadOnlyList<OpcaoDto>> ListarFornecedoresAsync(CancellationToken ct = default)
    {
        var lista = await _fornecedores.ListarAsync(null, ct);
        return lista.Select(f => new OpcaoDto(f.Id, f.RazaoSocial)).ToList();
    }

    public async Task<IReadOnlyList<CatalogoItemDto>> BuscarCatalogoAsync(BuscaCatalogoDto filtro, CancellationToken ct = default)
    {
        var produtos = await _produtos.BuscarPorVeiculoAsync(
            filtro.Termo, filtro.Montadora, filtro.Modelo, filtro.Ano, ct);

        return produtos.Select(p => new CatalogoItemDto(
            p.Id, p.CodProduto, p.Descricao, p.Categoria?.Descricao, p.Marca?.Descricao,
            p.Unidade, p.Posicao, p.Lado, p.CodFabricante, p.VlrVenda, ResumirAplicacoes(p))).ToList();
    }

    public Task<IReadOnlyList<string>> ListarMontadorasAsync(CancellationToken ct = default) =>
        _produtos.ListarMontadorasAsync(ct);

    public Task<IReadOnlyList<string>> ListarModelosAsync(string? montadora, CancellationToken ct = default) =>
        _produtos.ListarModelosAsync(montadora, ct);

    // Resume as aplicações do produto numa linha legível: "GOL 2008-2014; PARATI 2005-2018".
    private static string ResumirAplicacoes(Produto p) =>
        string.Join("; ", p.Aplicacoes.Select(a =>
        {
            var anos = (a.AnoInicio, a.AnoFim) switch
            {
                (null, null) => "",
                (var ini, null) => $" {ini}+",
                (null, var fim) => $" até {fim}",
                var (ini, fim) => $" {ini}-{fim}",
            };
            return $"{a.Modelo}{anos}";
        }));

    /// <summary>Valida o DTO, confere a categoria e a unicidade do código de barras. Retorna o erro ou null.</summary>
    private async Task<Error?> ValidarAsync(SalvarProdutoDto dto, Guid? idAtual, CancellationToken ct)
    {
        var validacao = await _validator.ValidateAsync(dto, ct);
        if (!validacao.IsValid)
            return Error.Validacao(validacao.Errors.Count > 0 ? validacao.Errors[0].ErrorMessage : "Dados inválidos.");

        if (await _categorias.ObterPorIdAsync(dto.IdCategoria, ct) is null)
            return Error.Validacao("Selecione uma categoria válida.");

        // Código de barras é único só quando informado (índice único parcial no banco).
        var codBarras = dto.CodBarras?.Trim();
        if (!string.IsNullOrEmpty(codBarras) &&
            await _produtos.ExisteCodBarrasAsync(codBarras, idAtual, ct))
            return Error.Conflito("Já existe um produto com este código de barras.");

        return null;
    }

    // Converte os DTOs de aplicação em entidades (linhas vazias são descartadas).
    private static IEnumerable<ProdutoAplicacao> MapearAplicacoes(IReadOnlyList<AplicacaoDto> aplicacoes) =>
        aplicacoes
            .Where(a => !string.IsNullOrWhiteSpace(a.Montadora) || !string.IsNullOrWhiteSpace(a.Modelo))
            .Select(a => new ProdutoAplicacao(
                a.Montadora, a.Modelo, a.AnoInicio, a.AnoFim, a.Motorizacao, a.Combustivel, a.Observacao));

    private static ProdutoDto Mapear(Produto p) => new(
        p.Id, p.CodProduto, p.IdCategoria, p.Descricao, p.DescricaoComplementar, p.CodBarras,
        p.CodFabricante, p.Unidade, p.Posicao, p.Lado, p.VlrCusto, p.VlrVenda, p.IdMarca, p.IdFornecedor, p.FlgAtivo,
        p.Aplicacoes.Select(a => new AplicacaoDto(
            a.Montadora, a.Modelo, a.AnoInicio, a.AnoFim, a.Motorizacao, a.Combustivel, a.Observacao)).ToList());
}
