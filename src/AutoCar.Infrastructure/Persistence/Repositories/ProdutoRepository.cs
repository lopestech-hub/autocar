using AutoCar.Domain.Entities;
using AutoCar.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core do repositório de produtos. Usa <see cref="IDbContextFactory{TContext}"/>
/// para criar um DbContext novo por operação — evita o xmin defasado de um contexto de longa
/// duração (o produto tem concorrência otimista e aplicações 1:N que mudam fora desta tela).
/// </summary>
public class ProdutoRepository : IProdutoRepository
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ProdutoRepository(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Fornecedor)
            .Include(p => p.Aplicacoes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Produto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var query = db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Where(p => p.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Descricao, $"%{termo}%") ||
                (p.CodBarras != null && EF.Functions.ILike(p.CodBarras, $"%{termo}%")) ||
                (p.CodFabricante != null && EF.Functions.ILike(p.CodFabricante, $"%{termo}%")));
        }

        return await query.OrderBy(p => p.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        await db.Produtos.AddAsync(produto, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Guid id, Action<Produto> alterar, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega rastreado (sem AsNoTracking) para o change tracker detectar as alterações do produto.
        var produto = await db.Produtos
            .Include(p => p.Aplicacoes)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException($"Produto {id} não encontrado para atualização.");

        // Snapshot das aplicações que existiam ANTES da alteração (as rastreadas, vindas do banco).
        var aplicacoesAntigas = produto.Aplicacoes.ToList();

        alterar(produto); // AlterarDados + DefinirAplicacoes (limpa a coleção e adiciona NOVAS instâncias)

        // O padrão "substitui tudo" recria as aplicações com Id gerado no cliente (Guid.NewGuid no
        // construtor). O change tracker, ao ver Id preenchido, infere ESTADO ERRADO (Modified → UPDATE
        // numa linha inexistente → "0 rows affected"). Forçar explicitamente: remover as antigas,
        // inserir as novas como Added.
        db.RemoveRange(aplicacoesAntigas);
        foreach (var nova in produto.Aplicacoes)
            db.Entry(nova).State = EntityState.Added;

        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExisteCodBarrasAsync(string codBarras, Guid? excetoId = null, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        return await db.Produtos.AnyAsync(
            p => p.CodBarras == codBarras && (excetoId == null || p.Id != excetoId), ct);
    }

    public async Task<IReadOnlyList<Produto>> BuscarPorVeiculoAsync(
        string? termo, string? montadora, string? modelo, int? ano, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var query = db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .Include(p => p.Aplicacoes)
            .Where(p => p.FlgAtivo);

        // Termo casa com descrição, código de barras ou código de fabricante.
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Descricao, $"%{t}%") ||
                (p.CodBarras != null && EF.Functions.ILike(p.CodBarras, $"%{t}%")) ||
                (p.CodFabricante != null && EF.Functions.ILike(p.CodFabricante, $"%{t}%")));
        }

        // Filtros de veículo: o produto precisa ter PELO MENOS UMA aplicação que case com todos
        // os critérios informados. Ano cai dentro da faixa (faixa aberta tratada com null).
        if (!string.IsNullOrWhiteSpace(montadora))
        {
            var m = montadora.Trim();
            query = query.Where(p => p.Aplicacoes.Any(a => EF.Functions.ILike(a.Montadora, $"%{m}%")));
        }

        if (!string.IsNullOrWhiteSpace(modelo))
        {
            var mod = modelo.Trim();
            query = query.Where(p => p.Aplicacoes.Any(a => EF.Functions.ILike(a.Modelo, $"%{mod}%")));
        }

        if (ano is int anoFiltro)
        {
            query = query.Where(p => p.Aplicacoes.Any(a =>
                (a.AnoInicio == null || a.AnoInicio <= anoFiltro) &&
                (a.AnoFim == null || a.AnoFim >= anoFiltro)));
        }

        return await query.OrderBy(p => p.Descricao).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> ListarMontadorasAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        // Só aplicações de produtos ativos (ProdutoAplicacao não tem navegação de volta — filtra por subquery).
        var idsAtivos = db.Produtos.Where(p => p.FlgAtivo).Select(p => p.Id);
        return await db.ProdutoAplicacoes
            .AsNoTracking()
            .Where(a => idsAtivos.Contains(a.IdProduto))
            .Select(a => a.Montadora)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> ListarModelosAsync(string? montadora, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var idsAtivos = db.Produtos.Where(p => p.FlgAtivo).Select(p => p.Id);
        var query = db.ProdutoAplicacoes.AsNoTracking().Where(a => idsAtivos.Contains(a.IdProduto));

        if (!string.IsNullOrWhiteSpace(montadora))
        {
            var m = montadora.Trim();
            query = query.Where(a => EF.Functions.ILike(a.Montadora, $"%{m}%"));
        }

        return await query
            .Select(a => a.Modelo)
            .Distinct()
            .OrderBy(mod => mod)
            .ToListAsync(ct);
    }
}
