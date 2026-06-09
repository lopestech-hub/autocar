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

    public ProdutoFormViewModel(IProdutoService produtos, ILogger<ProdutoFormViewModel> logger)
    {
        _produtos = produtos;
        _logger = logger;
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    /// <summary>Unidades de medida disponíveis (combo).</summary>
    public IReadOnlyList<UnidadeMedida> Unidades { get; } = Enum.GetValues<UnidadeMedida>();

    /// <summary>Posições/eixo disponíveis (combo). NaoAplica é o padrão (peça sem eixo).</summary>
    public IReadOnlyList<PosicaoPeca> Posicoes { get; } = Enum.GetValues<PosicaoPeca>();

    /// <summary>Lados disponíveis (combo). NaoAplica é o padrão (peça sem distinção de lado).</summary>
    public IReadOnlyList<LadoPeca> Lados { get; } = Enum.GetValues<LadoPeca>();

    /// <summary>Categorias para o combo (obrigatório). Carregadas ao abrir o form.</summary>
    public ObservableCollection<OpcaoDto> Categorias { get; } = new();

    /// <summary>Marcas para o combo (opcional — inclui item nulo "—").</summary>
    public ObservableCollection<OpcaoDto?> Marcas { get; } = new();

    /// <summary>Fornecedores para o combo (opcional — inclui item nulo "—").</summary>
    public ObservableCollection<OpcaoDto?> Fornecedores { get; } = new();

    /// <summary>Aplicações por veículo (mini-grid editável). Salvas junto com o produto.</summary>
    public ObservableCollection<AplicacaoItemViewModel> Aplicacoes { get; } = new();

    [ObservableProperty] private string _descricao = string.Empty;
    [ObservableProperty] private string? _descricaoComplementar;
    [ObservableProperty] private string? _codBarras;
    [ObservableProperty] private string? _codFabricante;
    [ObservableProperty] private UnidadeMedida _unidade = UnidadeMedida.UN;
    [ObservableProperty] private PosicaoPeca _posicao = PosicaoPeca.NaoAplica;
    [ObservableProperty] private LadoPeca _lado = LadoPeca.NaoAplica;
    [ObservableProperty] private decimal _vlrCusto;
    [ObservableProperty] private decimal _vlrVenda;
    [ObservableProperty] private OpcaoDto? _categoriaSelecionada;
    [ObservableProperty] private OpcaoDto? _marcaSelecionada;
    [ObservableProperty] private OpcaoDto? _fornecedorSelecionado;

    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

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

    /// <summary>Margem sobre o custo: (venda - custo) / custo. Texto somente leitura ("—" sem base).</summary>
    public string MargemTexto
    {
        get
        {
            if (VlrCusto <= 0) return "—";
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
        Posicao = PosicaoPeca.NaoAplica;
        Lado = LadoPeca.NaoAplica;
        VlrCusto = VlrVenda = 0;
        CategoriaSelecionada = null;
        MarcaSelecionada = null;
        FornecedorSelecionado = null;
        Aplicacoes.Clear();
        MensagemErro = null;
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
            Posicao = p.Posicao;
            Lado = p.Lado;
            VlrCusto = p.VlrCusto;
            VlrVenda = p.VlrVenda;

            // Selecionar pelo Id dentro da coleção do combo (matching por referência do Avalonia).
            CategoriaSelecionada = Categorias.FirstOrDefault(c => c.Id == p.IdCategoria);
            MarcaSelecionada = Marcas.FirstOrDefault(m => m?.Id == p.IdMarca);
            FornecedorSelecionado = Fornecedores.FirstOrDefault(f => f?.Id == p.IdFornecedor);

            Aplicacoes.Clear();
            foreach (var a in p.Aplicacoes)
                Aplicacoes.Add(new AplicacaoItemViewModel(
                    a.Montadora, a.Modelo, a.AnoInicio, a.AnoFim, a.Motorizacao, a.Combustivel, a.Observacao));

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

            var dto = new SalvarProdutoDto(
                CategoriaSelecionada.Id, Descricao, DescricaoComplementar, CodBarras,
                CodFabricante, Unidade, Posicao, Lado, VlrCusto, VlrVenda,
                MarcaSelecionada?.Id, FornecedorSelecionado?.Id, aplicacoes);

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
    private void Cancelar() => Cancelado?.Invoke();

    // Carrega as opções dos combos. Marca/Fornecedor começam com item nulo (opção "—").
    private async Task CarregarOpcoesAsync()
    {
        var categorias = await _produtos.ListarCategoriasAsync();
        var marcas = await _produtos.ListarMarcasAsync();
        var fornecedores = await _produtos.ListarFornecedoresAsync();

        Categorias.Clear();
        foreach (var c in categorias) Categorias.Add(c);

        Marcas.Clear();
        Marcas.Add(null); // "—" (sem marca)
        foreach (var m in marcas) Marcas.Add(m);

        Fornecedores.Clear();
        Fornecedores.Add(null); // "—" (sem fornecedor)
        foreach (var f in fornecedores) Fornecedores.Add(f);
    }

    private static decimal ParseMoeda(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }
}
