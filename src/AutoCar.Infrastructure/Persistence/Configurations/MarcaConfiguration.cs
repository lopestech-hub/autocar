using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Marca para a tabela "marca". Cadastro mestre auxiliar
/// (id + cod_marca). Nomes em português com prefixos obrigatórios.
/// </summary>
public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> builder)
    {
        builder.ToTable("marca");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id_marca");

        builder.Property(m => m.CodMarca)
            .HasColumnName("cod_marca")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(m => m.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(m => m.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(m => m.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(m => m.CodMarca)
            .IsUnique()
            .HasDatabaseName("ix_marca_cod");

        // Índice de unicidade da descrição (garantia de integridade no banco). A checagem
        // case-insensitive ("Bosch" == "BOSCH") é feita no repositório via ILike, seguindo
        // o padrão do projeto de validar unicidade no service.
        builder.HasIndex(m => m.Descricao)
            .IsUnique()
            .HasDatabaseName("ix_marca_descricao");

        // Concorrência otimista via system column xmin (mesmo padrão de Usuario/Cliente).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
