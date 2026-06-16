using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Shared.Results;

namespace AutoCar.Application.Modules.Registrations.Produtos;

/// <summary>Casos de uso do cadastro de Produto (CRUD + opções para os combos).</summary>
public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoListaDto>> ListarAsync(string? filtro, CancellationToken ct = default);
    Task<Result<ProdutoDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ProdutoDto>> CriarAsync(SalvarProdutoDto dto, CancellationToken ct = default);
    Task<Result<ProdutoDto>> AtualizarAsync(Guid id, SalvarProdutoDto dto, CancellationToken ct = default);
    Task<Result> InativarAsync(Guid id, CancellationToken ct = default);

    /// <summary>Categorias ativas para o combo (obrigatório no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarCategoriasAsync(CancellationToken ct = default);

    /// <summary>Marcas ativas para o combo (opcional no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarMarcasAsync(CancellationToken ct = default);

    /// <summary>Fornecedores ativos para o combo (opcional no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarFornecedoresAsync(CancellationToken ct = default);

    /// <summary>Posições ativas para o combo (opcional no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarPosicoesAsync(CancellationToken ct = default);

    /// <summary>Lados ativos para o combo (opcional no produto).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarLadosAsync(CancellationToken ct = default);

    /// <summary>Grupos ativos de uma categoria (combo dependente — só faz sentido após escolher a categoria).</summary>
    Task<IReadOnlyList<OpcaoDto>> ListarGruposAsync(Guid idCategoria, CancellationToken ct = default);

    /// <summary>Busca de peças por veículo (tela Catálogo). Cruza produto × aplicação;
    /// todos os filtros são opcionais e vão estreitando o resultado.</summary>
    Task<IReadOnlyList<CatalogoItemDto>> BuscarCatalogoAsync(BuscaCatalogoDto filtro, CancellationToken ct = default);

    /// <summary>Montadoras distintas já cadastradas em aplicações (autocomplete do Catálogo).</summary>
    Task<IReadOnlyList<string>> ListarMontadorasAsync(CancellationToken ct = default);

    /// <summary>Modelos distintos já cadastrados (autocomplete); filtra por montadora se informada.</summary>
    Task<IReadOnlyList<string>> ListarModelosAsync(string? montadora, CancellationToken ct = default);
}
