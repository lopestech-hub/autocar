using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="PreVenda"/> (documento + itens).</summary>
public interface IPreVendaRepository
{
    /// <summary>Obtém a pré-venda com cliente e itens carregados.</summary>
    Task<PreVenda?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lista pré-vendas ativas (com cliente), opcionalmente filtrando por nome do cliente
    /// (cadastrado ou avulso) ou número do documento.</summary>
    Task<IReadOnlyList<PreVenda>> ListarAsync(string? filtro, CancellationToken ct = default);

    /// <summary>Persiste uma nova pré-venda (com seus itens). Contexto próprio por operação.</summary>
    Task AdicionarAsync(PreVenda preVenda, CancellationToken ct = default);

    /// <summary>Carrega a pré-venda rastreada, aplica a alteração e salva — num único contexto.
    /// Os itens novos são forçados a Added (a PK gerada no cliente faria o EF inferir Modified).
    /// Lança se a pré-venda não existir.</summary>
    Task AtualizarAsync(Guid id, Action<PreVenda> alterar, CancellationToken ct = default);
}
