using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views.Registrations;

public partial class PosicaoFormView : UserControl
{
    public PosicaoFormView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
