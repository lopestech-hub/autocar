using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Devolucao para a tabela "devolucao" (cabeçalho do documento). Aponta para
/// a pré-venda de origem (Restrict — a venda permanece para histórico) e o usuário. Os itens (1:N)
/// apagam junto (Cascade). Sem `xmin` — coleção filha editada no mesmo SaveChanges (padrão Pré-venda).
/// </summary>
public class DevolucaoConfiguration : IEntityTypeConfiguration<Devolucao>
{
    public void Configure(EntityTypeBuilder<Devolucao> builder)
    {
        builder.ToTable("devolucao");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id_devolucao");

        builder.Property(d => d.CodDevolucao)
            .HasColumnName("cod_devolucao")
            .ValueGeneratedOnAdd();

        builder.Property(d => d.IdPreVenda)
            .HasColumnName("id_pre_venda")
            .IsRequired();

        builder.Property(d => d.IdUsuario)
            .HasColumnName("id_usuario")
            .IsRequired();

        builder.Property(d => d.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(255);

        builder.Property(d => d.VlrTotal)
            .HasColumnName("vlr_total")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(d => d.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(d => d.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Pré-venda de origem (Restrict — não apaga venda com devolução), sem navegação.
        builder.HasOne<PreVenda>()
            .WithMany()
            .HasForeignKey(d => d.IdPreVenda)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Usuário que devolveu (Restrict), sem navegação.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(d => d.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(d => d.CodDevolucao)
            .IsUnique()
            .HasDatabaseName("ix_devolucao_cod");

        builder.HasIndex(d => d.IdPreVenda)
            .HasDatabaseName("ix_devolucao_pre_venda");

        // Itens (1:N) — apaga junto com o documento (Cascade). Coleção exposta como somente leitura;
        // o EF acessa o backing field _itens.
        builder.HasMany(d => d.Itens)
            .WithOne()
            .HasForeignKey(i => i.IdDevolucao)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(d => d.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
