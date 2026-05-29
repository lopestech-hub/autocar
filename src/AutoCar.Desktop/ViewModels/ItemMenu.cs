using System.Collections.Generic;
using System.Linq;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.ViewModels;

/// <summary>
/// Item de módulo do sistema. Declara a categoria (menu superior de texto), o ícone
/// FontAwesome, se aparece na toolbar de atalhos e quais perfis podem vê-lo. A montagem
/// do menu/toolbar no shell filtra pelos perfis permitidos do usuário logado.
/// </summary>
public sealed class ItemMenu
{
    public ItemMenu(
        string titulo,
        string icone,
        string categoria,
        bool flgToolbar,
        IReadOnlyCollection<PerfilUsuario> perfis)
    {
        Titulo = titulo;
        Icone = icone;
        Categoria = categoria;
        FlgToolbar = flgToolbar;
        Perfis = perfis;
    }

    /// <summary>Rótulo exibido no menu e na legenda do ícone.</summary>
    public string Titulo { get; }

    /// <summary>Nome do ícone FontAwesome (ex: "fa-solid fa-users").</summary>
    public string Icone { get; }

    /// <summary>Categoria do menu superior de texto (ex: Cadastros, Movimentos, Financeiro).</summary>
    public string Categoria { get; }

    /// <summary>Se verdadeiro, aparece como atalho na toolbar de ícones.</summary>
    public bool FlgToolbar { get; }

    /// <summary>Perfis que enxergam este item.</summary>
    public IReadOnlyCollection<PerfilUsuario> Perfis { get; }

    public bool VisivelPara(PerfilUsuario perfil) =>
        perfil == PerfilUsuario.Admin || Perfis.Contains(perfil);
}
