using AutoCar.Desktop.ViewModels;

namespace AutoCar.Desktop.Navegacao;

/// <summary>
/// Resolve a rota de um módulo no ViewModel da sua tela. Mantém o shell desacoplado
/// dos serviços de cada módulo — o navegador conhece o mapa rota → ViewModel.
/// </summary>
public interface INavegador
{
    /// <summary>Retorna o ViewModel da tela da rota, ou null se a rota não tem tela ainda.</summary>
    ViewModelBase? Resolver(string? rota);
}
