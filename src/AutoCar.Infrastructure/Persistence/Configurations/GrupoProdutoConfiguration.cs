using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de GrupoProduto para a tabela "grupo_produto". Cadastro mestre auxiliar
/// editável (id + cod_grupo). Pertence a uma categoria (FK obrigatória). Descrição única DENTRO
/// da categoria (índice composto). Nível Categoria → Grupo → Produto.
/// </summary>
public class GrupoProdutoConfiguration : IEntityTypeConfiguration<GrupoProduto>
{
    public void Configure(EntityTypeBuilder<GrupoProduto> builder)
    {
        builder.ToTable("grupo_produto");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasColumnName("id_grupo");

        builder.Property(g => g.CodGrupo)
            .HasColumnName("cod_grupo")
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(g => g.IdCategoria)
            .HasColumnName("id_categoria")
            .IsRequired();

        builder.Property(g => g.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(g => g.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(g => g.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Categoria obrigatória; Restrict evita apagar categoria com grupos vinculados.
        builder.HasOne(g => g.Categoria)
            .WithMany()
            .HasForeignKey(g => g.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(g => g.CodGrupo)
            .IsUnique()
            .HasDatabaseName("ix_grupo_produto_cod");

        // Descrição única DENTRO da categoria (índice composto): "TAMPA" pode existir em
        // categorias diferentes, mas não duas vezes na mesma. Checagem case-insensitive no
        // repositório (ILike), seguindo o padrão do projeto.
        builder.HasIndex(g => new { g.IdCategoria, g.Descricao })
            .IsUnique()
            .HasDatabaseName("ix_grupo_produto_categoria_descricao");

        // Concorrência otimista via system column xmin (mesmo padrão de Marca).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
