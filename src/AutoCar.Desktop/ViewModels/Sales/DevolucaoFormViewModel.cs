using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Sales.Devolucoes;
using AutoCar.Application.Modules.Sales.Devolucoes.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Sales;

/// <summary>
/// Formulário de devolução de uma venda faturada (janela separada). Lista os itens devolvíveis (com o
/// saldo vendido − já devolvido), deixa o usuário escolher as quantidades, e registra a devolução
/// repondo o estoque. A confirmação (ação que mexe no estoque) é pedida via evento, como no faturamento.
/// </summary>
public partial class DevolucaoFormViewModel : ViewModelBase
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    private readonly IDevolucaoService _devolucoes;
    private readonly ILogger<DevolucaoFormViewModel> _logger;

    private Guid _idPreVenda;
    private Guid _idUsuario;

    public DevolucaoFormViewModel(IDevolucaoService devolucoes, ILogger<DevolucaoFormViewModel> logger)
    {
        _devolucoes = devolucoes;
        _logger = logger;
    }

    /// <summary>Disparado quando a devolução é registrada com sucesso — a janela fecha e a lista recarrega.</summary>
    public event Action? Concluido;

    /// <summary>Disparado ao cancelar — a janela fecha sem alterar nada.</summary>
    public event Action? Cancelado;

    /// <summary>Disparado ao clicar em Devolver: a janela mostra a confirmação e, se confirmada, chama
    /// <see cref="ConfirmarDevolucaoAsync"/>.</summary>
    public event Action? ConfirmacaoDevolucaoSolicitada;

    public ObservableCollection<DevolucaoItemViewModel> Itens { get; } = new();

    [ObservableProperty] private int _codPreVenda;
    [ObservableProperty] private string? _motivo;
    [ObservableProperty] private bool _carregando;
    [ObservableProperty] private string? _mensagemErro;

    public string Titulo => $"Devolução da Venda Nº {CodPreVenda}";

    partial void OnCodPreVendaChanged(int value) => OnPropertyChanged(nameof(Titulo));

    /// <summary>Prepara a janela para devolver itens de uma venda faturada: carrega os devolvíveis.</summary>
    public async Task PrepararAsync(Guid idPreVenda, int codPreVenda, Guid idUsuario)
    {
        _idPreVenda = idPreVenda;
        _idUsuario = idUsuario;
        CodPreVenda = codPreVenda;
        Motivo = null;
        MensagemErro = null;

        Carregando = true;
        try
        {
            var resultado = await _devolucoes.ListarItensDevolviveisAsync(idPreVenda);
            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Itens.Clear();
            foreach (var item in resultado.Valor)
                Itens.Add(new DevolucaoItemViewModel(item));
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar os itens devolvíveis.";
            _logger.LogError(ex, "Erro ao listar itens devolvíveis da venda {Id}.", idPreVenda);
        }
        finally
        {
            Carregando = false;
        }
    }

    /// <summary>Botão Devolver: valida que há item selecionado e pede a confirmação à janela.</summary>
    [RelayCommand]
    private void Devolver()
    {
        if (!Itens.Any(i => i.Selecionado))
        {
            MensagemErro = "Informe a quantidade de ao menos um item para devolver.";
            return;
        }
        MensagemErro = null;
        ConfirmacaoDevolucaoSolicitada?.Invoke();
    }

    /// <summary>Efetiva a devolução (após confirmada): registra e repõe o estoque. Em sucesso, fecha.</summary>
    public async Task ConfirmarDevolucaoAsync()
    {
        Carregando = true;
        MensagemErro = null;
        try
        {
            var itens = Itens
                .Where(i => i.Selecionado)
                .Select(i => new DevolucaoItemDto(i.IdProduto, i.QtdDevolver))
                .ToList();

            var dto = new CriarDevolucaoDto(_idPreVenda, Motivo, itens);
            var resultado = await _devolucoes.CriarAsync(_idUsuario, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Concluido?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao registrar a devolução.";
            _logger.LogError(ex, "Erro ao registrar devolução da venda {Id}.", _idPreVenda);
        }
        finally
        {
            Carregando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
