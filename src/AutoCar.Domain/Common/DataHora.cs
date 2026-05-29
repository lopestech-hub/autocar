namespace AutoCar.Domain.Common;

/// <summary>
/// Fonte única de horário do sistema. Padrão adotado: PERSISTIR EM UTC,
/// EXIBIR EM BRASÍLIA. O PostgreSQL (timestamptz) só aceita UTC; a conversão
/// para America/Sao_Paulo (UTC-3) acontece apenas na camada de apresentação.
/// </summary>
public static class DataHora
{
    private static readonly TimeZoneInfo FusoBrasil =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>Instante atual em UTC (Kind=Utc) — usado para gravar no banco.</summary>
    public static DateTime AgoraUtc() => DateTime.UtcNow;

    /// <summary>Converte um horário UTC vindo do banco para o fuso de Brasília (exibição).</summary>
    public static DateTime ParaBrasilia(DateTime utc)
    {
        var origem = utc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utc, DateTimeKind.Utc)
            : utc;
        return TimeZoneInfo.ConvertTimeFromUtc(origem.ToUniversalTime(), FusoBrasil);
    }

    /// <summary>Horário atual já no fuso de Brasília (para exibição direta).</summary>
    public static DateTime AgoraBrasilia() => ParaBrasilia(DateTime.UtcNow);
}
