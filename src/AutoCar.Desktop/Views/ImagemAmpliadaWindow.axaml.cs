using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AutoCar.Desktop.Services;

namespace AutoCar.Desktop.Views;

/// <summary>
/// Janela simples que exibe a foto de um produto ampliada (fundo escuro, imagem centralizada).
/// Reutilizável: recebe o NOME do arquivo e carrega da pasta-base via <see cref="ImagemProduto"/>.
/// Fecha com Esc ou clique. Aberta modal pela tela que tem a miniatura.
/// </summary>
public partial class ImagemAmpliadaWindow : Window
{
    public ImagemAmpliadaWindow()
    {
        InitializeComponent();
    }

    public ImagemAmpliadaWindow(string? arquivoImagem) : this()
    {
        this.FindControl<Image>("Foto")!.Source = ImagemProduto.Carregar(arquivoImagem);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void AoClicar(object? sender, PointerPressedEventArgs e) => Close();

    private void AoTeclar(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}
