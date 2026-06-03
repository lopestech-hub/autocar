using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de OrdemServicoItem para a tabela "ordem_servico_item" (linha do documento).
/// Linha única para os dois tipos (peça/serviço), distinguidos por sts_tipo_item. As FKs id_produto e
/// id_servico são opcionais (só a do tipo da linha é preenchida); ambas Restrict. Tabela filho de
/// ordem_servico (1:N) — apaga junto com o documento (Cascade).
/// </summary>
public class OrdemServicoItemConfiguration : IEntityTypeConfiguration<OrdemServicoItem>
{
    public void Configure(EntityTypeBuilder<OrdemServicoItem> builder)
    {
        builder.ToTable("ordem_servico_item");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id_ordem_servico_item");

        builder.Property(i => i.IdOrdemServico)
            .HasColumnName("id_ordem_servico")
            .IsRequired();

        builder.Property(i => i.Tipo)
            .HasColumnName("sts_tipo_item")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.IdProduto)
            .HasColumnName("id_produto");

        builder.Property(i => i.IdServico)
            .HasColumnName("id_servico");

        builder.Property(i => i.DescricaoItem)
            .HasColumnName("descricao_item")
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(i => i.Qtd)
            .HasColumnName("qtd")
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

        // Produto (Restrict, opcional — só nas linhas de peça), sem navegação.
        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.IdProduto)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Serviço (Restrict, opcional — só nas linhas de serviço), sem navegação.
        builder.HasOne<Servico>()
            .WithMany()
            .HasForeignKey(i => i.IdServico)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(i => i.IdOrdemServico)
            .HasDatabaseName("ix_ordem_servico_item_os");
    }
}
