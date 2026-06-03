using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core do repositório de Ordens de Serviço. Usa <see cref="IDbContextFactory{TContext}"/>
/// para criar um DbContext novo por operação — mesmo padrão do PreVendaRepository, pelo mesmo motivo
/// (agregado com coleção filha 1:N editada no mesmo SaveChanges).
/// </summary>
public class OrdemServicoRepository : IOrdemServicoRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public OrdemServicoRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<OrdemServico?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.OrdensServico
            .AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<IReadOnlyList<OrdemServico>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var query = db.OrdensServico
            .AsNoTracking()
            .Include(o => o.Cliente)
            .Include(o => o.Itens)
            .Where(o => o.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            // Filtra por nome do cliente avulso, razão social do cadastrado, placa do veículo ou nº do documento.
            query = query.Where(o =>
                (o.NomeClienteAvulso != null && EF.Functions.ILike(o.NomeClienteAvulso, $"%{termo}%")) ||
                (o.Cliente != null && EF.Functions.ILike(o.Cliente.RazaoSocial, $"%{termo}%")) ||
                (o.VeiculoPlaca != null && EF.Functions.ILike(o.VeiculoPlaca, $"%{termo}%")) ||
                o.CodOrdemServico.ToString() == termo);
        }

        return await query.OrderByDescending(o => o.CodOrdemServico).ToListAsync(ct);
    }

    public async Task AdicionarAsync(OrdemServico ordemServico, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        await db.OrdensServico.AddAsync(ordemServico, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Guid id, Action<OrdemServico> alterar, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega rastreado (sem AsNoTracking) para o change tracker detectar as alterações do cabeçalho.
        var os = await db.OrdensServico
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new InvalidOperationException($"Ordem de serviço {id} não encontrada para atualização.");

        // Snapshot dos itens que existiam ANTES da alteração (os rastreados, vindos do banco).
        var itensAntigos = os.Itens.ToList();

        alterar(os); // AlterarCabecalho + DefinirItens (limpa a coleção e adiciona NOVAS instâncias), ou transição de ciclo.

        // O padrão "substitui tudo" recria os itens com Id gerado no cliente (Guid.NewGuid no construtor).
        // O change tracker, ao ver Id preenchido, infere ESTADO ERRADO (Modified → UPDATE numa linha
        // inexistente → "0 rows affected"). Forçar: remover os antigos, inserir os novos como Added.
        // (Mesma lição de Produto/PreVenda — coleção filha com PK no cliente.) As transições de ciclo
        // (Iniciar/Concluir/Cancelar) não mexem na coleção: itensAntigos == os.Itens, RemoveRange/Added
        // operam sobre as mesmas instâncias rastreadas e o EF resolve sem efeito colateral.
        db.RemoveRange(itensAntigos);
        foreach (var novo in os.Itens)
            db.Entry(novo).State = EntityState.Added;

        await db.SaveChangesAsync(ct);
    }

    public async Task AplicarTransicaoAsync(Guid id, Action<OrdemServico> transicao, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega rastreado COM os itens (o domínio precisa deles: Concluir/Faturar exigem ≥1 item).
        // A transição muda só o cabeçalho (situação) — a coleção de itens não é tocada, então não há
        // o problema de "Id no cliente → Modified" aqui; o EF gera apenas o UPDATE do cabeçalho.
        var os = await db.OrdensServico
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id, ct)
            ?? throw new InvalidOperationException($"Ordem de serviço {id} não encontrada para transição.");

        transicao(os);

        await db.SaveChangesAsync(ct);
    }
}
