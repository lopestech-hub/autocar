using System;
using AutoCar.Application.Modules.Sales.Devolucoes.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoCar.Desktop.ViewModels.Sales;

/// <summary>
/// Linha da tela de devolução: um produto da venda com o saldo devolvível e a quantidade que o
/// usuário escolhe devolver (0 = não devolve este item). A quantidade é digitada como texto (parse
/// tolerante; o projeto evita NumericUpDown) e nunca passa do devolvível.
/// </summary>
public partial class DevolucaoItemViewModel : ViewModelBase
{
    public DevolucaoItemViewModel(ItemDevolvivelDto dto)
    {
        IdProduto = dto.IdProduto;
        DescricaoProduto = dto.DescricaoProduto;
        VlrUnitario = dto.VlrUnitario;
        QtdVendida = dto.QtdVendida;
        QtdJaDevolvida = dto.QtdJaDevolvida;
        QtdDevolvivel = dto.QtdDevolvivel;
    }

    public Guid IdProduto { get; }
    public string DescricaoProduto { get; }
    public decimal VlrUnitario { get; }
    public int QtdVendida { get; }
    public int QtdJaDevolvida { get; }

    /// <summary>Máximo que ainda pode ser devolvido deste item (vendido − já devolvido).</summary>
    public int QtdDevolvivel { get; }

    private int _qtdDevolver;

    /// <summary>Quantidade a devolver (texto editável). Limitada a [0, devolvível]; inválido vira 0.</summary>
    public string QtdDevolverTexto
    {
        get => _qtdDevolver.ToString();
        set
        {
            var q = int.TryParse(value, out var parsed) && parsed > 0 ? parsed : 0;
            _qtdDevolver = q > QtdDevolvivel ? QtdDevolvivel : q;
            OnPropertyChanged();
        }
    }

    /// <summary>Quantidade efetiva a devolver (≥0, nunca acima do devolvível).</summary>
    public int QtdDevolver => _qtdDevolver;

    /// <summary>True quando este item entra na devolução (quantidade > 0).</summary>
    public bool Selecionado => _qtdDevolver > 0;
}
