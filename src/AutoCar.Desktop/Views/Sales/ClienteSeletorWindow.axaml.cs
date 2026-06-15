using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AutoCar.Application.Modules.Registrations.Clientes;
using AutoCar.Application.Modules.Registrations.Clientes.DTOs;

namespace AutoCar.Desktop.Views.Sales;

/// <summary>
/// Janela seletora de cliente (aberta com F3 na pré-venda). Busca por nome OU código (só dígitos
/// = código), navegação por setas, Enter/duplo-clique seleciona, Esc fecha. Tabela renderizada por
/// <see cref="ListBox"/> virtualizado. Reusa IClienteService — não duplica busca.
/// </summary>
public partial class ClienteSeletorWindow : Window
{
    private readonly IClienteService? _clientes;
    private readonly Action<ClienteListaDto?>? _aoSelecionar;
    private ListBox? _lista;
    private CancellationTokenSource? _debounce;

    /// <summary>Resultado da busca, ligado ao ListBox (DataContext = a própria janela).</summary>
    public ObservableCollection<ClienteListaDto> Resultado { get; } = new();

    public ClienteSeletorWindow()
    {
        InitializeComponent();
    }

    public ClienteSeletorWindow(IClienteService clientes, Action<ClienteListaDto?> aoSelecionar) : this()
    {
        _clientes = clientes;
        _aoSelecionar = aoSelecionar;
        DataContext = this;

        _lista = this.FindControl<ListBox>("ListaResultado");
        if (_lista is not null)
            _lista.DoubleTapped += (_, _) => SelecionarAtual();

        var busca = this.FindControl<TextBox>("CampoBusca")!;
        busca.TextChanged += (_, _) => AgendarBusca(busca.Text);

        Opened += async (_, _) => { busca.Focus(); await BuscarAsync(null); };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AgendarBusca(string? termo)
    {
        _debounce?.Cancel();
        _debounce = new CancellationTokenSource();
        var token = _debounce.Token;
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                    await Dispatcher.UIThread.InvokeAsync(() => BuscarAsync(termo));
            }
            catch (TaskCanceledException) { }
        });
    }

    private async Task BuscarAsync(string? termo)
    {
        if (_clientes is null)
            return;
        // O ListarAsync do serviço já filtra por nome/razão social ou documento. Para código,
        // filtramos em memória (só dígitos → casa com cod_cliente).
        var lista = await _clientes.ListarAsync(termo);
        IEnumerable<ClienteListaDto> filtrada = lista;

        var t = (termo ?? string.Empty).Trim();
        if (t.Length > 0 && t.All(char.IsDigit))
            filtrada = lista.Where(c => c.CodCliente.ToString().Contains(t));

        Resultado.Clear();
        foreach (var c in filtrada)
            Resultado.Add(c);

        // 1ª linha já marcada (fluxo de teclado imediato).
        if (_lista is not null && Resultado.Count > 0)
            _lista.SelectedIndex = 0;
    }

    private void SelecionarAtual()
    {
        if (_lista?.SelectedItem is ClienteListaDto c)
        {
            _aoSelecionar?.Invoke(c);
            Close();
        }
    }

    private void AoTeclar(object? sender, KeyEventArgs e)
    {
        if (_lista is null) return;
        switch (e.Key)
        {
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
            case Key.Down:
                if (_lista.ItemCount > 0)
                    _lista.SelectedIndex = Math.Min(_lista.SelectedIndex + 1, _lista.ItemCount - 1);
                e.Handled = true;
                break;
            case Key.Up:
                if (_lista.ItemCount > 0)
                    _lista.SelectedIndex = Math.Max(_lista.SelectedIndex - 1, 0);
                e.Handled = true;
                break;
            case Key.Enter:
                SelecionarAtual();
                e.Handled = true;
                break;
        }
    }

    private void AoEscolherConsumidor(object? sender, RoutedEventArgs e)
    {
        _aoSelecionar?.Invoke(null); // null = Consumidor (venda avulsa)
        Close();
    }

    private void AoFechar(object? sender, RoutedEventArgs e) => Close();
}
