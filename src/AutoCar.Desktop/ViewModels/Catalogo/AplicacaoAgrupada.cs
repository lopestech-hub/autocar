using System.Collections.Generic;

namespace AutoCar.Desktop.ViewModels.Catalogo;

/// <summary>
/// Aplicação da peça selecionada agrupada por montadora, para o painel de detalhe do Catálogo
/// (estilo Cofap): a montadora vira cabeçalho do grupo e os modelos aparecem abaixo.
/// Agrupamento feito no ViewModel — o Avalonia 11 tem suporte fraco a GroupStyle nativo.
/// </summary>
public sealed record GrupoAplicacaoVm(string Montadora, IReadOnlyList<LinhaAplicacaoVm> Linhas);

/// <summary>Uma linha de aplicação dentro do grupo: o modelo + a faixa de anos já formatada
/// ("1996-2001", "2019+", "até 2001" ou vazio) + a motorização, quando houver.</summary>
public sealed record LinhaAplicacaoVm(string Modelo, string Ano, string? Motorizacao);
