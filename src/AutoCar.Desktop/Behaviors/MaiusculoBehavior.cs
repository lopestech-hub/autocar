using Avalonia;
using Avalonia.Controls;

namespace AutoCar.Desktop.Behaviors;

/// <summary>
/// Comportamento anexável que força o texto de um <see cref="TextBox"/> para CAIXA ALTA
/// enquanto o usuário digita (padrão ERP). O que aparece no campo é exatamente o que será
/// salvo — a entidade de domínio também normaliza para maiúscula, então fica consistente
/// ponta a ponta. Aplicado seletivamente (ex: descrição, razão social), nunca em e-mail
/// ou observação.
///
/// Uso no AXAML:
///   xmlns:b="using:AutoCar.Desktop.Behaviors"
///   &lt;TextBox b:MaiusculoBehavior.Ativo="True" ... /&gt;
/// </summary>
public static class MaiusculoBehavior
{
    public static readonly AttachedProperty<bool> AtivoProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>("Ativo", typeof(MaiusculoBehavior));

    public static void SetAtivo(TextBox obj, bool value) => obj.SetValue(AtivoProperty, value);

    public static bool GetAtivo(TextBox obj) => obj.GetValue(AtivoProperty);

    static MaiusculoBehavior()
    {
        AtivoProperty.Changed.AddClassHandler<TextBox>((textBox, e) =>
        {
            if (e.NewValue is true)
                textBox.TextChanged += AoMudarTexto;
            else
                textBox.TextChanged -= AoMudarTexto;
        });
    }

    private static void AoMudarTexto(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || string.IsNullOrEmpty(textBox.Text))
            return;

        var maiusculo = textBox.Text.ToUpperInvariant();
        if (maiusculo == textBox.Text)
            return; // já está em maiúscula — evita loop e preserva o cursor

        // Preserva a posição do cursor: converter não muda o tamanho do texto.
        var posicao = textBox.CaretIndex;
        textBox.Text = maiusculo;
        textBox.CaretIndex = posicao;
    }
}
