using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

// Badges de situação (Pré-venda e OS): fundo claro + borda 1px da cor forte da família + texto.
// Cor = informação (etiqueta que salta da linha). Mesmas cores do code-behind antigo.
// Helpers internos para não repetir o switch em cada converter.

internal static class CoresSituacao
{
    // (fundo, borda, texto, rótulo) por situação de Pré-venda.
    public static (string fundo, string borda, string texto, string rotulo) PreVenda(SituacaoPreVenda s) => s switch
    {
        SituacaoPreVenda.Faturada => ("#DCFCE7", "#22C55E", "#166534", "Faturada"),
        SituacaoPreVenda.Cancelada => ("#FEE2E2", "#EF4444", "#991B1B", "Cancelada"),
        _ => ("#D6E4F2", "#1E5CA5", "#143F70", "Aberta"),
    };

    // (fundo, borda, texto, rótulo) por situação de Ordem de Serviço (5 estados).
    public static (string fundo, string borda, string texto, string rotulo) Ordem(SituacaoOrdemServico s) => s switch
    {
        SituacaoOrdemServico.EmAndamento => ("#FEF3C7", "#F59E0B", "#92400E", "Em andamento"),
        SituacaoOrdemServico.Concluida => ("#E0E7FF", "#6366F1", "#3730A3", "Concluída"),
        SituacaoOrdemServico.Faturada => ("#DCFCE7", "#22C55E", "#166534", "Faturada"),
        SituacaoOrdemServico.Cancelada => ("#FEE2E2", "#EF4444", "#991B1B", "Cancelada"),
        _ => ("#D6E4F2", "#1E5CA5", "#143F70", "Aberta"),
    };

    public static IBrush Brush(string hex) => new SolidColorBrush(Color.Parse(hex));
}

// ===== Pré-venda =====
public sealed class SituacaoPreVendaTextoConverter : IValueConverter
{
    public static readonly SituacaoPreVendaTextoConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        v is SituacaoPreVenda s ? CoresSituacao.PreVenda(s).rotulo : "";
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SituacaoPreVendaFundoConverter : IValueConverter
{
    public static readonly SituacaoPreVendaFundoConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        CoresSituacao.Brush(v is SituacaoPreVenda s ? CoresSituacao.PreVenda(s).fundo : "#D6E4F2");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SituacaoPreVendaBordaConverter : IValueConverter
{
    public static readonly SituacaoPreVendaBordaConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        CoresSituacao.Brush(v is SituacaoPreVenda s ? CoresSituacao.PreVenda(s).borda : "#1E5CA5");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SituacaoPreVendaTextoCorConverter : IValueConverter
{
    public static readonly SituacaoPreVendaTextoCorConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        CoresSituacao.Brush(v is SituacaoPreVenda s ? CoresSituacao.PreVenda(s).texto : "#143F70");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

// ===== Ordem de Serviço =====
public sealed class SituacaoOrdemTextoConverter : IValueConverter
{
    public static readonly SituacaoOrdemTextoConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        v is SituacaoOrdemServico s ? CoresSituacao.Ordem(s).rotulo : "";
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SituacaoOrdemFundoConverter : IValueConverter
{
    public static readonly SituacaoOrdemFundoConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        CoresSituacao.Brush(v is SituacaoOrdemServico s ? CoresSituacao.Ordem(s).fundo : "#D6E4F2");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SituacaoOrdemBordaConverter : IValueConverter
{
    public static readonly SituacaoOrdemBordaConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        CoresSituacao.Brush(v is SituacaoOrdemServico s ? CoresSituacao.Ordem(s).borda : "#1E5CA5");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SituacaoOrdemTextoCorConverter : IValueConverter
{
    public static readonly SituacaoOrdemTextoCorConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        CoresSituacao.Brush(v is SituacaoOrdemServico s ? CoresSituacao.Ordem(s).texto : "#143F70");
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

// ===== Data UTC -> Brasília, só data (dd/MM/yyyy) =====
// O banco grava em UTC; usa o helper canônico DataHora.ParaBrasilia (fuso America/Sao_Paulo),
// não ToLocalTime() (que dependeria do fuso da máquina). O DataBrasiliaConverter existente
// (MovimentoEstoqueConverters) inclui a hora; nas LISTAGENS só queremos a data.
public sealed class DataBrasiliaCurtaConverter : IValueConverter
{
    public static readonly DataBrasiliaCurtaConverter Instancia = new();
    private static readonly CultureInfo PtBr = new("pt-BR");
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        v is DateTime dt ? AutoCar.Domain.Common.DataHora.ParaBrasilia(dt).ToString("dd/MM/yyyy", PtBr) : "";
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

// ===== Veículo da OS: modelo + placa (MultiValue [0]=modelo, [1]=placa) =====
// Com placa: "MODELO - PLACA". Sem placa: só o modelo. Sem nada: vazio (sem traço).
public sealed class VeiculoConverter : IMultiValueConverter
{
    public static readonly VeiculoConverter Instancia = new();
    public object? Convert(System.Collections.Generic.IList<object?> values, Type t, object? p, CultureInfo c)
    {
        var modelo = values.Count > 0 ? values[0] as string : null;
        var placa = values.Count > 1 ? values[1] as string : null;
        if (string.IsNullOrWhiteSpace(modelo) && string.IsNullOrWhiteSpace(placa)) return "";
        if (string.IsNullOrWhiteSpace(placa)) return modelo;
        if (string.IsNullOrWhiteSpace(modelo)) return placa;
        return $"{modelo} - {placa}";
    }
}

// ===== Saldo de estoque (realce: > 0 escuro/SemiBold; zero cinza) =====
public sealed class SaldoCorConverter : IValueConverter
{
    public static readonly SaldoCorConverter Instancia = new();
    private static readonly IBrush Positivo = new SolidColorBrush(Color.Parse("#1E293B"));
    private static readonly IBrush Zero = new SolidColorBrush(Color.Parse("#94A3B8"));
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        v is int n && n > 0 ? Positivo : Zero;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class SaldoPesoConverter : IValueConverter
{
    public static readonly SaldoPesoConverter Instancia = new();
    public object Convert(object? v, Type t, object? p, CultureInfo c) =>
        v is int n && n > 0 ? FontWeight.SemiBold : FontWeight.Normal;
    public object ConvertBack(object? v, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}
