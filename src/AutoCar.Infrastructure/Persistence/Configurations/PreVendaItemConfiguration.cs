using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de PreVendaItem para a tabela "pre_venda_item" (linha do documento).
/// Tabela filho de pre_venda (1:N) — apaga junto com o documento (Cascade). Guarda snapshot da
/// descrição e do preço do produto. FK para produto é Restrict (não apaga produto usado em doc).
/// </summary>
public class PreVendaItemConfiguration : IEntityTypeConfiguration<PreVendaItem>
{
    public void Configure(EntityTypeBuilder<PreVendaItem> builder)
    {
        builder.ToTable("pre_venda_item");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id_pre_venda_item");

        builder.Property(i => i.IdPreVenda)
            .HasColumnName("id_pre_venda")
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
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(i => i.VlrUnitario)
            .HasColumnName("vlr_unitario")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(i => i.VlrDesconto)
            .HasColumnName("vlr_desconto")
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

        // Produto (Restrict — não apaga produto referenciado em documento), sem navegação.
        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.IdProduto)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(i => i.IdPreVenda)
            .HasDatabaseName("ix_pre_venda_item_pre_venda");
    }
}
