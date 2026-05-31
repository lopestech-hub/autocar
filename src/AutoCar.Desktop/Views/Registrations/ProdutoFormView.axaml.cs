using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views.Registrations;

public partial class ProdutoFormView : UserControl
{
    public ProdutoFormView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
