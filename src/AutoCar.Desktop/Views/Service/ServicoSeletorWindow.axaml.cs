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
using AutoCar.Application.Modules.Registrations.Servicos;
using AutoCar.Application.Modules.Registrations.Servicos.DTOs;

namespace AutoCar.Desktop.Views.Service;

/// <summary>
/// Janela seletora de serviço (aberta com F4 na OS). Busca por descrição OU código (só dígitos
/// = código), navegação por setas, Enter/duplo-clique seleciona, Esc fecha. Tabela renderizada por
/// <see cref="ListBox"/> virtualizado. Reusa IServicoService — não duplica busca.
/// </summary>
public partial class ServicoSeletorWindow : Window
{
    private readonly IServicoService? _servicos;
    private readonly Action<ServicoDto>? _aoSelecionar;
    private ListBox? _lista;
    private CancellationTokenSource? _debounce;

    /// <summary>Resultado da busca, ligado ao ListBox (DataContext = a própria janela).</summary>
    public ObservableCollection<ServicoDto> Resultado { get; } = new();

    public ServicoSeletorWindow()
    {
        InitializeComponent();
    }

    public ServicoSeletorWindow(IServicoService servicos, Action<ServicoDto> aoSelecionar) : this()
    {
        _servicos = servicos;
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
        if (_servicos is null)
            return;
        // O ListarAsync do serviço já filtra por descrição. Para código, filtramos em memória
        // (só dígitos → casa com cod_servico).
        var lista = await _servicos.ListarAsync(termo);
        IEnumerable<ServicoDto> filtrada = lista;

        var t = (termo ?? string.Empty).Trim();
        if (t.Length > 0 && t.All(char.IsDigit))
            filtrada = lista.Where(s => s.CodServico.ToString().Contains(t));

        Resultado.Clear();
        foreach (var s in filtrada)
            Resultado.Add(s);

        if (_lista is not null && Resultado.Count > 0)
            _lista.SelectedIndex = 0;
    }

    private void SelecionarAtual()
    {
        if (_lista?.SelectedItem is ServicoDto s)
        {
            _aoSelecionar?.Invoke(s);
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

    private void AoFechar(object? sender, RoutedEventArgs e) => Close();
}
