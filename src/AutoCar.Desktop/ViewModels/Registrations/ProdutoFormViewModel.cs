using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Produtos;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Produto em dois modos (visualização/edição). Novo abre em edição.
/// Categoria é obrigatória; marca e fornecedor são opcionais (combos com item "—").
/// Valores monetários como texto (parse no VM), mesmo padrão de Cliente.
/// </summary>
public partial class ProdutoFormViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private readonly IProdutoService _produtos;
    private readonly ILogger<ProdutoFormViewModel> _logger;
    private Guid? _id;

    // Durante a carga de um produto, a categoria é setada programaticamente; nesse caso o combo de
    // grupo é montado/selecionado manualmente (sem o trigger reativo limpar a seleção).
    private bool _carregandoProduto;

    public ProdutoFormViewModel(IProdutoService produtos, ILogger<ProdutoFormViewModel> logger)
    {
        _produtos = produtos;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    /// <summary>Unidades de medida disponíveis (combo).</summary>
    public IReadOnlyList<UnidadeMedida> Unidades { get; } = Enum.GetValues<UnidadeMedida>();

    /// <summary>Posições para o combo (opcional — inclui item nulo "—"). Cadastro editável.</summary>
    public ObservableCollection<OpcaoDto?> Posicoes { get; } = new();

    /// <summary>Lados para o combo (opcional — inclui item nulo "—"). Cadastro editável.</summary>
    public ObservableCollection<OpcaoDto?> Lados { get; } = new();

    /// <summary>Categorias para o combo (obrigatório). Carregadas ao abrir o form.</summary>
    public ObservableCollection<OpcaoDto> Categorias { get; } = new();

    /// <summary>Grupos da categoria selecionada (opcional — combo DEPENDENTE, inclui item nulo "—").
    /// Recarregado sempre que a categoria muda.</summary>
    public ObservableCollection<OpcaoDto?> Grupos { get; } = new();

    /// <summary>Marcas para o combo (opcional — inclui item nulo "—").</summary>
    public ObservableCollection<OpcaoDto?> Marcas { get; } = new();

    /// <summary>Fornecedores para o combo (opcional — inclui item nulo "—").</summary>
    public ObservableCollection<OpcaoDto?> Fornecedores { get; } = new();

    /// <summary>Aplicações por veículo (mini-grid editável). Salvas junto com o produto.</summary>
    public ObservableCollection<AplicacaoItemViewModel> Aplicacoes { get; } = new();

    /// <summary>Equivalências/cross-reference (mini-grid editável). Salvas junto com o produto.</summary>
    public ObservableCollection<SimilarItemViewModel> Similares { get; } = new();

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private string? _descricaoComplementar;
    [ObservableProperty] private string? _codBarras;
    [ObservableProperty] private string? _codFabricante;
    [ObservableProperty] private UnidadeMedida _unidade = UnidadeMedida.UN;
    [ObservableProperty] private OpcaoDto? _posicaoSelecionada;
    [ObservableProperty] private OpcaoDto? _ladoSelecionado;
    [ObservableProperty] private decimal _vlrCusto;
    [ObservableProperty] private decimal _vlrVenda;
    [ObservableProperty] private OpcaoDto? _categoriaSelecionada;
    [ObservableProperty] private OpcaoDto? _grupoSelecionado;
    [ObservableProperty] private OpcaoDto? _marcaSelecionada;
    [ObservableProperty] private OpcaoDto? _fornecedorSelecionado;

    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    // Aba ativa do formulário. Em vez de empilhar todas as seções (que cortavam no rodapé), o
    // cadastro é dividido em 3 abas: "dados" (identificação + classificação + valores),
    // "aplicacoes" (mini-grid de veículos) e "similares" (cross-reference). Padrão de abas
    // inspirado na tela de Importação do WMS; cores/estilo são os do AutoCar (classe Button.aba).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AbaDadosAtiva))]
    [NotifyPropertyChangedFor(nameof(AbaAplicacoesAtiva))]
    [NotifyPropertyChangedFor(nameof(AbaSimilaresAtiva))]
    private string _abaAtual = "dados";

    public bool AbaDadosAtiva => AbaAtual == "dados";
    public bool AbaAplicacoesAtiva => AbaAtual == "aplicacoes";
    public bool AbaSimilaresAtiva => AbaAtual == "similares";

    [RelayCommand]
    private void SelecionarAba(string aba) => AbaAtual = aba;

    /// <summary>Custo como texto editável (TextBox, padrão do projeto). Parse tolerante BR.</summary>
    public string VlrCustoTexto
    {
        get => VlrCusto.ToString("N2", PtBr);
        set { VlrCusto = ParseMoeda(value); OnPropertyChanged(); }
    }

    /// <summary>Valor de venda como texto editável.</summary>
    public string VlrVendaTexto
    {
        get => VlrVenda.ToString("N2", PtBr);
        set { VlrVenda = ParseMoeda(value); OnPropertyChanged(); }
    }

    public string Titulo => _id is null ? "Novo Produto" : $"Produto {Descricao}";

    /// <summary>Margem sobre o custo: (venda - custo) / custo. Somente leitura; em branco sem base de custo.</summary>
    public string MargemTexto
    {
        get
        {
            if (VlrCusto <= 0) return "";
            var margem = (VlrVenda - VlrCusto) / VlrCusto * 100m;
            return margem.ToString("N1", PtBr) + "%";
        }
    }

    partial void OnVlrCustoChanged(decimal value)
    {
        OnPropertyChanged(nameof(VlrCustoTexto));
        OnPropertyChanged(nameof(MargemTexto));
    }

    partial void OnVlrVendaChanged(decimal value)
    {
        OnPropertyChanged(nameof(VlrVendaTexto));
        OnPropertyChanged(nameof(MargemTexto));
    }

    partial void OnDescricaoChanged(string value) => OnPropertyChanged(nameof(Titulo));

    /// <summary>Prepara o formulário para um novo cadastro (limpo, em edição).</summary>
    public async Task PrepararNovoAsync()
    {
        await CarregarOpcoesAsync();
        _id = null;
        Descricao = string.Empty;
        DescricaoComplementar = CodBarras = CodFabricante = null;
        Unidade = UnidadeMedida.UN;
        PosicaoSelecionada = null;
        LadoSelecionado = null;
        VlrCusto = VlrVenda = 0;
        _carregandoProduto = true;
        CategoriaSelecionada = null;
        Grupos.Clear();
        Grupos.Add(null); // "—" (sem categoria ainda, sem grupos)
        GrupoSelecionado = null;
        _carregandoProduto = false;
        MarcaSelecionada = null;
        FornecedorSelecionado = null;
        Aplicacoes.Clear();
        Similares.Clear();
        MensagemErro = null;
        AbaAtual = "dados";
        ModoVisualizacao = false;
        OnPropertyChanged(nameof(Titulo));
    }

    /// <summary>Carrega um produto existente em modo visualização.</summary>
    public async Task CarregarAsync(Guid id)
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            await CarregarOpcoesAsync();

            var resultado = await _produtos.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var p = resultado.Valor;
            _id = p.Id;
            Descricao = p.Descricao;
            DescricaoComplementar = p.DescricaoComplementar;
            CodBarras = p.CodBarras;
            CodFabricante = p.CodFabricante;
            Unidade = p.Unidade;
            VlrCusto = p.VlrCusto;
            VlrVenda = p.VlrVenda;

            // Selecionar pelo Id dentro da coleção do combo (matching por referência do Avalonia).
            // Grupo é combo dependente: monta a lista da categoria do produto ANTES de selecionar,
            // com o trigger suprimido (senão ele zeraria a seleção que vamos restaurar).
            _carregandoProduto = true;
            CategoriaSelecionada = Categorias.FirstOrDefault(c => c.Id == p.IdCategoria);
            await RecarregarGruposAsync(p.IdCategoria);
            GrupoSelecionado = Grupos.FirstOrDefault(x => x?.Id == p.IdGrupo);
            _carregandoProduto = false;

            MarcaSelecionada = Marcas.FirstOrDefault(m => m?.Id == p.IdMarca);
            FornecedorSelecionado = Fornecedores.FirstOrDefault(f => f?.Id == p.IdFornecedor);
            PosicaoSelecionada = Posicoes.FirstOrDefault(x => x?.Id == p.IdPosicao);
            LadoSelecionado = Lados.FirstOrDefault(x => x?.Id == p.IdLado);

            Aplicacoes.Clear();
            foreach (var a in p.Aplicacoes)
                Aplicacoes.Add(new AplicacaoItemViewModel(
                    a.Montadora, a.Modelo, a.AnoInicio, a.AnoFim, a.Motorizacao, a.Combustivel, a.Observacao));

            Similares.Clear();
            foreach (var s in p.Similares)
                Similares.Add(new SimilarItemViewModel(s.Marca, s.CodReferencia, s.IdProdutoEquivalente, s.Observacao));

            AbaAtual = "dados";
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o produto.";
            _logger.LogError(ex, "Erro ao carregar produto {Id}.", id);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void HabilitarEdicao() => ModoVisualizacao = false;

    [RelayCommand]
    private async Task SalvarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            if (CategoriaSelecionada is null)
            {
                MensagemErro = "Selecione a categoria do produto.";
                return;
            }

            // Descarta linhas vazias; o service normaliza CAIXA ALTA e grava substituindo as antigas.
            var aplicacoes = Aplicacoes
                .Where(a => a.TemConteudo)
                .Select(a => new AplicacaoDto(
                    a.Montadora, a.Modelo, a.AnoInicio, a.AnoFim, a.Motorizacao, a.Combustivel, a.Observacao))
                .ToList();

            // Equivalências: descarta linhas incompletas; preserva o vínculo automático já resolvido.
            var similares = Similares
                .Where(s => s.TemConteudo)
                .Select(s => new SimilarDto(s.Marca, s.CodReferencia, s.IdProdutoEquivalente, s.Observacao))
                .ToList();

            var dto = new SalvarProdutoDto(
                CategoriaSelecionada.Id, Descricao, DescricaoComplementar, CodBarras,
                CodFabricante, Unidade, PosicaoSelecionada?.Id, LadoSelecionado?.Id, VlrCusto, VlrVenda,
                MarcaSelecionada?.Id, FornecedorSelecionado?.Id, GrupoSelecionado?.Id, aplicacoes, similares);

            var resultado = _id is null
                ? await _produtos.CriarAsync(dto)
                : await _produtos.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar o produto.";
            _logger.LogError(ex, "Erro ao salvar produto.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void AdicionarAplicacao() => Aplicacoes.Add(new AplicacaoItemViewModel());

    [RelayCommand]
    private void RemoverAplicacao(AplicacaoItemViewModel? item)
    {
        if (item is not null)
            Aplicacoes.Remove(item);
    }

    [RelayCommand]
    private void AdicionarSimilar() => Similares.Add(new SimilarItemViewModel());

    [RelayCommand]
    private void RemoverSimilar(SimilarItemViewModel? item)
    {
        if (item is not null)
            Similares.Remove(item);
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();

    // Combo dependente: quando o usuário troca a categoria, o grupo anterior pode não pertencer mais
    // a ela. Recarrega a lista de grupos e zera a seleção. Suprimido durante a carga de um produto
    // (lá o grupo é restaurado manualmente pelo Id salvo, em CarregarAsync).
    partial void OnCategoriaSelecionadaChanged(OpcaoDto? value)
    {
        if (_carregandoProduto) return;
        _ = AtualizarGruposPorCategoriaAsync(value?.Id);
    }

    private async Task AtualizarGruposPorCategoriaAsync(Guid? idCategoria)
    {
        // Disparado por troca de categoria (fire-and-forget no trigger): trata o erro aqui, senão
        // uma falha na consulta de grupos viraria exceção não observada. Grupo é opcional, então
        // degrada para lista vazia e avisa.
        try
        {
            await RecarregarGruposAsync(idCategoria);
            GrupoSelecionado = null;
        }
        catch (Exception ex)
        {
            Grupos.Clear();
            Grupos.Add(null);
            GrupoSelecionado = null;
            MensagemErro = "Falha ao carregar os grupos da categoria.";
            _logger.LogError(ex, "Erro ao recarregar grupos da categoria {IdCategoria}.", idCategoria);
        }
    }

    // Monta a lista de grupos da categoria (com "—" no topo). Sem categoria, fica só o "—".
    private async Task RecarregarGruposAsync(Guid? idCategoria)
    {
        Grupos.Clear();
        Grupos.Add(null); // "—" (sem grupo)
        if (idCategoria is Guid cat)
        {
            var grupos = await _produtos.ListarGruposAsync(cat);
            foreach (var g in grupos) Grupos.Add(g);
        }
    }

    // Carrega as opções dos combos. Marca/Fornecedor começam com item nulo (opção "—").
    private async Task CarregarOpcoesAsync()
    {
        var categorias = await _produtos.ListarCategoriasAsync();
        var marcas = await _produtos.ListarMarcasAsync();
        var fornecedores = await _produtos.ListarFornecedoresAsync();
        var posicoes = await _produtos.ListarPosicoesAsync();
        var lados = await _produtos.ListarLadosAsync();

        Categorias.Clear();
        foreach (var c in categorias) Categorias.Add(c);

        Marcas.Clear();
        Marcas.Add(null); // "—" (sem marca)
        foreach (var m in marcas) Marcas.Add(m);

        Fornecedores.Clear();
        Fornecedores.Add(null); // "—" (sem fornecedor)
        foreach (var f in fornecedores) Fornecedores.Add(f);

        Posicoes.Clear();
        Posicoes.Add(null); // "—" (sem posição)
        foreach (var p in posicoes) Posicoes.Add(p);

        Lados.Clear();
        Lados.Add(null); // "—" (sem lado)
        foreach (var l in lados) Lados.Add(l);
    }

    private static decimal ParseMoeda(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }
}
