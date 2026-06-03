using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="OrdemServico"/> (documento + itens).</summary>
public interface IOrdemServicoRepository
{
    /// <summary>Obtém a OS com cliente e itens carregados.</summary>
    Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista OS ativas (com cliente), opcionalmente filtrando por nome do cliente
    /// (cadastrado ou avulso), placa do veículo ou número do documento.</summary>
    Task<IReadOnlyList<OrdemServico>> ListarAsync(string? filtro, CancellationToken ct = default);

    /// <summary>Persiste uma nova OS (com seus itens). Contexto próprio por operação.</summary>
    Task AdicionarAsync(OrdemServico ordemServico, CancellationToken ct = default);

    /// <summary>Carrega a OS rastreada, aplica a alteração (cabeçalho + substitui itens) e salva —
    /// num único contexto. Os itens novos são forçados a Added (a PK gerada no cliente faria o EF
    /// inferir Modified). Use para editar a OS. Lança se a OS não existir.</summary>
    Task AtualizarAsync(Guid id, Action<OrdemServico> alterar, CancellationToken ct = default);

    /// <summary>Carrega a OS rastreada, aplica uma transição de ciclo (Iniciar/Concluir/Cancelar) e
    /// salva — sem tocar na coleção de itens (só muda o cabeçalho). Lança se a OS não existir.</summary>
    Task AplicarTransicaoAsync(Guid id, Action<OrdemServico> transicao, CancellationToken ct = default);
}
