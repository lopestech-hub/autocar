using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;
using AutoCar.Domain.Enums;
using AutoCar.Domain.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Registrations;

/// <summary>
/// Formulário de Cliente em dois modos: abre em visualização (campos bloqueados); o
/// usuário clica em Editar para liberar. Novo abre direto em edição. A View é da Luna.
/// </summary>
public partial class ClienteFormViewModel : ViewModelBase
{
    private readonly IClienteService _clientes;
    private readonly IConsultaCnpj _consultaCnpj;
    private readonly ILogger<ClienteFormViewModel> _logger;
    private Guid? _id;

    public ClienteFormViewModel(IClienteService clientes, IConsultaCnpj consultaCnpj, ILogger<ClienteFormViewModel> logger)
    {
        _clientes = clientes;
        _consultaCnpj = consultaCnpj;
        _logger = logger;
    }

    /// <summary>Disparado ao salvar com sucesso (a listagem fecha o form e recarrega).</summary>
    public event Action? Salvo;

    /// <summary>Disparado ao cancelar/fechar (a listagem fecha o form).</summary>
    public event Action? Cancelado;

    public IReadOnlyList<TipoPessoa> TiposPessoa { get; } = new[] { TipoPessoa.Fisica, TipoPessoa.Juridica };

    [ObservableProperty] private TipoPessoa _tipoPessoa = TipoPessoa.Fisica;
    [ObservableProperty] private string _documento = string.Empty;
    [ObservableProperty] private string _razaoSocial = string.Empty;
    [ObservableProperty] private string? _nomeFantasia;
    [ObservableProperty] private string? _telefone;
    [ObservableProperty] private string? _email;
    [ObservableProperty] private string? _cep;
    [ObservableProperty] private string? _logradouro;
    [ObservableProperty] private string? _numero;
    [ObservableProperty] private string? _complemento;
    [ObservableProperty] private string? _bairro;
    [ObservableProperty] private string? _cidade;
    [ObservableProperty] private string? _uf;
    [ObservableProperty] private string? _observacao;
    [ObservableProperty] private decimal _limiteCredito;

    [ObservableProperty] private bool _modoVisualizacao = true;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    /// <summary>True para PJ — a View usa para mostrar/ocultar o campo Nome Fantasia.</summary>
    public bool EhJuridica => TipoPessoa == TipoPessoa.Juridica;

    public string TituloDocumento => TipoPessoa == TipoPessoa.Fisica ? "CPF" : "CNPJ";

    public string TituloRazaoSocial => TipoPessoa == TipoPessoa.Fisica ? "Nome" : "Razão Social";

    public string Titulo => _id is null ? "Novo Cliente" : $"Cliente {RazaoSocial}";

    /// <summary>Botão de consulta de CNPJ: só PJ, em edição e quando não está carregando.</summary>
    public bool PodeConsultarCnpj => EhJuridica && !ModoVisualizacao && !Carregando;

    partial void OnTipoPessoaChanged(TipoPessoa value)
    {
        OnPropertyChanged(nameof(EhJuridica));
        OnPropertyChanged(nameof(TituloDocumento));
        OnPropertyChanged(nameof(TituloRazaoSocial));
        OnPropertyChanged(nameof(PodeConsultarCnpj));
    }

    partial void OnModoVisualizacaoChanged(bool value) => OnPropertyChanged(nameof(PodeConsultarCnpj));
    partial void OnCarregandoChanged(bool value) => OnPropertyChanged(nameof(PodeConsultarCnpj));

    /// <summary>Prepara o formulário para um novo cadastro (limpo, em edição).</summary>
    public void PrepararNovo()
    {
        _id = null;
        TipoPessoa = TipoPessoa.Fisica;
        Documento = string.Empty;
        RazaoSocial = string.Empty;
        NomeFantasia = Telefone = Email = null;
        Cep = Logradouro = Numero = Complemento = Bairro = Cidade = Uf = Observacao = null;
        LimiteCredito = 0;
        MensagemErro = null;
        ModoVisualizacao = false;
        OnPropertyChanged(nameof(Titulo));
    }

    /// <summary>Carrega um cliente existente em modo visualização.</summary>
    public async Task CarregarAsync(Guid id)
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var resultado = await _clientes.ObterPorIdAsync(id);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var c = resultado.Valor;
            _id = c.Id;
            TipoPessoa = c.TipoPessoa;
            Documento = c.Documento;
            RazaoSocial = c.RazaoSocial;
            NomeFantasia = c.NomeFantasia;
            Telefone = c.Telefone;
            Email = c.Email;
            Cep = c.Cep;
            Logradouro = c.Logradouro;
            Numero = c.Numero;
            Complemento = c.Complemento;
            Bairro = c.Bairro;
            Cidade = c.Cidade;
            Uf = c.Uf;
            Observacao = c.Observacao;
            LimiteCredito = c.LimiteCredito;
            ModoVisualizacao = true;
            OnPropertyChanged(nameof(Titulo));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o cliente.";
            _logger.LogError(ex, "Erro ao carregar cliente {Id}.", id);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void HabilitarEdicao() => ModoVisualizacao = false;

    /// <summary>Consulta o CNPJ na Receita (BrasilAPI) e preenche os campos do cadastro.</summary>
    [RelayCommand]
    private async Task ConsultarCnpjAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var resultado = await _consultaCnpj.ConsultarAsync(Documento);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            var d = resultado.Valor;
            RazaoSocial = d.RazaoSocial;
            NomeFantasia = d.NomeFantasia;
            // Preenche contato/endereço só quando a Receita retorna (não apaga o que o usuário já digitou).
            if (d.Telefone is not null) Telefone = d.Telefone;
            if (d.Email is not null) Email = d.Email;
            if (d.Cep is not null) Cep = d.Cep;
            if (d.Logradouro is not null) Logradouro = d.Logradouro;
            if (d.Numero is not null) Numero = d.Numero;
            if (d.Complemento is not null) Complemento = d.Complemento;
            if (d.Bairro is not null) Bairro = d.Bairro;
            if (d.Cidade is not null) Cidade = d.Cidade;
            if (d.Uf is not null) Uf = d.Uf;
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao consultar o CNPJ.";
            _logger.LogError(ex, "Erro ao consultar CNPJ.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var dto = new SalvarClienteDto(
                TipoPessoa, Documento, RazaoSocial, NomeFantasia, Telefone, Email,
                Cep, Logradouro, Numero, Complemento, Bairro, Cidade, Uf, Observacao, LimiteCredito);

            var resultado = _id is null
                ? await _clientes.CriarAsync(dto)
                : await _clientes.AtualizarAsync(_id.Value, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Salvo?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao salvar o cliente.";
            _logger.LogError(ex, "Erro ao salvar cliente.");
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
