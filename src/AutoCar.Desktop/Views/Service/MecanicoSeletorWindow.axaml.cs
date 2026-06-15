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
using AutoCar.Application.Modules.Registrations.Mecanicos;
using AutoCar.Application.Modules.Registrations.Mecanicos.DTOs;

namespace AutoCar.Desktop.Views.Service;

/// <summary>
/// Janela seletora de mecânico (aberta com F5 na OS). Busca por nome OU código (só dígitos
/// = código), navegação por setas, Enter/duplo-clique seleciona, Esc fecha. Tabela renderizada por
/// <see cref="ListBox"/> virtualizado. Reusa IMecanicoService — não duplica busca. O mecânico é
/// opcional na OS: o botão "Sem mecânico" invoca o callback com null.
/// </summary>
public partial class MecanicoSeletorWindow : Window
{
    private readonly IMecanicoService? _mecanicos;
    private readonly Action<MecanicoDto?>? _aoSelecionar;
    private ListBox? _lista;
    private CancellationTokenSource? _debounce;

    /// <summary>Resultado da busca, ligado ao ListBox (DataContext = a própria janela).</summary>
    public ObservableCollection<MecanicoDto> Resultado { get; } = new();

    public MecanicoSeletorWindow()
    {
        InitializeComponent();
    }

    public MecanicoSeletorWindow(IMecanicoService mecanicos, Action<MecanicoDto?> aoSelecionar) : this()
    {
        _mecanicos = mecanicos;
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
        if (_mecanicos is null)
            return;
        // O ListarAsync do serviço já filtra por nome. Para código, filtramos em memória
        // (só dígitos → casa com cod_mecanico).
        var lista = await _mecanicos.ListarAsync(termo);
        IEnumerable<MecanicoDto> filtrada = lista;

        var t = (termo ?? string.Empty).Trim();
        if (t.Length > 0 && t.All(char.IsDigit))
            filtrada = lista.Where(m => m.CodMecanico.ToString().Contains(t));

        Resultado.Clear();
        foreach (var m in filtrada)
            Resultado.Add(m);

        if (_lista is not null && Resultado.Count > 0)
            _lista.SelectedIndex = 0;
    }

    private void SelecionarAtual()
    {
        if (_lista?.SelectedItem is MecanicoDto m)
        {
            _aoSelecionar?.Invoke(m);
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

    private void AoLimparMecanico(object? sender, RoutedEventArgs e)
    {
        _aoSelecionar?.Invoke(null); // null = OS sem mecânico definido
        Close();
    }

    private void AoFechar(object? sender, RoutedEventArgs e) => Close();
}
