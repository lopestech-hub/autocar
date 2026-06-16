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
            .Include(p => p.Grupo)
            .Include(p => p.Marca)
            .Include(p => p.Fornecedor)
            .Include(p => p.Posicao)
            .Include(p => p.Lado)
            .Include(p => p.Aplicacoes)
            .Include(p => p.Similares)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, ProdutoCodigoLeitura>> ObterCodigosPorIdsAsync(
        IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, ProdutoCodigoLeitura>();

        await using var db = await _factory.CreateDbContextAsync(ct);
        // Projeção direta (sem materializar a entidade): uma única query para todos os ids das peças.
        var leituras = await db.Produtos
            .AsNoTracking()
            .Where(p => ids.Contains(p.Id))
            .Select(p => new ProdutoCodigoLeitura(p.Id, p.CodProduto, p.CodFabricante))
            .ToListAsync(ct);

        return leituras.ToDictionary(l => l.Id);
    }

    public async Task<IReadOnlyList<Produto>> ListarAsync(string? filtro, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        var query = db.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Grupo)
            .Include(p => p.Marca)
            .Include(p => p.Posicao)
            .Include(p => p.Lado)
            .Where(p => p.FlgAtivo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var termo = filtro.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Descricao, $"%{termo}%") ||
                (p.CodBarras != null && EF.Functions.ILike(p.CodBarras, $"%{termo}%")) ||
                (p.CodFabricante != null && EF.Functions.ILike(p.CodFabricante, $"%{termo}%")) ||
                // Espelhamento: acha o produto também pela referência de uma marca equivalente.
                p.Similares.Any(s => EF.Functions.ILike(s.CodReferencia, $"%{termo}%")));
        }

        return await query.OrderBy(p => p.Descricao).ToListAsync(ct);
    }

    public async Task AdicionarAsync(Produto produto, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);
        await db.Produtos.AddAsync(produto, ct);

        // Vínculo automático: equivalências pendentes que apontam para o cod_fabricante deste novo
        // produto passam a referenciá-lo (a marca equivalente "ganhou vida" no cadastro).
        await VincularSimilaresPendentesAsync(db, produto, ct);

        await db.SaveChangesAsync(ct);
    }

    public async Task AtualizarAsync(Guid id, Action<Produto> alterar, CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Carrega rastreado (sem AsNoTracking) para o change tracker detectar as alterações do produto.
        var produto = await db.Produtos
            .Include(p => p.Aplicacoes)
            .Include(p => p.Similares)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new InvalidOperationException($"Produto {id} não encontrado para atualização.");

        // Snapshot das coleções que existiam ANTES da alteração (as rastreadas, vindas do banco).
        var aplicacoesAntigas = produto.Aplicacoes.ToList();
        var similaresAntigos = produto.Similares.ToList();

        alterar(produto); // AlterarDados + DefinirAplicacoes + DefinirSimilares (recriam as coleções)

        // O padrão "substitui tudo" recria os filhos com Id gerado no cliente (Guid.NewGuid no
        // construtor). O change tracker, ao ver Id preenchido, infere ESTADO ERRADO (Modified → UPDATE
        // numa linha inexistente → "0 rows affected"). Forçar explicitamente: remover os antigos,
        // inserir os novos como Added. Mesmo tratamento para aplicações e equivalências.
        db.RemoveRange(aplicacoesAntigas);
        foreach (var nova in produto.Aplicacoes)
            db.Entry(nova).State = EntityState.Added;

        db.RemoveRange(similaresAntigos);
        foreach (var novo in produto.Similares)
            db.Entry(novo).State = EntityState.Added;

        // O cod_fabricante pode ter mudado na edição — revincula equivalências pendentes a este produto.
        await VincularSimilaresPendentesAsync(db, produto, ct);

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
            .Include(p => p.Posicao)
            .Include(p => p.Lado)
            .Include(p => p.Aplicacoes)
            .Include(p => p.Similares)
            .Where(p => p.FlgAtivo);

        // Termo casa com descrição, código de barras, código de fabricante OU a referência de uma
        // marca equivalente (espelhamento: pedir "NAKATA 12345" acha a Cofap que aponta para ela).
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();
            query = query.Where(p =>
                EF.Functions.ILike(p.Descricao, $"%{t}%") ||
                (p.CodBarras != null && EF.Functions.ILike(p.CodBarras, $"%{t}%")) ||
                (p.CodFabricante != null && EF.Functions.ILike(p.CodFabricante, $"%{t}%")) ||
                p.Similares.Any(s => EF.Functions.ILike(s.CodReferencia, $"%{t}%")));
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

    // Vínculo automático por referência: quando um produto é salvo, as equivalências PENDENTES
    // (id_produto_equivalente IS NULL) de OUTROS produtos cujo cod_referencia casa com o
    // cod_fabricante deste produto passam a apontar para ele. Roda no mesmo contexto/transação do
    // save, garantindo atomicidade. Só age quando o produto tem cod_fabricante informado.
    private static async Task VincularSimilaresPendentesAsync(AppDbContext db, Produto produto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(produto.CodFabricante))
            return;

        // A referência é gravada normalizada (Trim + CAIXA ALTA); compara no mesmo formato.
        var referencia = produto.CodFabricante.Trim().ToUpperInvariant();

        var pendentes = await db.Set<ProdutoSimilar>()
            .Where(s => s.IdProdutoEquivalente == null
                        && s.IdProduto != produto.Id
                        && s.CodReferencia == referencia)
            .ToListAsync(ct);

        foreach (var similar in pendentes)
            similar.VincularProduto(produto.Id);
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
