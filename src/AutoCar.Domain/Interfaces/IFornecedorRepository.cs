using AutoCar.Domain.Entities;

namespace AutoCar.Domain.Interfaces;

/// <summary>Contrato de persistência de <see cref="Fornecedor"/>.</summary>
public interface IFornecedorRepository
{
    Task<Fornecedor?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    Task<Fornecedor?> ObterPorDocumentoAsync(string documento, CancellationToken ct = default);

    /// <summary>Lista fornecedores ativos, opcionalmente filtrando por nome/razão social ou documento.</summary>
    Task<IReadOnlyList<Fornecedor>> ListarAsync(string? filtro, CancellationToken ct = default);

    Task AdicionarAsync(Fornecedor fornecedor, CancellationToken ct = default);

    void Atualizar(Fornecedor fornecedor);

    /// <summary>Verifica se já existe outro fornecedor com o documento (exceto o id informado, em edição).</summary>
    Task<bool> ExisteDocumentoAsync(string documento, Guid? excetoId = null, CancellationToken ct = default);

    Task<int> SalvarAsync(CancellationToken ct = default);
}
