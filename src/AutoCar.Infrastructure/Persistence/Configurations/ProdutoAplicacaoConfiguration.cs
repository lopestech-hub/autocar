using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de ProdutoAplicacao para a tabela "produto_aplicacao".
/// Tabela filho de produto (1:N) — apaga junto com o produto (Cascade). Texto livre no MVP.
/// </summary>
public class ProdutoAplicacaoConfiguration : IEntityTypeConfiguration<ProdutoAplicacao>
{
    public void Configure(EntityTypeBuilder<ProdutoAplicacao> builder)
    {
        builder.ToTable("produto_aplicacao");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id_aplicacao");

        builder.Property(a => a.IdProduto)
            .HasColumnName("id_produto")
            .IsRequired();

        builder.Property(a => a.Montadora)
            .HasColumnName("montadora")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(a => a.Modelo)
            .HasColumnName("modelo")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(a => a.AnoInicio)
            .HasColumnName("ano_inicio");

        builder.Property(a => a.AnoFim)
            .HasColumnName("ano_fim");

        builder.Property(a => a.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(120);

        builder.Property(a => a.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(a => a.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Busca "quais produtos servem no modelo X" — índice por montadora/modelo.
        builder.HasIndex(a => new { a.Montadora, a.Modelo })
            .HasDatabaseName("ix_produto_aplicacao_veiculo");

        // FK para produto, apagando as aplicações junto com o produto pai.
        builder.HasIndex(a => a.IdProduto)
            .HasDatabaseName("ix_produto_aplicacao_produto");
    }
}
