using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de DevolucaoItem para a tabela "devolucao_item" (linha da devolução). Tabela
/// filho de devolucao (1:N) — apaga junto (Cascade). Guarda snapshot da descrição e do preço unitário
/// praticado na venda. Quantidade inteira (autopeça não fraciona). FK para produto é Restrict.
/// </summary>
public class DevolucaoItemConfiguration : IEntityTypeConfiguration<DevolucaoItem>
{
    public void Configure(EntityTypeBuilder<DevolucaoItem> builder)
    {
        builder.ToTable("devolucao_item");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id_devolucao_item");

        builder.Property(i => i.IdDevolucao)
            .HasColumnName("id_devolucao")
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

        builder.Property(i => i.VlrUnitario)
            .HasColumnName("vlr_unitario")
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

        builder.HasIndex(i => i.IdDevolucao)
            .HasDatabaseName("ix_devolucao_item_devolucao");
    }
}
