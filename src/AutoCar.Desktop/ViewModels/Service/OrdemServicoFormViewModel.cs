using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Application.Modules.Registrations.Mecanicos;
using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;
using AutoCar.Application.Modules.Registrations.Produtos.DTOs;
using AutoCar.Application.Modules.Registrations.Servicos;
using AutoCar.Application.Modules.Registrations.Servicos.DTOs;
using AutoCar.Application.Modules.Service.OrdensServico;
using AutoCar.Application.Modules.Service.OrdensServico.DTOs;
using AutoCar.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Service;

/// <summary>
/// Formulário de Ordem de Serviço em dois modos (visualização/edição). Cabeçalho com cliente opcional
/// (seletor F3), veículo livre e mecânico responsável (combo). Grid de itens misturando peças (F2,
/// Catálogo) e serviços (F4, seletor de serviço). Total calculado das linhas e do desconto geral.
/// Ciclo Aberta→EmAndamento→Concluída→Faturada/Cancelada — botões habilitam conforme a situação.
/// O Faturar (que baixa estoque) entra na sub-fase 4.4.
/// </summary>
public partial class OrdemServicoFormViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private readonly IOrdemServicoService _ordens;
    private readonly IClienteService _clientes;
    private readonly IMecanicoService _mecanicos;
    private readonly IServicoService _servicos;
    private readonly ILogger<OrdemServicoFormViewModel> _logger;

    private Guid? _id;
    private Guid _idUsuario;

    public OrdemServicoFormViewModel(
        IOrdemServicoService ordens,
        IClienteService clientes,
        IMecanicoService mecanicos,
        IServicoService servicos,
        ILogger<OrdemServicoFormViewModel> logger)
    {
        _ordens = ordens;
        _clientes = clientes;
        _mecanicos = mecanicos;
        _servicos = servicos;
        _logger = logger;

        Itens.CollectionChanged += (_, _) => RecalcularTotais();
    }

    public event Action? Salvo;
    public event Action? Cancelado;

    /// <summary>Disparado ao pedir o catálogo de peças (F2). A janela abre o Catálogo seletor.</summary>
    public event Action? AbrirCatalogoSolicitado;

    /// <summary>Disparado ao pedir o seletor de serviço (F4). A janela abre o seletor de serviço.</summary>
    public event Action? AbrirSeletorServicoSolicitado;

    /// <summary>Disparado ao pedir o seletor de cliente (F3 / clique no campo).</summary>
    public event Action? AbrirSeletorClienteSolicitado;

    /// <summary>Disparado ao pedir o seletor de mecânico (F5 / clique no campo).</summary>
    public event Action? AbrirSeletorMecanicoSolicitado;

    /// <summary>Itens (linhas) da OS — peças e serviços misturados.</summary>
    public ObservableCollection<OrdemServicoItemViewModel> Itens { get; } = new();

    // --- Cabeçalho ---
    [ObservableProperty] private ClienteListaDto? _clienteSelecionado;
    [ObservableProperty] private string? _nomeClienteAvulso;
    [ObservableProperty] private MecanicoDto? _mecanicoSelecionado;
    [ObservableProperty] private string? _veiculoMontadora;
    [ObservableProperty] private string? _veiculoModelo;
    [ObservableProperty] private string? _veiculoAno;
    [ObservableProperty] private string? _veiculoPlaca;
    [ObservableProperty] private int? _qtdKm;
    [ObservableProperty] private string? _observacao;
    [ObservableProperty] private decimal _vlrDesconto;

    /// <summary>Quilometragem como texto editável (inteiro, opcional). Vazio = sem km.</summary>
    public string? QtdKmTexto
    {
        get => QtdKm?.ToString(CultureInfo.InvariantCulture);
        set
        {
            var limpo = (value ?? string.Empty).Trim().Replace(".", "").Replace(",", "");
            QtdKm = int.TryParse(limpo, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && v >= 0
                ? v : (int?)null;
            OnPropertyChanged();
        }
    }

    partial void OnQtdKmChanged(int? value) => OnPropertyChanged(nameof(QtdKmTexto));

    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    /// <summary>Situação atual da OS carregada (Aberta enquanto nova).</summary>
    [ObservableProperty] private SituacaoOrdemServico _situacao = SituacaoOrdemServico.Aberta;

    /// <summary>Nome do atendente logado (read-only no cabeçalho — quem abriu a OS).</summary>
    [ObservableProperty] private string _nomeAtendente = string.Empty;

    [ObservableProperty] private int _codOrdemServico;

    public string Titulo => _id is null ? "Nova Ordem de Serviço" : $"OS Nº {CodOrdemServico}";

    // --- Estados do ciclo (controlam a visibilidade dos botões) ---

    /// <summary>OS é editável (Aberta ou Em andamento) e está em modo edição.</summary>
    public bool EhEditavel => Situacao is SituacaoOrdemServico.Aberta or SituacaoOrdemServico.EmAndamento;

    /// <summary>Iniciar: só uma OS Aberta já salva, em visualização.</summary>
    public bool PodeIniciar => _id is not null && Situacao == SituacaoOrdemServico.Aberta && ModoVisualizacao;

    /// <summary>Concluir: OS Aberta ou Em andamento, já salva, em visualização.</summary>
    public bool PodeConcluir => _id is not null && EhEditavel && ModoVisualizacao;

    /// <summary>Faturar: só OS Concluída (o faturamento de fato entra na 4.4).</summary>
    public bool PodeFaturar => _id is not null && Situacao == SituacaoOrdemServico.Concluida && ModoVisualizacao;

    /// <summary>Cancelar: enquanto não faturada nem cancelada, já salva, em visualização.</summary>
    public bool PodeCancelar => _id is not null
        && Situacao is not (SituacaoOrdemServico.Faturada or SituacaoOrdemServico.Cancelada)
        && ModoVisualizacao;

    /// <summary>Editar (botão): só quando a OS é editável (Aberta/EmAndamento) e em visualização.</summary>
    public bool PodeEditar => EhEditavel && ModoVisualizacao;

    /// <summary>Selo da situação no header (sempre visível em visualização para OS salva).</summary>
    public string SituacaoRotulo => Situacao switch
    {
        SituacaoOrdemServico.Aberta => "ABERTA",
        SituacaoOrdemServico.EmAndamento => "EM ANDAMENTO",
        SituacaoOrdemServico.Concluida => "CONCLUÍDA",
        SituacaoOrdemServico.Faturada => "FATURADA",
        SituacaoOrdemServico.Cancelada => "CANCELADA",
        _ => "",
    };

    /// <summary>Mostra o selo de situação — só para OS já salva (nova não tem situação relevante).</summary>
    public bool MostrarSelo => _id is not null;

    partial void OnSituacaoChanged(SituacaoOrdemServico value) => NotificarBotoes();
    partial void OnModoVisualizacaoChanged(bool value) => NotificarBotoes();

    private void NotificarBotoes()
    {
        OnPropertyChanged(nameof(EhEditavel));
        OnPropertyChanged(nameof(PodeIniciar));
        OnPropertyChanged(nameof(PodeConcluir));
        OnPropertyChanged(nameof(PodeFaturar));
        OnPropertyChanged(nameof(PodeCancelar));
        OnPropertyChanged(nameof(PodeEditar));
        OnPropertyChanged(nameof(SituacaoRotulo));
        OnPropertyChanged(nameof(MostrarSelo));
    }

    /// <summary>Quando há cliente cadastrado, o nome avulso fica desabilitado/limpo.</summary>
    public bool ClienteAvulso => ClienteSelecionado is null;

    /// <summary>Texto do campo-seletor de cliente: "CÓD — NOME" ou "Consumidor" se avulso.</summary>
    public string ClienteTexto => ClienteSelecionado is { } c
        ? $"{c.CodCliente:0000} — {c.RazaoSocial}"
        : "Consumidor";

    partial void OnClienteSelecionadoChanged(ClienteListaDto? value)
    {
        OnPropertyChanged(nameof(ClienteAvulso));
        OnPropertyChanged(nameof(ClienteTexto));
        if (value is not null)
            NomeClienteAvulso = null;
    }

    /// <summary>Texto exibido no campo-seletor de mecânico: "CÓD — NOME" ou "Sem mecânico".</summary>
    public string MecanicoTexto => MecanicoSelecionado is { } m
        ? $"{m.CodMecanico:0000} — {m.Nome}"
        : "Sem mecânico";

    partial void OnMecanicoSelecionadoChanged(MecanicoDto? value) =>
        OnPropertyChanged(nameof(MecanicoTexto));

    partial void OnCodOrdemServicoChanged(int value) => OnPropertyChanged(nameof(Titulo));

    /// <summary>Desconto geral como texto editável.</summary>
    public string VlrDescontoTexto
    {
        get => VlrDesconto.ToString("N2", PtBr);
        set { VlrDesconto = ParseMoeda(value); OnPropertyChanged(); }
    }

    partial void OnVlrDescontoChanged(decimal value)
    {
        OnPropertyChanged(nameof(VlrDescontoTexto));
        OnPropertyChanged(nameof(VlrTotal));
        OnPropertyChanged(nameof(VlrTotalTexto));
    }

    // --- Totais (peças + serviços) ---
    public decimal SubtotalPecas => Itens.Where(i => i.EhPeca).Sum(i => i.VlrTotalItem);
    public string SubtotalPecasTexto => SubtotalPecas.ToString("N2", PtBr);

    public decimal SubtotalServicos => Itens.Where(i => !i.EhPeca).Sum(i => i.VlrTotalItem);
    public string SubtotalServicosTexto => SubtotalServicos.ToString("N2", PtBr);

    public decimal SubtotalItens => Itens.Sum(i => i.VlrTotalItem);
    public string SubtotalItensTexto => SubtotalItens.ToString("N2", PtBr);

    public decimal VlrTotal => Math.Max(0, SubtotalItens - VlrDesconto);
    public string VlrTotalTexto => VlrTotal.ToString("N2", PtBr);

    /// <summary>Prepara o formulário para uma nova OS (limpa, em edição).</summary>
    public Task PrepararNovaAsync(Guid idUsuario, string nomeAtendente)
    {
        _idUsuario = idUsuario;
        NomeAtendente = nomeAtendente;
        _id = null;
        CodOrdemServico = 0;
        Situacao = SituacaoOrdemServico.Aberta;
        ClienteSelecionado = null;
        NomeClienteAvulso = null;
        MecanicoSelecionado = null;
        VeiculoMontadora = VeiculoModelo = VeiculoAno = VeiculoPlaca = null;
        QtdKm = null;
        Observacao = null;
        VlrDesconto = 0;
        LimparItens();
        MensagemErro = null;
        ModoVisualizacao = false;
        OnPropertyChanged(nameof(Titulo));
        NotificarBotoes();
        RecalcularTotais();
        return Task.CompletedTask;
    }

    /// <summary>Carrega uma OS existente em modo visualização.</summary>
    public async Task CarregarAsync(Guid id, Guid idUsuario, string nomeAtendente)
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            _idUsuario = idUsuario;
            NomeAtendente = nomeAtendente;

            var resultado = await _ordens.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var o = resultado.Valor;
            _id = o.Id;
            CodOrdemServico = o.CodOrdemServico;
            Situacao = o.Situacao;

            ClienteSelecionado = await ObterClienteResumoAsync(o.IdCliente);
            NomeClienteAvulso = o.NomeClienteAvulso;
            // Mecânico: busca o resumo pelo Id (igual ao cliente) para exibir no campo-seletor.
            MecanicoSelecionado = await ObterMecanicoResumoAsync(o.IdMecanico);
            VeiculoMontadora = o.VeiculoMontadora;
            VeiculoModelo = o.VeiculoModelo;
            VeiculoAno = o.VeiculoAno;
            VeiculoPlaca = o.VeiculoPlaca;
            QtdKm = o.QtdKm;
            Observacao = o.Observacao;
            VlrDesconto = o.VlrDesconto;

            LimparItens();
            foreach (var i in o.Itens)
                AdicionarItemVm(new OrdemServicoItemViewModel(
                    i.Tipo, i.IdProduto, i.IdServico, i.DescricaoItem, i.Qtd, i.VlrUnitario, i.VlrDesconto));

            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
            NotificarBotoes();
            RecalcularTotais();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar a ordem de serviço.";
            _logger.LogError(ex, "Erro ao carregar OS {Id}.", id);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void HabilitarEdicao() => ModoVisualizacao = false;

    /// <summary>F2 / "Buscar peça": pede o Catálogo seletor. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirCatalogo()
    {
        if (!ModoVisualizacao)
            AbrirCatalogoSolicitado?.Invoke();
    }

    /// <summary>F4 / "Adicionar serviço": pede o seletor de serviço. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirSeletorServico()
    {
        if (!ModoVisualizacao)
            AbrirSeletorServicoSolicitado?.Invoke();
    }

    /// <summary>F3 / clique no campo Cliente: pede o seletor de cliente. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirSeletorCliente()
    {
        if (!ModoVisualizacao)
            AbrirSeletorClienteSolicitado?.Invoke();
    }

    public void DefinirCliente(ClienteListaDto? cliente) => ClienteSelecionado = cliente;

    [RelayCommand]
    private void LimparCliente() => ClienteSelecionado = null;

    /// <summary>F5 / clique no campo Mecânico: pede o seletor de mecânico. Só no modo edição.</summary>
    [RelayCommand]
    private void AbrirSeletorMecanico()
    {
        if (!ModoVisualizacao)
            AbrirSeletorMecanicoSolicitado?.Invoke();
    }

    /// <summary>Define o mecânico escolhido no seletor (null = sem mecânico). Chamado pela janela seletora.</summary>
    public void DefinirMecanico(MecanicoDto? mecanico) => MecanicoSelecionado = mecanico;

    /// <summary>Adiciona a peça escolhida no Catálogo (preço = snapshot do VlrVenda). Se já existe a
    /// mesma peça, incrementa a quantidade. Chamado pela janela seletora.</summary>
    public void AdicionarPecaDoCatalogo(CatalogoItemDto peca)
    {
        var existente = Itens.FirstOrDefault(i => i.EhPeca && i.IdProduto == peca.Id);
        if (existente is not null)
        {
            existente.QtdTexto = (existente.Qtd + 1).ToString(CultureInfo.InvariantCulture);
            existente.Realcar();
        }
        else
        {
            var novo = new OrdemServicoItemViewModel(
                TipoItemOrdemServico.Peca, peca.Id, null, peca.Descricao, 1, peca.VlrVenda, 0);
            AdicionarItemVm(novo);
            novo.Realcar();
        }
    }

    /// <summary>Adiciona o serviço escolhido no seletor (valor = snapshot do VlrPadrao). Se já existe
    /// o mesmo serviço, incrementa a quantidade. Chamado pela janela seletora.</summary>
    public void AdicionarServico(ServicoDto servico)
    {
        var existente = Itens.FirstOrDefault(i => !i.EhPeca && i.IdServico == servico.Id);
        if (existente is not null)
        {
            existente.QtdTexto = (existente.Qtd + 1).ToString(CultureInfo.InvariantCulture);
            existente.Realcar();
        }
        else
        {
            var novo = new OrdemServicoItemViewModel(
                TipoItemOrdemServico.Servico, null, servico.Id, servico.Descricao, 1, servico.VlrPadrao, 0);
            AdicionarItemVm(novo);
            novo.Realcar();
        }
    }

    [RelayCommand]
    private void RemoverItem(OrdemServicoItemViewModel? item)
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
                MensagemErro = "Adicione ao menos um item à ordem de serviço.";
                return;
            }

            var itens = Itens.Select(i => new OrdemServicoItemDto(
                i.Tipo, i.IdProduto, i.IdServico, i.DescricaoItem, i.Qtd, i.VlrUnitario, i.VlrDesconto)).ToList();

            var dto = new SalvarOrdemServicoDto(
                ClienteSelecionado?.Id, NomeClienteAvulso, MecanicoSelecionado?.Id,
                VeiculoMontadora, VeiculoModelo, VeiculoAno, VeiculoPlaca, QtdKm,
                VlrDesconto, Observacao, itens);

            var resultado = _id is null
                ? await _ordens.CriarAsync(_idUsuario, dto)
                : await _ordens.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar a ordem de serviço.";
            _logger.LogError(ex, "Erro ao salvar OS.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task IniciarAsync() => await TransicionarAsync(() => _ordens.IniciarAsync(_id!.Value), PodeIniciar);

    [RelayCommand]
    private async Task ConcluirAsync() => await TransicionarAsync(() => _ordens.ConcluirAsync(_id!.Value), PodeConcluir);

    /// <summary>Botão "Cancelar OS": pede a confirmação à janela (cancelar encerra a OS, é irreversível).
    /// O cancelamento de fato roda em <see cref="ConfirmarCancelamentoAsync"/> só após confirmar.</summary>
    [RelayCommand]
    private void CancelarOs()
    {
        if (PodeCancelar)
            ConfirmacaoCancelamentoSolicitada?.Invoke();
    }

    /// <summary>Disparado ao clicar em "Cancelar OS". A janela mostra a confirmação e, se o usuário
    /// confirmar, chama <see cref="ConfirmarCancelamentoAsync"/>.</summary>
    public event Action? ConfirmacaoCancelamentoSolicitada;

    /// <summary>Efetiva o cancelamento da OS (após confirmado na janela).</summary>
    public Task ConfirmarCancelamentoAsync() =>
        TransicionarAsync(() => _ordens.CancelarAsync(_id!.Value), PodeCancelar);

    /// <summary>Faturar a OS (baixa estoque das peças). A implementação transacional entra na sub-fase
    /// 4.4 (FaturamentoOrdemServicoRepository). Por ora, o botão existe e avisa que está pendente.</summary>
    [RelayCommand]
    private void Faturar()
    {
        if (PodeFaturar)
            MensagemErro = "O faturamento da OS (baixa de estoque) será habilitado na próxima etapa.";
    }

    /// <summary>Aplica uma transição de ciclo e recarrega a OS (a situação muda na tela). Em sucesso,
    /// a OS continua aberta na janela em visualização com o novo status.</summary>
    private async Task TransicionarAsync(Func<Task<AutoCar.Shared.Results.Result>> acao, bool permitido)
    {
        if (_id is null || !permitido)
            return;

        Carregando = true;
        MensagemErro = null;
        try
        {
            var resultado = await acao();
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            // Recarrega para refletir a nova situação (e manter a tela coerente).
            await CarregarAsync(_id.Value, _idUsuario, NomeAtendente);
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao mudar a situação da ordem de serviço.";
            _logger.LogError(ex, "Erro na transição da OS {Id}.", _id);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();

    // Busca o resumo de um mecânico pelo Id (para exibir no campo ao reabrir uma OS).
    // Retorna null se não houver mecânico vinculado ou se não encontrar.
    private async Task<MecanicoDto?> ObterMecanicoResumoAsync(Guid? idMecanico)
    {
        if (idMecanico is not Guid id)
            return null;

        var r = await _mecanicos.ObterPorIdAsync(id);
        return r.Falha ? null : r.Valor;
    }

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

    private void AdicionarItemVm(OrdemServicoItemViewModel item)
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
        OnPropertyChanged(nameof(SubtotalPecas));
        OnPropertyChanged(nameof(SubtotalPecasTexto));
        OnPropertyChanged(nameof(SubtotalServicos));
        OnPropertyChanged(nameof(SubtotalServicosTexto));
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
