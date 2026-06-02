using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Compra para a tabela "compra" (cabeçalho do documento). Aponta para o
/// fornecedor (Restrict — obrigatório, não apaga fornecedor com compra) e o usuário. Os itens (1:N)
/// apagam junto (Cascade). Sem `xmin` — coleção filha editada no mesmo SaveChanges (padrão Pré-venda).
/// A navegação para Fornecedor existe só para a listagem exibir o nome.
/// </summary>
public class CompraConfiguration : IEntityTypeConfiguration<Compra>
{
    public void Configure(EntityTypeBuilder<Compra> builder)
    {
        builder.ToTable("compra");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id_compra");

        builder.Property(c => c.CodCompra)
            .HasColumnName("cod_compra")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.IdFornecedor)
            .HasColumnName("id_fornecedor")
            .IsRequired();

        builder.Property(c => c.IdUsuario)
            .HasColumnName("id_usuario")
            .IsRequired();

        builder.Property(c => c.NumDocumento)
            .HasColumnName("num_documento")
            .HasMaxLength(40);

        builder.Property(c => c.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(255);

        builder.Property(c => c.VlrTotal)
            .HasColumnName("vlr_total")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(c => c.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(c => c.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Fornecedor (Restrict — não apaga fornecedor com compra). Navegação usada só na listagem.
        builder.HasOne(c => c.Fornecedor)
            .WithMany()
            .HasForeignKey(c => c.IdFornecedor)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Usuário que registrou a compra (Restrict), sem navegação.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(c => c.CodCompra)
            .IsUnique()
            .HasDatabaseName("ix_compra_cod");

        builder.HasIndex(c => c.IdFornecedor)
            .HasDatabaseName("ix_compra_fornecedor");

        // Itens (1:N) — apaga junto com o documento (Cascade). Coleção exposta como somente leitura;
        // o EF acessa o backing field _itens.
        builder.HasMany(c => c.Itens)
            .WithOne()
            .HasForeignKey(i => i.IdCompra)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
