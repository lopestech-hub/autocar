using System;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Security;
using AutoCar.Application.Modules.Security.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels;

/// <summary>
/// ViewModel da tela de login. Valida credenciais via IAutenticacaoService e
/// dispara LoginConcluido com o usuário autenticado para a janela trocar de cena.
/// </summary>
public partial class LoginViewModel : ViewModelBase
{
    private readonly IAutenticacaoService _autenticacao;
    private readonly ILogger<LoginViewModel> _logger;

    public LoginViewModel(IAutenticacaoService autenticacao, ILogger<LoginViewModel> logger)
    {
        _autenticacao = autenticacao;
        _logger = logger;
    }

    /// <summary>Disparado quando o login é bem-sucedido. Carrega o usuário logado.</summary>
    public event Action<UsuarioLogado>? LoginConcluido;

    [ObservableProperty]
    private string _login = string.Empty;

    [ObservableProperty]
    private string _senha = string.Empty;

    [ObservableProperty]
    private bool _carregando;

    [ObservableProperty]
    private string? _mensagemErro;

    [RelayCommand]
    private async Task EntrarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var resultado = await _autenticacao.AutenticarAsync(Login, Senha);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            _logger.LogInformation("Login efetuado: {Login}", resultado.Valor.Login);
            LoginConcluido?.Invoke(resultado.Valor);
        }
        catch (Exception ex)
        {
            MensagemErro = "Não foi possível conectar. Tente novamente.";
            _logger.LogError(ex, "Erro ao autenticar usuário.");
        }
        finally
        {
            Carregando = false;
        }
    }
}
