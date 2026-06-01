using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoCar.Application.Modules.Estoque;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Desktop.Converters;
using AutoCar.Domain.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AutoCar.Desktop.ViewModels.Inventory;

/// <summary>Opção de tipo de movimento para o combo (rótulo amigável + valor do enum).</summary>
public sealed record OpcaoTipoMovimento(TipoMovimentoEstoque Tipo, string Rotulo);

/// <summary>
/// Formulário de movimentação de estoque de UM produto (janela separada): mostra o saldo atual,
/// permite registrar Entrada/Saída/Ajuste com quantidade e observação, e lista o histórico de
/// movimentos do produto. O saldo só muda pelo domínio; aqui a UI só coleta os dados e chama o
/// serviço. Erros de regra (saldo insuficiente) e de concorrência vêm como mensagem amigável.
/// </summary>
public partial class MovimentoEstoqueFormViewModel : ViewModelBase
{
    private readonly IEstoqueService _estoque;
    private readonly ILogger<MovimentoEstoqueFormViewModel> _logger;
    private Guid _idUsuario;

    public MovimentoEstoqueFormViewModel(
        IEstoqueService estoque,
        ILogger<MovimentoEstoqueFormViewModel> logger)
    {
        _estoque = estoque;
        _logger = logger;
    }

    /// <summary>Disparado quando um movimento é registrado com sucesso — a janela fecha e a lista recarrega.</summary>
    public event Action? Concluido;

    /// <summary>Disparado ao cancelar — a janela fecha sem alterar nada.</summary>
    public event Action? Cancelado;

    public Guid IdProduto { get; private set; }

    [ObservableProperty] private int _codProduto;
    [ObservableProperty] private string _descricaoProduto = string.Empty;
    [ObservableProperty] private string _unidade = string.Empty;
    [ObservableProperty] private int _saldoAtual;

    /// <summary>Tipos de movimento disponíveis no combo. O rótulo vem do conversor (fonte única do
    /// texto, compartilhada com o histórico) — evita divergência entre combo e listagem.</summary>
    public IReadOnlyList<OpcaoTipoMovimento> TiposMovimento { get; } = new[]
    {
        TipoMovimentoEstoque.Entrada,
        TipoMovimentoEstoque.Saida,
        TipoMovimentoEstoque.AjustePositivo,
        TipoMovimentoEstoque.AjusteNegativo,
    }.Select(t => new OpcaoTipoMovimento(t, TipoMovimentoRotuloConverter.Rotular(t))).ToList();

    [ObservableProperty] private OpcaoTipoMovimento? _tipoSelecionado;
    [ObservableProperty] private string? _observacao;

    private int _quantidade = 1;

    /// <summary>Quantidade a movimentar (inteira positiva). Exposta como texto para o TextBox —
    /// o projeto evita NumericUpDown (estiliza mal no Fluent). Parse tolerante; valor inválido
    /// ou ≤0 mantém 1.</summary>
    public string QuantidadeTexto
    {
        get => _quantidade.ToString();
        set
        {
            _quantidade = int.TryParse(value, out var q) && q > 0 ? q : 1;
            OnPropertyChanged();
        }
    }

    /// <summary>Quantidade efetiva usada na movimentação (sempre ≥ 1).</summary>
    public int Quantidade => _quantidade;

    [ObservableProperty] private bool _processando;
    [ObservableProperty] private string? _mensagemErro;

    /// <summary>Histórico de movimentos do produto (mais recentes primeiro).</summary>
    public ObservableCollection<MovimentoEstoqueDto> Movimentos { get; } = new();

    /// <summary>Prepara o formulário para um produto: guarda os dados e carrega saldo + histórico.</summary>
    public async Task PrepararAsync(Guid idUsuario, Guid idProduto, int codProduto, string descricao, string unidade)
    {
        _idUsuario = idUsuario;
        IdProduto = idProduto;
        CodProduto = codProduto;
        DescricaoProduto = descricao;
        Unidade = unidade;
        TipoSelecionado = TiposMovimento[0]; // Entrada por padrão
        QuantidadeTexto = "1";
        Observacao = null;
        MensagemErro = null;

        await CarregarAsync();
    }

    /// <summary>Recarrega o histórico de movimentos e atualiza o saldo atual (saldo = último saldo após).</summary>
    private async Task CarregarAsync()
    {
        try
        {
            var movimentos = await _estoque.ListarMovimentosAsync(IdProduto);
            Movimentos.Clear();
            foreach (var m in movimentos)
                Movimentos.Add(m);

            // O saldo atual é o saldo após o movimento mais recente (ou zero se não há movimentos).
            SaldoAtual = movimentos.Count > 0 ? movimentos[0].QtdSaldoApos : 0;
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao carregar o histórico de movimentos.";
            _logger.LogError(ex, "Erro ao listar movimentos do produto {IdProduto}.", IdProduto);
        }
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (TipoSelecionado is null)
        {
            MensagemErro = "Selecione o tipo de movimento.";
            return;
        }

        Processando = true;
        MensagemErro = null;
        try
        {
            var dto = new MovimentarEstoqueDto(IdProduto, TipoSelecionado.Tipo, Quantidade, Observacao);
            var resultado = await _estoque.MovimentarAsync(_idUsuario, dto);

            if (resultado.Falha)
            {
                MensagemErro = resultado.Erro.Mensagem;
                return;
            }

            Concluido?.Invoke();
        }
        catch (Exception ex)
        {
            MensagemErro = "Falha ao registrar o movimento.";
            _logger.LogError(ex, "Erro ao movimentar estoque do produto {IdProduto}.", IdProduto);
        }
        finally
        {
            Processando = false;
        }
    }

    [RelayCommand]
    private void Cancelar() => Cancelado?.Invoke();
}
