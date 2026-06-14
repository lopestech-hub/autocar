using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de ProdutoSimilar para a tabela "produto_similar".
/// Tabela filho de produto (1:N) — apaga junto com o produto pai (Cascade). A referência é texto;
/// o vínculo ao produto equivalente é opcional (Restrict — apagar o equivalente não pode apagar
/// a equivalência, e não há cascata circular).
/// </summary>
public class ProdutoSimilarConfiguration : IEntityTypeConfiguration<ProdutoSimilar>
{
    public void Configure(EntityTypeBuilder<ProdutoSimilar> builder)
    {
        builder.ToTable("produto_similar");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id_similar");

        builder.Property(s => s.IdProduto)
            .HasColumnName("id_produto")
            .IsRequired();

        builder.Property(s => s.Marca)
            .HasColumnName("marca")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(s => s.CodReferencia)
            .HasColumnName("cod_referencia")
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(s => s.IdProdutoEquivalente)
            .HasColumnName("id_produto_equivalente");

        builder.Property(s => s.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(120);

        builder.Property(s => s.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(s => s.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Busca "qual produto tem esta referência equivalente?" — casa termo × marca/cod_referencia.
        builder.HasIndex(s => new { s.Marca, s.CodReferencia })
            .HasDatabaseName("ix_produto_similar_referencia");

        // FK do dono (produto pai). Índice usado no carregamento da coleção.
        builder.HasIndex(s => s.IdProduto)
            .HasDatabaseName("ix_produto_similar_produto");

        // Vínculo opcional ao produto equivalente (quando ele existe no cadastro). NÃO cascateia:
        // apagar o equivalente apenas impede a exclusão (Restrict) — a equivalência é desvinculada
        // explicitamente no app, nunca apagada por tabela. O índice acelera o vínculo automático.
        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(s => s.IdProdutoEquivalente)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(s => s.IdProdutoEquivalente)
            .HasDatabaseName("ix_produto_similar_equivalente");
    }
}
