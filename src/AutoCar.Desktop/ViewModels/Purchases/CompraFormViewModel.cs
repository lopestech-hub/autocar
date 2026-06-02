using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Fornecedores.DTOs;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Application.Modules.Purchases.Compras;
using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Purchases;

/// <summary>
/// Formulário de Compra em dois modos (edição/visualização). Cabeçalho com fornecedor obrigatório
/// (seletor por janela via F3), nº do documento e observação; grid de itens editável alimentado pelo
/// Catálogo (aberto numa janela separada via F2). Total = soma das linhas (qtd × custo). Ao salvar,
/// dá entrada no estoque numa transação única (delegada ao service/repositório). Compra registrada
/// reabre só em visualização (já consumou a entrada — não se edita no MVP).
/// </summary>
public partial class CompraFormViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private readonly ICompraService _compras;
    private readonly ILogger<CompraFormViewModel> _logger;

    private Guid? _id;
    private Guid _idUsuario;

    public CompraFormViewModel(ICompraService compras, ILogger<CompraFormViewModel> logger)
    {
        _compras = compras;
        _logger = logger;

        Itens.CollectionChanged += (_, _) => RecalcularTotais();
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    /// <summary>Disparado ao pedir o catálogo (F2). A janela da compra abre a janela seletora de
    /// peças (Catálogo) e devolve a escolhida via <see cref="AdicionarPecaDoCatalogo"/>.</summary>
    public event Action? AbrirCatalogoSolicitado;

    /// <summary>Disparado ao pedir o seletor de fornecedor (F3 / clique no campo). A janela da compra
    /// abre a janela seletora e devolve o escolhido via <see cref="DefinirFornecedor"/>.</summary>
    public event Action? AbrirSeletorFornecedorSolicitado;

    /// <summary>Itens (linhas) da compra.</summary>
    public ObservableCollection<CompraItemViewModel> Itens { get; } = new();

    // --- Cabeçalho ---
    [ObservableProperty] private FornecedorListaDto? _fornecedorSelecionado;
    [ObservableProperty] private string? _numDocumento;
    [ObservableProperty] private string? _observacao;

    [ObservableProperty] private bool _modoVisualizacao;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    /// <summary>Nome do usuário logado (read-only no cabeçalho — quem está registrando a compra).</summary>
    [ObservableProperty] private string _nomeUsuario = string.Empty;

    [ObservableProperty] private int _codCompra;

    public string Titulo => _id is null ? "Nova Compra" : $"Compra Nº {CodCompra}";

    /// <summary>Texto exibido no campo-seletor de fornecedor: "CÓD — RAZÃO SOCIAL" ou aviso se vazio.</summary>
    public string FornecedorTexto => FornecedorSelecionado is { } f
        ? $"{f.CodFornecedor:0000} — {f.RazaoSocial}"
        : "Selecione o fornecedor (F3)";

    /// <summary>Soma dos totais das linhas (= total da compra; não há desconto no MVP).</summary>
    public decimal VlrTotal => Itens.Sum(i => i.VlrTotalItem);
    public string VlrTotalTexto => VlrTotal.ToString("N2", PtBr);

    partial void OnFornecedorSelecionadoChanged(FornecedorListaDto? value) =>
        OnPropertyChanged(nameof(FornecedorTexto));

    partial void OnCodCompraChanged(int value) => OnPropertyChanged(nameof(Titulo));

    /// <summary>Prepara o formulário para uma nova compra (limpa, em edição).</summary>
    public Task PrepararNovaAsync(Guid idUsuario, string nomeUsuario)
    {
        _idUsuario = idUsuario;
        NomeUsuario = nomeUsuario;
        _id = null;
        CodCompra = 0;
        FornecedorSelecionado = null;
        NumDocumento = null;
        Observacao = null;
        LimparItens();
        MensagemErro = null;
        ModoVisualizacao = false;
        OnPropertyChanged(nameof(Titulo));
        RecalcularTotais();
        return Task.CompletedTask;
    }

    /// <summary>Carrega uma compra registrada em modo visualização (read-only — já deu entrada).</summary>
    public async Task CarregarAsync(Guid id, Guid idUsuario, string nomeUsuario)
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            _idUsuario = idUsuario;
            NomeUsuario = nomeUsuario;

            var resultado = await _compras.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var c = resultado.Valor;
            _id = c.Id;
            CodCompra = c.CodCompra;
            FornecedorSelecionado = new FornecedorListaDto(
                c.IdFornecedor, c.CodFornecedor, c.TipoFornecedor, c.DocumentoFornecedor,
                c.NomeFornecedor, null, true);
            NumDocumento = c.NumDocumento;
            Observacao = c.Observacao;

            LimparItens();
            foreach (var i in c.Itens)
                AdicionarItemVm(new CompraItemViewModel(i.IdProduto, i.DescricaoProduto, i.Qtd, i.VlrCustoUnitario));

            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
            RecalcularTotais();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar a compra.";
            _logger.LogError(ex, "Erro ao carregar compra {Id}.", id);
        }
        finally
        {
            Carregando = false;
        }
    }

    /// <summary>F2 / botão "Buscar peça": pede a janela do Catálogo. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirCatalogo()
    {
        if (!ModoVisualizacao)
            AbrirCatalogoSolicitado?.Invoke();
    }

    /// <summary>F3 / clique no campo Fornecedor: pede a janela seletora. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirSeletorFornecedor()
    {
        if (!ModoVisualizacao)
            AbrirSeletorFornecedorSolicitado?.Invoke();
    }

    /// <summary>Define o fornecedor escolhido no seletor. Chamado pela janela seletora.</summary>
    public void DefinirFornecedor(FornecedorListaDto? fornecedor)
    {
        if (fornecedor is not null)
            FornecedorSelecionado = fornecedor;
    }

    /// <summary>Adiciona a peça escolhida no Catálogo como nova linha. O custo unitário inicia em 0 — o
    /// operador digita o custo da nota do fornecedor (o custo do cadastro pode estar desatualizado). Se a
    /// peça já está na lista, incrementa a quantidade. Chamado pela janela seletora.</summary>
    public void AdicionarPecaDoCatalogo(CatalogoItemDto peca)
    {
        var existente = Itens.FirstOrDefault(i => i.IdProduto == peca.Id);
        if (existente is not null)
        {
            existente.QtdTexto = (existente.Qtd + 1).ToString(PtBr);
            existente.Realcar();
        }
        else
        {
            var novo = new CompraItemViewModel(peca.Id, peca.Descricao, 1, vlrCustoUnitario: 0);
            AdicionarItemVm(novo);
            novo.Realcar();
        }
    }

    [RelayCommand]
    private void RemoverItem(CompraItemViewModel? item)
    {
        if (item is null)
            return;
        item.TotalAlterado -= RecalcularTotais;
        Itens.Remove(item);
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            if (FornecedorSelecionado is null)
            {
                MensagemErro = "Selecione o fornecedor da compra (F3).";
                return;
            }

            if (Itens.Count == 0)
            {
                MensagemErro = "Adicione ao menos um item à compra.";
                return;
            }

            var itens = Itens
                .Select(i => new CompraItemDto(i.IdProduto, i.Qtd, i.VlrCustoUnitario))
                .ToList();

            var dto = new CriarCompraDto(FornecedorSelecionado.Id, NumDocumento, Observacao, itens);

            var resultado = await _compras.CriarAsync(_idUsuario, dto);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao registrar a compra.";
            _logger.LogError(ex, "Erro ao registrar compra.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();

    private void AdicionarItemVm(CompraItemViewModel item)
    {
        item.TotalAlterado += RecalcularTotais;
        Itens.Add(item);
    }

    private void LimparItens()
    {
        foreach (var i in Itens) i.TotalAlterado -= RecalcularTotais;
        Itens.Clear();
    }

    private void RecalcularTotais()
    {
        OnPropertyChanged(nameof(VlrTotal));
        OnPropertyChanged(nameof(VlrTotalTexto));
    }
}
