using System.Collections.Generic;

namespace AutoCar.Desktop.ViewModels;

/// <summary>
/// Categoria do menu superior de texto (ex: "Cadastros") com os itens visíveis
/// que pertencem a ela. Montada no shell já filtrada pelo perfil do usuário.
/// </summary>
public sealed class CategoriaMenu
{
    public CategoriaMenu(string nome, IReadOnlyList<ItemMenu> itens)
    {
        Nome = nome;
        Itens = itens;
    }

    public string Nome { get; }

    public IReadOnlyList<ItemMenu> Itens { get; }
}
