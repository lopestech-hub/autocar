using System.Reflection;
using AutoCar.Domain.Common;
using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoCar.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core do AutoCar. Aplica todas as configurações Fluent API
/// do assembly e mantém os timestamps (CriadoEm/AtualizadoEm) coerentes.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();

    public DbSet<Marca> Marcas => Set<Marca>();

    public DbSet<CategoriaProduto> Categorias => Set<CategoriaProduto>();

    public DbSet<GrupoProduto> GruposProduto => Set<GrupoProduto>();

    public DbSet<PosicaoPeca> PosicoesPeca => Set<PosicaoPeca>();

    public DbSet<LadoPeca> LadosPeca => Set<LadoPeca>();

    public DbSet<Produto> Produtos => Set<Produto>();

    public DbSet<ProdutoAplicacao> ProdutoAplicacoes => Set<ProdutoAplicacao>();

    public DbSet<ProdutoSimilar> ProdutoSimilares => Set<ProdutoSimilar>();

    public DbSet<PreVenda> PreVendas => Set<PreVenda>();

    public DbSet<PreVendaItem> PreVendaItens => Set<PreVendaItem>();

    public DbSet<EstoqueProduto> EstoqueProdutos => Set<EstoqueProduto>();

    public DbSet<MovimentoEstoque> MovimentosEstoque => Set<MovimentoEstoque>();

    public DbSet<Devolucao> Devolucoes => Set<Devolucao>();

    public DbSet<DevolucaoItem> DevolucaoItens => Set<DevolucaoItem>();

    public DbSet<Compra> Compras => Set<Compra>();

    public DbSet<CompraItem> CompraItens => Set<CompraItem>();

    public DbSet<Servico> Servicos => Set<Servico>();

    public DbSet<Mecanico> Mecanicos => Set<Mecanico>();

    public DbSet<OrdemServico> OrdensServico => Set<OrdemServico>();

    public DbSet<OrdemServicoItem> OrdemServicoItens => Set<OrdemServicoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override int SaveChanges()
    {
        AtualizarTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Mantém AtualizadoEm sempre em UTC ao alterar entidades (exibição converte p/ Brasília).</summary>
    private void AtualizarTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<EntidadeBase>())
        {
            if (entry.State == EntityState.Modified)
                entry.Property(nameof(EntidadeBase.AtualizadoEm)).CurrentValue = DataHora.AgoraUtc();
        }
    }
}
