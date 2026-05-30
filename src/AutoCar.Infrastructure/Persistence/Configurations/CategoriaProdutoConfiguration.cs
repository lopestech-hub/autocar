using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de CategoriaProduto para a tabela "categoria_produto".
/// Cadastro mestre auxiliar (id + cod_categoria). Nomes em português com prefixos.
/// </summary>
public class CategoriaProdutoConfiguration : IEntityTypeConfiguration<CategoriaProduto>
{
    public void Configure(EntityTypeBuilder<CategoriaProduto> builder)
    {
        builder.ToTable("categoria_produto");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id_categoria");

        builder.Property(c => c.CodCategoria)
            .HasColumnName("cod_categoria")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(c => c.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(c => c.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(c => c.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(c => c.CodCategoria)
            .IsUnique()
            .HasDatabaseName("ix_categoria_produto_cod");

        // Índice de unicidade da descrição (garantia de integridade no banco). A checagem
        // case-insensitive é feita no repositório via ILike, seguindo o padrão do projeto.
        builder.HasIndex(c => c.Descricao)
            .IsUnique()
            .HasDatabaseName("ix_categoria_produto_descricao");

        // Concorrência otimista via system column xmin (mesmo padrão de Usuario/Cliente).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
