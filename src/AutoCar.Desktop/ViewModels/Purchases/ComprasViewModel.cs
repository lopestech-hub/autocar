using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Purchases.Compras;
using AutoCar.Application.Modules.Purchases.Compras.DTOs;
using AutoCar.Application.Modules.Security.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Purchases;

/// <summary>
/// Listagem de compras: abre o formulário (nova/visualização) numa JANELA SEPARADA maximizada.
/// A listagem não embute o form — dispara <see cref="AbrirJanelaSolicitado"/> e a View (code-behind)
/// abre a CompraWindow não-modal, mantendo o shell principal acessível. Depende do usuário logado
/// (registra quem fez a compra) — por isso recebe o UsuarioLogado e não vai no DI puro.
/// </summary>
public partial class ComprasViewModel : ViewModelBase
{
    private readonly ICompraService _compras;
    private readonly Func<CompraFormViewModel> _formFactory;
    private readonly ILogger<ComprasViewModel> _logger;
    private readonly Guid _idUsuario;
    private readonly string _nomeUsuario;

    public ComprasViewModel(
        UsuarioLogado usuario,
        ICompraService compras,
        Func<CompraFormViewModel> formFactory,
        ILogger<ComprasViewModel> logger)
    {
        _idUsuario = usuario.Id;
        _nomeUsuario = usuario.Nome;
        _compras = compras;
        _formFactory = formFactory;
        _logger = logger;
    }

    /// <summary>Disparado quando o form da compra deve abrir numa janela separada. A View (code-behind)
    /// escuta, abre a CompraWindow não-modal e recarrega a lista ao salvar.</summary>
    public event Action<CompraFormViewModel>? AbrirJanelaSolicitado;

    /// <summary>Recarrega a listagem (chamado pela View após a janela fechar com sucesso).</summary>
    public Task RecarregarAsync() => CarregarAsync();

    public ObservableCollection<CompraListaDto> Compras { get; } = new();

    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string TextoContador => Compras.Count switch
    {
        0 => "Nenhuma compra",
        1 => "1 compra",
        var n => $"{n} compras",
    };

    [RelayCommand]
    private async Task CarregarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var resultado = await _compras.ListarAsync();
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Compras.Clear();
            foreach (var c in resultado.Valor)
                Compras.Add(c);
            OnPropertyChanged(nameof(TextoContador));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar compras.";
            _logger.LogError(ex, "Erro ao listar compras.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task NovoAsync()
    {
        var form = _formFactory();
        await form.PrepararNovaAsync(_idUsuario, _nomeUsuario);
        AbrirJanelaSolicitado?.Invoke(form);
    }

    [RelayCommand]
    private async Task AbrirAsync(CompraListaDto? compra)
    {
        if (compra is null)
            return;

        var form = _formFactory();
        await form.CarregarAsync(compra.Id, _idUsuario, _nomeUsuario);
        AbrirJanelaSolicitado?.Invoke(form);
    }
}
