using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Domain.Entities;

/// <summary>
/// Produto (peça/item) do AutoCar. Tabela mestre: id + cod_produto. Categoria é
/// obrigatória (organiza e permite buscar a peça); marca e fornecedor são opcionais
/// (item genérico pode não ter marca; o fornecedor pode variar por compra).
/// Saldo de estoque NÃO mora aqui — fica no módulo de Estoque (Fase 3).
/// </summary>
public class Produto : EntidadeBase
{
    // Construtor protegido para o EF Core materializar a entidade.
    protected Produto() { }

    public Produto(
        Guid idCategoria,
        string descricao,
        string? descricaoComplementar,
        string? codBarras,
        string? codFabricante,
        UnidadeMedida unidade,
        PosicaoPeca posicao,
        decimal vlrCusto,
        decimal vlrVenda,
        Guid? idMarca,
        Guid? idFornecedor)
    {
        IdCategoria = idCategoria;
        Descricao = descricao.Trim().ToUpperInvariant();
        DescricaoComplementar = Normalizar(descricaoComplementar);
        CodBarras = NormalizarCodigo(codBarras);
        CodFabricante = NormalizarCodigo(codFabricante);
        Unidade = unidade;
        Posicao = posicao;
        VlrCusto = vlrCusto;
        VlrVenda = vlrVenda;
        IdMarca = idMarca;
        IdFornecedor = idFornecedor;
        FlgAtivo = true;
    }

    /// <summary>Código legível autoincrement, gerado pelo banco.</summary>
    public int CodProduto { get; protected set; }

    /// <summary>Código de barras (EAN/GTIN). Opcional; único quando informado.</summary>
    public string? CodBarras { get; protected set; }

    public string Descricao { get; protected set; } = string.Empty;

    public string? DescricaoComplementar { get; protected set; }

    /// <summary>Código do produto no fabricante (referência da peça).</summary>
    public string? CodFabricante { get; protected set; }

    public UnidadeMedida Unidade { get; protected set; }

    /// <summary>Posição/eixo da peça (dianteira/traseira). NaoAplica para peças sem distinção.</summary>
    public PosicaoPeca Posicao { get; protected set; }

    public decimal VlrCusto { get; protected set; }

    public decimal VlrVenda { get; protected set; }

    // --- FKs ---

    /// <summary>Categoria (obrigatória).</summary>
    public Guid IdCategoria { get; protected set; }

    /// <summary>Marca/fabricante (opcional).</summary>
    public Guid? IdMarca { get; protected set; }

    /// <summary>Fornecedor preferencial (opcional).</summary>
    public Guid? IdFornecedor { get; protected set; }

    // Navegações (carregadas só quando necessário, ex: exibição com nomes).
    public CategoriaProduto? Categoria { get; protected set; }
    public Marca? Marca { get; protected set; }
    public Fornecedor? Fornecedor { get; protected set; }

    // Aplicações por veículo (1:N). Backing field para expor como somente leitura.
    private readonly List<ProdutoAplicacao> _aplicacoes = new();

    /// <summary>Veículos em que este produto se aplica (montadora/modelo/ano). Somente leitura;
    /// alterar via <see cref="DefinirAplicacoes"/>.</summary>
    public IReadOnlyList<ProdutoAplicacao> Aplicacoes => _aplicacoes;

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(
        Guid idCategoria,
        string descricao,
        string? descricaoComplementar,
        string? codBarras,
        string? codFabricante,
        UnidadeMedida unidade,
        PosicaoPeca posicao,
        decimal vlrCusto,
        decimal vlrVenda,
        Guid? idMarca,
        Guid? idFornecedor)
    {
        IdCategoria = idCategoria;
        Descricao = descricao.Trim().ToUpperInvariant();
        DescricaoComplementar = Normalizar(descricaoComplementar);
        CodBarras = NormalizarCodigo(codBarras);
        CodFabricante = NormalizarCodigo(codFabricante);
        Unidade = unidade;
        Posicao = posicao;
        VlrCusto = vlrCusto;
        VlrVenda = vlrVenda;
        IdMarca = idMarca;
        IdFornecedor = idFornecedor;
        MarcarAtualizada();
    }

    /// <summary>Substitui todas as aplicações por veículo (padrão "salva junto" — o form envia a
    /// lista completa a cada gravação). O EF remove as antigas (Cascade) e insere as novas.</summary>
    public void DefinirAplicacoes(IEnumerable<ProdutoAplicacao> aplicacoes)
    {
        _aplicacoes.Clear();
        _aplicacoes.AddRange(aplicacoes);
        MarcarAtualizada();
    }

    public void Ativar()
    {
        FlgAtivo = true;
        MarcarAtualizada();
    }

    public void Inativar()
    {
        FlgAtivo = false;
        MarcarAtualizada();
    }

    // Texto descritivo livre em CAIXA ALTA (padrão do projeto); nulo se vazio.
    private static string? Normalizar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim().ToUpperInvariant();

    // Códigos (barras/fabricante) só normalizam espaços — preservam o conteúdo digitado.
    private static string? NormalizarCodigo(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
