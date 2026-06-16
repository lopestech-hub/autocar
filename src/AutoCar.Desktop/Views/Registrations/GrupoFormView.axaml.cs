using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AutoCar.Desktop.Views.Registrations;

public partial class GrupoFormView : UserControl
{
    public GrupoFormView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
