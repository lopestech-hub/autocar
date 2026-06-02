using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

/// <summary>
/// (Origem, CodDocumentoOrigem) → rótulo da origem do movimento no histórico: "Venda nº 1",
/// "Compra nº 5", "Devolução nº 2" ou "Manual". MultiBinding porque combina o tipo da origem com o
/// número do documento. Dado estruturado — não se confunde com a observação livre.
/// </summary>
public sealed class OrigemMovimentoRotuloConverter : IMultiValueConverter
{
    public static readonly OrigemMovimentoRotuloConverter Instancia = new();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not OrigemMovimento origem)
            return string.Empty;

        var cod = values[1] as int?;
        return origem switch
        {
            OrigemMovimento.Venda => cod is int v ? $"Venda nº {v}" : "Venda",
            OrigemMovimento.Compra => cod is int c ? $"Compra nº {c}" : "Compra",
            OrigemMovimento.Devolucao => cod is int d ? $"Devolução nº {d}" : "Devolução",
            _ => "Manual",
        };
    }
}

/// <summary>TipoMovimentoEstoque → rótulo amigável para o histórico (Entrada/Saída/Ajuste +/−).</summary>
public sealed class TipoMovimentoRotuloConverter : IValueConverter
{
    public static readonly TipoMovimentoRotuloConverter Instancia = new();

    public static string Rotular(TipoMovimentoEstoque tipo) => tipo switch
    {
        TipoMovimentoEstoque.Entrada => "Entrada",
        TipoMovimentoEstoque.Saida => "Saída",
        TipoMovimentoEstoque.AjustePositivo => "Ajuste (+)",
        TipoMovimentoEstoque.AjusteNegativo => "Ajuste (−)",
        _ => tipo.ToString(),
    };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoMovimentoEstoque t ? Rotular(t) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// TipoMovimentoEstoque → cor do texto do tipo no histórico. Entrada/ajuste+ em verde (soma),
/// saída/ajuste− em vermelho (subtrai) — cor comunica a direção do movimento, sem decoração.
/// </summary>
public sealed class TipoMovimentoCorConverter : IValueConverter
{
    public static readonly TipoMovimentoCorConverter Instancia = new();
    private static readonly IBrush Verde = new SolidColorBrush(Color.Parse("#166534"));
    private static readonly IBrush Vermelho = new SolidColorBrush(Color.Parse("#991B1B"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TipoMovimentoEstoque t
            && t is TipoMovimentoEstoque.Saida or TipoMovimentoEstoque.AjusteNegativo
            ? Vermelho : Verde;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>
/// DateTime (UTC do banco) → texto "dd/MM/yyyy HH:mm" em horário de Brasília. O banco grava em UTC;
/// usa o helper canônico do projeto (DataHora.ParaBrasilia, fuso America/Sao_Paulo), não ToLocalTime()
/// — que dependeria do fuso da máquina e erraria fora de UTC-3.
/// </summary>
public sealed class DataBrasiliaConverter : IValueConverter
{
    public static readonly DataBrasiliaConverter Instancia = new();
    private static readonly CultureInfo PtBr = new("pt-BR");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime data ? DataHora.ParaBrasilia(data).ToString("dd/MM/yyyy HH:mm", PtBr) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
