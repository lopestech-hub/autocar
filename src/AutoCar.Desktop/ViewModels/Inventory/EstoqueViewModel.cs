using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AutoCar.Application.Modules.Estoque;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Application.Modules.Security.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Inventory;

/// <summary>
/// Listagem de saldos de estoque: busca por descrição/código e abre a janela de movimentação de um
/// produto (Entrada/Saída/Ajuste) numa JANELA SEPARADA, mesmo padrão da Pré-venda — dispara
/// <see cref="AbrirMovimentacaoSolicitado"/> e a View (code-behind) abre a janela não-modal.
/// Depende do usuário logado (registra quem movimentou), por isso é montada à mão, não via DI puro.
/// </summary>
public partial class EstoqueViewModel : ViewModelBase
{
    private readonly IEstoqueService _estoque;
    private readonly Func<MovimentoEstoqueFormViewModel> _formFactory;
    private readonly ILogger<EstoqueViewModel> _logger;
    private readonly Guid _idUsuario;

    private CancellationTokenSource? _debounce;

    public EstoqueViewModel(
        UsuarioLogado usuario,
        IEstoqueService estoque,
        Func<MovimentoEstoqueFormViewModel> formFactory,
        ILogger<EstoqueViewModel> logger)
    {
        _idUsuario = usuario.Id;
        _estoque = estoque;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando a janela de movimentação deve abrir. A View (code-behind) escuta,
    /// abre a janela não-modal e recarrega a lista ao concluir.</summary>
    public event Action<MovimentoEstoqueFormViewModel>? AbrirMovimentacaoSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela fechar com sucesso).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<SaldoEstoqueListaDto> Saldos { get; } = new();

    [ObservableProperty] private string _filtro = string.Empty;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string TextoContador => Saldos.Count switch
    {
        0 => "Nenhum produto",
        1 => "1 produto",
        var n => $"{n} produtos",
    };

    partial void OnFiltroChanged(string value) => AgendarBusca();

    private void AgendarBusca()
    {
        _debounce?.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(350, token);
                if (!token.IsCancellationRequested)
                    await Dispatcher.UIThread.InvokeAsync(CarregarAsync);
            }
            catch (TaskCanceledException) { /* nova tecla cancelou: ignora */ }
        });
    }

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var lista = await _estoque.ListarSaldosAsync(Filtro);
            Saldos.Clear();
            foreach (var s in lista)
                Saldos.Add(s);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar os saldos de estoque.";
            _logger.LogError(ex, "Erro ao listar saldos de estoque.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task BuscarAsync() => await CarregarAsync();

    [RelayCommand]
    private async Task MovimentarAsync(SaldoEstoqueListaDto? saldo)
    {
        if (saldo is null)
            return;

        var form = _formFactory();
        await form.PrepararAsync(_idUsuario, saldo.IdProduto, saldo.CodProduto,
            saldo.Descricao, saldo.Unidade.ToString());
        AbrirMovimentacaoSolicitado?.Invoke(form);
    }
}
