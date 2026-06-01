using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace AutoCar.Desktop.Behaviors;

/// <summary>
/// Comportamento anexável que SELECIONA TODO o conteúdo do <see cref="TextBox"/> ao receber foco
/// (clique ou Tab). Pensado para campos numéricos de fluxo rápido (PDV): o vendedor clica e digita
/// o novo valor por cima, sem apagar o anterior nem posicionar o cursor. Ex: quantidade "1" fica
/// selecionada e vira "5" direto.
///
/// Uso no AXAML:
///   xmlns:b="using:AutoCar.Desktop.Behaviors"
///   &lt;TextBox b:SelecionarTudoBehavior.Ativo="True" ... /&gt;
/// </summary>
public static class SelecionarTudoBehavior
{
    public static readonly AttachedProperty<bool> AtivoProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("Ativo", typeof(SelecionarTudoBehavior));

    public static void SetAtivo(TextBox obj, bool value) => obj.SetValue(AtivoProperty, value);

    public static bool GetAtivo(TextBox obj) => obj.GetValue(AtivoProperty);

    static SelecionarTudoBehavior()
    {
        AtivoProperty.Changed.AddClassHandler<TextBox>((textBox, e) =>
        {
            if (e.NewValue is true)
            {
                textBox.GotFocus += AoGanharFoco;
                // Tunnel: roda ANTES do TextBox tratar o clique e mover o cursor — assim
                // selecionamos quando o campo ainda não tinha foco, sem a seleção "piscar e sumir".
                textBox.AddHandler(InputElement.PointerPressedEvent, AoClicar, RoutingStrategies.Tunnel);
            }
            else
            {
                textBox.GotFocus -= AoGanharFoco;
                textBox.RemoveHandler(InputElement.PointerPressedEvent, AoClicar);
            }
        });
    }

    // Foco via teclado (Tab) ou programático: seleciona tudo. Adiado para rodar após o
    // processamento do foco, garantindo que a seleção não seja desfeita em seguida.
    private static void AoGanharFoco(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is TextBox tb)
            Dispatcher.UIThread.Post(() => SelecionarTudo(tb));
    }

    // Clique com o campo ainda SEM foco: seleciona tudo e marca o evento como tratado, para o
    // TextBox não reposicionar o cursor no ponto do clique (o que desfaria a seleção). Cliques
    // subsequentes (campo já focado) seguem normais — o usuário pode posicionar o cursor à mão.
    private static void AoClicar(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBox tb || tb.IsFocused)
            return;

        tb.Focus();
        SelecionarTudo(tb);
        e.Handled = true;
    }

    private static void SelecionarTudo(TextBox tb)
    {
        if (!string.IsNullOrEmpty(tb.Text))
            tb.SelectAll();
    }
}
