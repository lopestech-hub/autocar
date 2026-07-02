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
        Guid? idPosicao,
        Guid? idLado,
        decimal vlrCusto,
        decimal vlrVenda,
        Guid? idMarca,
        Guid? idFornecedor,
        Guid? idGrupo,
        string? arquivoImagem = null)
    {
        IdCategoria = idCategoria;
        Descricao = descricao.Trim().ToUpperInvariant();
        DescricaoComplementar = Normalizar(descricaoComplementar);
        CodBarras = NormalizarCodigo(codBarras);
        CodFabricante = NormalizarCodigo(codFabricante);
        Unidade = unidade;
        IdPosicao = idPosicao;
        IdLado = idLado;
        VlrCusto = vlrCusto;
        VlrVenda = vlrVenda;
        IdMarca = idMarca;
        IdFornecedor = idFornecedor;
        IdGrupo = idGrupo;
        ArquivoImagem = NormalizarCodigo(arquivoImagem);
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

    public decimal VlrCusto { get; protected set; }

    public decimal VlrVenda { get; protected set; }

    /// <summary>Nome do arquivo de imagem do produto (ex: "27022.jpg"). Opcional. NÃO guarda caminho
    /// nem URL — só o nome; a pasta-base é configurável por terminal (appsettings: Imagens:PastaBase),
    /// porque o sistema é 2-tier multi-terminal e o caminho físico varia entre as máquinas da LAN.</summary>
    public string? ArquivoImagem { get; protected set; }

    // --- FKs ---

    /// <summary>Categoria (obrigatória).</summary>
    public Guid IdCategoria { get; protected set; }

    /// <summary>Marca/fabricante (opcional).</summary>
    public Guid? IdMarca { get; protected set; }

    /// <summary>Fornecedor preferencial (opcional).</summary>
    public Guid? IdFornecedor { get; protected set; }

    /// <summary>Grupo/família dentro da categoria (Amortecedor, Pastilha...). Opcional. Cadastro
    /// editável que pertence à categoria (ver <see cref="GrupoProduto"/>). Nível Categoria → Grupo → Produto.</summary>
    public Guid? IdGrupo { get; protected set; }

    /// <summary>Posição/eixo da peça (Dianteira/Traseira...). Opcional — peça sem distinção de eixo
    /// (óleo, filtro) não tem posição. Cadastro editável (ver <see cref="PosicaoPeca"/>).</summary>
    public Guid? IdPosicao { get; protected set; }

    /// <summary>Lado da peça (Esquerdo/Direito...). Opcional — peça sem distinção de lado não tem lado.
    /// Cadastro editável (ver <see cref="LadoPeca"/>).</summary>
    public Guid? IdLado { get; protected set; }

    /// <summary>Fonte/procedência do dado desta peça de referência (de qual catálogo e por qual método
    /// foi extraída — ver <see cref="FonteDado"/>). Opcional. NÃO confundir com marca (fabricante da
    /// peça): a fonte diz de onde o dado veio. Populada pelo carregador do automacao_catalogo; sem UI
    /// no formulário do Produto por ora.</summary>
    public Guid? IdFonte { get; protected set; }

    // Navegações (carregadas só quando necessário, ex: exibição com nomes).
    public CategoriaProduto? Categoria { get; protected set; }
    public Marca? Marca { get; protected set; }
    public Fornecedor? Fornecedor { get; protected set; }
    public GrupoProduto? Grupo { get; protected set; }
    public PosicaoPeca? Posicao { get; protected set; }
    public LadoPeca? Lado { get; protected set; }
    public FonteDado? Fonte { get; protected set; }

    // Aplicações por veículo (1:N). Backing field para expor como somente leitura.
    private readonly List<ProdutoAplicacao> _aplicacoes = new();

    /// <summary>Veículos em que este produto se aplica (montadora/modelo/ano). Somente leitura;
    /// alterar via <see cref="DefinirAplicacoes"/>.</summary>
    public IReadOnlyList<ProdutoAplicacao> Aplicacoes => _aplicacoes;

    // Equivalências/cruzamento com peças de outras marcas (1:N). Backing field somente leitura.
    private readonly List<ProdutoSimilar> _similares = new();

    /// <summary>Equivalências (cross-reference) com peças de outras marcas. Somente leitura;
    /// alterar via <see cref="DefinirSimilares"/>.</summary>
    public IReadOnlyList<ProdutoSimilar> Similares => _similares;

    public bool FlgAtivo { get; protected set; }

    public void AlterarDados(
        Guid idCategoria,
        string descricao,
        string? descricaoComplementar,
        string? codBarras,
        string? codFabricante,
        UnidadeMedida unidade,
        Guid? idPosicao,
        Guid? idLado,
        decimal vlrCusto,
        decimal vlrVenda,
        Guid? idMarca,
        Guid? idFornecedor,
        Guid? idGrupo,
        string? arquivoImagem = null)
    {
        IdCategoria = idCategoria;
        Descricao = descricao.Trim().ToUpperInvariant();
        DescricaoComplementar = Normalizar(descricaoComplementar);
        CodBarras = NormalizarCodigo(codBarras);
        CodFabricante = NormalizarCodigo(codFabricante);
        Unidade = unidade;
        IdPosicao = idPosicao;
        IdLado = idLado;
        VlrCusto = vlrCusto;
        VlrVenda = vlrVenda;
        IdMarca = idMarca;
        IdFornecedor = idFornecedor;
        IdGrupo = idGrupo;
        ArquivoImagem = NormalizarCodigo(arquivoImagem);
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

    /// <summary>Substitui todas as equivalências (mesmo padrão "salva junto" das aplicações — o
    /// form envia a lista completa a cada gravação).</summary>
    public void DefinirSimilares(IEnumerable<ProdutoSimilar> similares)
    {
        _similares.Clear();
        _similares.AddRange(similares);
        MarcarAtualizada();
    }

    /// <summary>Define (ou limpa) o grupo do produto isoladamente — útil para vínculo em lote/seed
    /// sem reenviar os demais campos.</summary>
    public void DefinirGrupo(Guid? idGrupo)
    {
        IdGrupo = idGrupo;
        MarcarAtualizada();
    }

    /// <summary>Define (ou limpa) a fonte/procedência do dado isoladamente — útil para atribuir a origem
    /// em lote na carga do catálogo sem reenviar os demais campos.</summary>
    public void DefinirFonte(Guid? idFonte)
    {
        IdFonte = idFonte;
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
