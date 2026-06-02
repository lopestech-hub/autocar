using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views;

/// <summary>
/// Janela de confirmação genérica (modal): título + mensagem + Cancelar/Confirmar. Reutilizável em
/// qualquer ação irreversível (faturar, inativar, etc.). Enter confirma (botão IsDefault), ESC cancela
/// (IsCancel). Use <see cref="MostrarAsync"/> e aguarde o bool.
/// </summary>
public partial class ConfirmacaoWindow : Window
{
    private bool _confirmado;

    public ConfirmacaoWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Abre a confirmação modal sobre <paramref name="dono"/> e devolve true se o usuário confirmar.
    /// </summary>
    /// <param name="dono">Janela-pai (a confirmação abre centralizada sobre ela).</param>
    /// <param name="titulo">Título curto da ação.</param>
    /// <param name="mensagem">Texto explicando a consequência.</param>
    /// <param name="textoConfirmar">Rótulo do botão de confirmação (ex: "Faturar").</param>
    public static async Task<bool> MostrarAsync(
        Window dono, string titulo, string mensagem, string textoConfirmar = "Confirmar")
    {
        var janela = new ConfirmacaoWindow();
        janela.FindControl<TextBlock>("Titulo")!.Text = titulo;
        janela.FindControl<TextBlock>("Mensagem")!.Text = mensagem;

        var confirmar = janela.FindControl<Button>("BotaoConfirmar")!;
        confirmar.Content = textoConfirmar;
        confirmar.Click += janela.AoConfirmar;

        await janela.ShowDialog(dono);
        return janela._confirmado;
    }

    private void AoConfirmar(object? sender, RoutedEventArgs e)
    {
        _confirmado = true;
        Close();
    }
}
