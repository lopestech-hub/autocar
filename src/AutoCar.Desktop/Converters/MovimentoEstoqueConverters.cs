using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AutoCar.Application.Modules.Estoque.DTOs;
using AutoCar.Domain.Common;
using AutoCar.Domain.Enums;

namespace AutoCar.Desktop.Converters;

/// <summary>OrigemMovimento → rótulo só do TIPO da origem ("Venda", "Compra", "Devolução", "Manual").
/// O número do documento fica em coluna própria (ver <see cref="DocumentoOrigemConverter"/>).</summary>
public sealed class OrigemMovimentoRotuloConverter : IValueConverter
{
    public static readonly OrigemMovimentoRotuloConverter Instancia = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is OrigemMovimento origem
            ? origem switch
            {
                OrigemMovimento.Venda => "Venda",
                OrigemMovimento.Compra => "Compra",
                OrigemMovimento.Devolucao => "Devolução",
                _ => "Manual",
            }
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>CodDocumentoOrigem (int?) → número do documento da movimentação para a coluna DOCUMENTO.
/// Vazio quando não há documento (ex: movimento manual). Mostra só o número, sem prefixo de tipo.</summary>
public sealed class DocumentoOrigemConverter : IValueConverter
{
    public static readonly DocumentoOrigemConverter Instancia = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int cod ? cod.ToString() : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
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
