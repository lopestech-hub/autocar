using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Application.Modules.Sales.PreVendas;
using AutoCar.Application.Modules.Sales.PreVendas.DTOs;
using AutoCar.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Sales;

/// <summary>
/// Formulário de Pré-venda em dois modos (visualização/edição). Cabeçalho com cliente
/// opcional (combo + opção "— (Consumidor)") e veículo texto livre; grid de itens editável
/// alimentado pelo Catálogo (aberto numa janela separada via F2). Total calculado a partir das
/// linhas e do desconto geral. Nesta rodada só cria/edita pré-venda Aberta (sem Faturar/Cancelar).
/// </summary>
public partial class PreVendaFormViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private readonly IPreVendaService _preVendas;
    private readonly IClienteService _clientes;
    private readonly ILogger<PreVendaFormViewModel> _logger;

    private Guid? _id;
    private Guid _idUsuario;

    public PreVendaFormViewModel(
        IPreVendaService preVendas,
        IClienteService clientes,
        ILogger<PreVendaFormViewModel> logger)
    {
        _preVendas = preVendas;
        _clientes = clientes;
        _logger = logger;

        Itens.CollectionChanged += (_, _) => RecalcularTotais();
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    /// <summary>Disparado quando a pré-venda é faturada com sucesso (baixou estoque). A janela fecha
    /// e a listagem recarrega — mesmo tratamento de <see cref="Salvo"/>.</summary>
    public event Action? Faturado;

    /// <summary>Disparado ao clicar em Devolver (venda faturada). A janela abre a DevolucaoWindow
    /// passando esta pré-venda. Devolver itens só faz sentido depois de faturada.</summary>
    public event Action? AbrirDevolucaoSolicitada;

    /// <summary>Disparado ao pedir o catálogo (F2). A janela da pré-venda abre a janela seletora de
    /// peças (Catálogo) e devolve a escolhida via <see cref="AdicionarPecaDoCatalogo"/>.</summary>
    public event Action? AbrirCatalogoSolicitado;

    /// <summary>Itens (linhas) da pré-venda.</summary>
    public ObservableCollection<PreVendaItemViewModel> Itens { get; } = new();

    // --- Cabeçalho ---
    [ObservableProperty] private ClienteListaDto? _clienteSelecionado;
    [ObservableProperty] private string? _nomeClienteAvulso;
    [ObservableProperty] private string? _veiculoMontadora;
    [ObservableProperty] private string? _veiculoModelo;
    [ObservableProperty] private string? _veiculoAno;
    [ObservableProperty] private string? _veiculoPlaca;
    [ObservableProperty] private string? _observacao;
    [ObservableProperty] private decimal _vlrDesconto;

    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    /// <summary>Situação atual do documento carregado (Aberta enquanto nova/em edição).</summary>
    [ObservableProperty] private SituacaoPreVenda _situacao = SituacaoPreVenda.Aberta;

    /// <summary>Faturar só faz sentido numa pré-venda já salva (tem Id), Aberta e em visualização —
    /// mesma porta do botão "Editar". Faturar baixa o estoque e torna o documento imutável.</summary>
    public bool PodeFaturar => _id is not null && Situacao == SituacaoPreVenda.Aberta && ModoVisualizacao;

    /// <summary>Documento já faturado — exibe um aviso/estado somente-leitura, sem ações de alteração.</summary>
    public bool EstaFaturada => Situacao == SituacaoPreVenda.Faturada;

    /// <summary>Mostra o selo "VISUALIZAÇÃO" — só quando em visualização e ainda não faturada (faturada
    /// tem o próprio selo). Evita os dois selos competindo no header.</summary>
    public bool EmVisualizacaoNaoFaturada => ModoVisualizacao && !EstaFaturada;

    /// <summary>Devolver itens só faz sentido numa venda já faturada (que baixou estoque).</summary>
    public bool PodeDevolver => _id is not null && Situacao == SituacaoPreVenda.Faturada;

    partial void OnSituacaoChanged(SituacaoPreVenda value)
    {
        OnPropertyChanged(nameof(PodeFaturar));
        OnPropertyChanged(nameof(EstaFaturada));
        OnPropertyChanged(nameof(EmVisualizacaoNaoFaturada));
        OnPropertyChanged(nameof(PodeDevolver));
    }

    partial void OnModoVisualizacaoChanged(bool value)
    {
        OnPropertyChanged(nameof(PodeFaturar));
        OnPropertyChanged(nameof(EmVisualizacaoNaoFaturada));
    }

    /// <summary>Nome do vendedor logado (read-only no cabeçalho — quem está registrando a venda).</summary>
    [ObservableProperty] private string _nomeVendedor = string.Empty;

    /// <summary>Quando há cliente cadastrado selecionado, o nome avulso fica desabilitado.</summary>
    public bool ClienteAvulso => ClienteSelecionado is null;

    /// <summary>Texto exibido no campo-seletor de cliente: "CÓD — NOME" ou "Consumidor" se avulso.</summary>
    public string ClienteTexto => ClienteSelecionado is { } c
        ? $"{c.CodCliente:0000} — {c.RazaoSocial}"
        : "Consumidor";

    /// <summary>Disparado ao pedir o seletor de cliente (F3 / clique no campo). A janela da
    /// pré-venda abre a janela seletora de cliente e devolve o escolhido via <see cref="DefinirCliente"/>.</summary>
    public event Action? AbrirSeletorClienteSolicitado;

    public string Titulo => _id is null ? "Nova Pré-venda" : $"Pré-venda Nº {CodPreVenda}";

    [ObservableProperty] private int _codPreVenda;

    /// <summary>Desconto geral como texto editável.</summary>
    public string VlrDescontoTexto
    {
        get => VlrDesconto.ToString("N2", PtBr);
        set { VlrDesconto = ParseMoeda(value); OnPropertyChanged(); }
    }

    /// <summary>Soma dos totais das linhas (antes do desconto geral).</summary>
    public decimal SubtotalItens => Itens.Sum(i => i.VlrTotalItem);
    public string SubtotalItensTexto => SubtotalItens.ToString("N2", PtBr);

    /// <summary>Total final: subtotal − desconto geral, nunca negativo.</summary>
    public decimal VlrTotal => Math.Max(0, SubtotalItens - VlrDesconto);
    public string VlrTotalTexto => VlrTotal.ToString("N2", PtBr);

    partial void OnClienteSelecionadoChanged(ClienteListaDto? value)
    {
        OnPropertyChanged(nameof(ClienteAvulso));
        OnPropertyChanged(nameof(ClienteTexto));
        if (value is not null)
            NomeClienteAvulso = null; // com cliente cadastrado, não usa nome avulso
    }

    partial void OnVlrDescontoChanged(decimal value)
    {
        OnPropertyChanged(nameof(VlrDescontoTexto));
        OnPropertyChanged(nameof(VlrTotal));
        OnPropertyChanged(nameof(VlrTotalTexto));
    }

    partial void OnCodPreVendaChanged(int value) => OnPropertyChanged(nameof(Titulo));

    /// <summary>Prepara o formulário para uma nova pré-venda (limpa, em edição).</summary>
    public Task PrepararNovaAsync(Guid idUsuario, string nomeVendedor)
    {
        _idUsuario = idUsuario;
        NomeVendedor = nomeVendedor;
        _id = null;
        CodPreVenda = 0;
        Situacao = SituacaoPreVenda.Aberta;
        ClienteSelecionado = null;
        NomeClienteAvulso = null;
        VeiculoMontadora = VeiculoModelo = VeiculoAno = VeiculoPlaca = null;
        Observacao = null;
        VlrDesconto = 0;
        LimparItens();
        MensagemErro = null;
        ModoVisualizacao = false;
        OnPropertyChanged(nameof(Titulo));
        RecalcularTotais();
        return Task.CompletedTask;
    }

    /// <summary>Carrega uma pré-venda existente em modo visualização.</summary>
    public async Task CarregarAsync(Guid id, Guid idUsuario, string nomeVendedor)
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            _idUsuario = idUsuario;
            NomeVendedor = nomeVendedor;

            var resultado = await _preVendas.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var p = resultado.Valor;
            _id = p.Id;
            CodPreVenda = p.CodPreVenda;
            Situacao = p.Situacao;

            // Carrega o cliente vinculado (se houver) para exibir no campo-seletor.
            ClienteSelecionado = await ObterClienteResumoAsync(p.IdCliente);
            NomeClienteAvulso = p.NomeClienteAvulso;
            VeiculoMontadora = p.VeiculoMontadora;
            VeiculoModelo = p.VeiculoModelo;
            VeiculoAno = p.VeiculoAno;
            VeiculoPlaca = p.VeiculoPlaca;
            Observacao = p.Observacao;
            VlrDesconto = p.VlrDesconto;

            LimparItens();
            foreach (var i in p.Itens)
                AdicionarItemVm(new PreVendaItemViewModel(i.IdProduto, i.DescricaoProduto, i.Qtd, i.VlrUnitario, i.VlrDesconto,
                    i.CodProduto, i.CodFabricante));

            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
            OnPropertyChanged(nameof(PodeFaturar));
            RecalcularTotais();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar a pré-venda.";
            _logger.LogError(ex, "Erro ao carregar pré-venda {Id}.", id);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void HabilitarEdicao() => ModoVisualizacao = false;

    /// <summary>F2 / botão "Buscar peça": pede a janela do Catálogo. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirCatalogo()
    {
        if (!ModoVisualizacao)
            AbrirCatalogoSolicitado?.Invoke();
    }

    /// <summary>F3 / clique no campo Cliente: pede a janela seletora de cliente. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirSeletorCliente()
    {
        if (!ModoVisualizacao)
            AbrirSeletorClienteSolicitado?.Invoke();
    }

    /// <summary>Define o cliente escolhido no seletor (null = volta para Consumidor/avulso).
    /// Chamado pela janela seletora de cliente.</summary>
    public void DefinirCliente(ClienteListaDto? cliente) => ClienteSelecionado = cliente;

    /// <summary>Adiciona a peça escolhida no Catálogo como uma nova linha (preço = snapshot do
    /// VlrVenda). Se a peça já está na lista, incrementa a quantidade. Chamado pela janela seletora.</summary>
    public void AdicionarPecaDoCatalogo(CatalogoItemDto peca)
    {
        var existente = Itens.FirstOrDefault(i => i.IdProduto == peca.Id);
        if (existente is not null)
        {
            existente.QtdTexto = (existente.Qtd + 1).ToString("N2", PtBr);
            existente.Realcar(); // pisca a linha que recebeu +1
        }
        else
        {
            var novo = new PreVendaItemViewModel(peca.Id, peca.Descricao, 1, peca.VlrVenda, 0,
                peca.CodProduto, peca.CodFabricante);
            AdicionarItemVm(novo);
            novo.Realcar(); // pisca a linha recém-adicionada
        }
    }

    [RelayCommand]
    private void RemoverItem(PreVendaItemViewModel? item)
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
            if (Itens.Count == 0)
            {
                MensagemErro = "Adicione ao menos um item à pré-venda.";
                return;
            }

            var itens = Itens
                .Select(i => new PreVendaItemDto(i.IdProduto, i.DescricaoProduto, i.Qtd, i.VlrUnitario, i.VlrDesconto))
                .ToList();

            var dto = new SalvarPreVendaDto(
                ClienteSelecionado?.Id, NomeClienteAvulso,
                VeiculoMontadora, VeiculoModelo, VeiculoAno, VeiculoPlaca,
                VlrDesconto, Observacao, itens);

            var resultado = _id is null
                ? await _preVendas.CriarAsync(_idUsuario, dto)
                : await _preVendas.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar a pré-venda.";
            _logger.LogError(ex, "Erro ao salvar pré-venda.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();

    /// <summary>Disparado ao clicar em Faturar. A janela mostra a confirmação (faturar é irreversível e
    /// baixa estoque) e, se o usuário confirmar, chama <see cref="ConfirmarFaturamentoAsync"/>.</summary>
    public event Action? ConfirmacaoFaturamentoSolicitada;

    /// <summary>Botão Faturar: pede a confirmação à janela. O faturamento de fato roda em
    /// <see cref="ConfirmarFaturamentoAsync"/> só após o usuário confirmar.</summary>
    [RelayCommand]
    private void Faturar()
    {
        if (PodeFaturar)
            ConfirmacaoFaturamentoSolicitada?.Invoke();
    }

    /// <summary>Botão Devolver (venda faturada): pede à janela para abrir a tela de devolução desta venda.</summary>
    [RelayCommand]
    private void Devolver()
    {
        if (PodeDevolver)
            AbrirDevolucaoSolicitada?.Invoke();
    }

    /// <summary>Id da pré-venda carregada (para a janela montar a devolução). Null se nova.</summary>
    public Guid? IdPreVenda => _id;

    /// <summary>Usuário logado (vendedor) — usado ao abrir a devolução.</summary>
    public Guid IdUsuarioLogado => _idUsuario;

    /// <summary>Efetiva o faturamento (após confirmado na janela): fatura a pré-venda e baixa o estoque
    /// de todos os itens numa única transação. Em sucesso, dispara <see cref="Faturado"/> (a janela
    /// fecha e a lista recarrega). Falhas (saldo insuficiente, concorrência) viram mensagem na tela.</summary>
    public async Task ConfirmarFaturamentoAsync()
    {
        if (_id is null)
            return;

        Carregando = true;
        MensagemErro = null;
        try
        {
            var resultado = await _preVendas.FaturarAsync(_id.Value, _idUsuario);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Faturado?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao faturar a pré-venda.";
            _logger.LogError(ex, "Erro ao faturar pré-venda {Id}.", _id);
        }
        finally
        {
            Carregando = false;
        }
    }

    // Busca o resumo de um cliente pelo Id (para exibir no campo ao reabrir uma pré-venda).
    // Retorna null se não houver cliente vinculado (venda avulsa) ou se não encontrar.
    private async Task<ClienteListaDto?> ObterClienteResumoAsync(Guid? idCliente)
    {
        if (idCliente is not Guid id)
            return null;

        var r = await _clientes.ObterPorIdAsync(id);
        if (r.Falha)
            return null;

        var c = r.Valor;
        return new ClienteListaDto(c.Id, c.CodCliente, c.TipoPessoa, c.Documento, c.RazaoSocial, c.Telefone, c.FlgAtivo);
    }

    /// <summary>Limpa a seleção de cliente (volta para venda avulsa/Consumidor).</summary>
    [RelayCommand]
    private void LimparCliente() => ClienteSelecionado = null;

    private void AdicionarItemVm(PreVendaItemViewModel item)
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
        OnPropertyChanged(nameof(SubtotalItens));
        OnPropertyChanged(nameof(SubtotalItensTexto));
        OnPropertyChanged(nameof(VlrTotal));
        OnPropertyChanged(nameof(VlrTotalTexto));
    }

    private static decimal ParseMoeda(string? texto)
    {
        var limpo = (texto ?? string.Empty).Trim().Replace(".", "").Replace(",", ".");
        return decimal.TryParse(limpo, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) && v >= 0 ? v : 0;
    }
}
