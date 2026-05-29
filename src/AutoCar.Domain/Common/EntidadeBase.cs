namespace AutoCar.Domain.Common;

/// <summary>
/// Base de toda entidade do domínio. Garante os campos obrigatórios:
/// Id (UUID, PK), CriadoEm e AtualizadoEm. Timestamps em America/Sao_Paulo.
/// </summary>
public abstract class EntidadeBase
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // Timestamps gravados em UTC (timestamptz). Exibição converte para Brasília.
    public DateTime CriadoEm { get; protected set; } = DataHora.AgoraUtc();

    public DateTime AtualizadoEm { get; protected set; } = DataHora.AgoraUtc();

    /// <summary>Marca a entidade como modificada agora (atualiza AtualizadoEm em UTC).</summary>
    protected void MarcarAtualizada() => AtualizadoEm = DataHora.AgoraUtc();
}
