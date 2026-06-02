using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de CompraItem para a tabela "compra_item" (linha da compra). Tabela filho de
/// compra (1:N) — apaga junto (Cascade). Guarda snapshot da descrição do produto e o custo unitário
/// pago. Quantidade inteira (autopeça não fraciona). FK para produto é Restrict.
/// </summary>
public class CompraItemConfiguration : IEntityTypeConfiguration<CompraItem>
{
    public void Configure(EntityTypeBuilder<CompraItem> builder)
    {
        builder.ToTable("compra_item");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id_compra_item");

        builder.Property(i => i.IdCompra)
            .HasColumnName("id_compra")
            .IsRequired();

        builder.Property(i => i.IdProduto)
            .HasColumnName("id_produto")
            .IsRequired();

        builder.Property(i => i.DescricaoProduto)
            .HasColumnName("descricao_produto")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(i => i.Qtd)
            .HasColumnName("qtd")
            .IsRequired();

        builder.Property(i => i.VlrCustoUnitario)
            .HasColumnName("vlr_custo_unitario")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(i => i.VlrTotalItem)
            .HasColumnName("vlr_total_item")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(i => i.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(i => i.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Produto (Restrict — não apaga produto referenciado), sem navegação.
        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.IdProduto)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(i => i.IdCompra)
            .HasDatabaseName("ix_compra_item_compra");
    }
}
