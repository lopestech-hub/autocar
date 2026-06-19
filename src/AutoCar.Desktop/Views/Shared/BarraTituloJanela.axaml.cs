using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views.Shared;

/// <summary>
/// Barra de título customizada (azul Cofap) reutilizável pelas janelas que estendem a área de
/// cliente sobre a decoração do Windows (ExtendClientAreaToDecorationsHint). Acha a janela-mãe
/// sozinha e cuida de arrastar, minimizar, maximizar/restaurar e fechar. O botão maximizar só
/// aparece quando a janela é redimensionável; o minimizar pode ser ocultado em diálogos modais.
/// </summary>
public partial class BarraTituloJanela : UserControl
{
    /// <summary>Exibe o botão minimizar. Desligar em diálogos modais (Confirmação, etc.).</summary>
    public static readonly StyledProperty<bool> MostrarMinimizarProperty =
        AvaloniaProperty.Register<BarraTituloJanela, bool>(nameof(MostrarMinimizar), defaultValue: true);

    public bool MostrarMinimizar
    {
        get => GetValue(MostrarMinimizarProperty);
        set => SetValue(MostrarMinimizarProperty, value);
    }

    public BarraTituloJanela()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private Window? Janela => TopLevel.GetTopLevel(this) as Window;

    // Arrastar a janela pela barra (só com o botão esquerdo).
    private void OnBarraPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            Janela?.BeginMoveDrag(e);
    }

    // Duplo-clique na barra alterna maximizar/restaurar (comportamento padrão do Windows).
    private void OnBarraDoubleTapped(object? sender, TappedEventArgs e) => AlternarMaximizar();

    private void OnMinimizarClick(object? sender, RoutedEventArgs e)
    {
        if (Janela is { } janela)
            janela.WindowState = WindowState.Minimized;
    }

    private void OnMaximizarClick(object? sender, RoutedEventArgs e) => AlternarMaximizar();

    private void OnFecharClick(object? sender, RoutedEventArgs e) => Janela?.Close();

    private void AlternarMaximizar()
    {
        if (Janela is not { CanResize: true } janela)
            return;

        janela.WindowState = janela.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }
}
