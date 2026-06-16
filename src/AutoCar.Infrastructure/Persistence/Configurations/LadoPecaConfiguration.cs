using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de LadoPeca para a tabela "lado_peca". Cadastro mestre auxiliar
/// editável (id + cod_lado). Substitui o enum fixo anterior. Nomes em português com
/// prefixos obrigatórios.
/// </summary>
public class LadoPecaConfiguration : IEntityTypeConfiguration<LadoPeca>
{
    public void Configure(EntityTypeBuilder<LadoPeca> builder)
    {
        builder.ToTable("lado_peca");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnName("id_lado");

        builder.Property(l => l.CodLado)
            .HasColumnName("cod_lado")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(l => l.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(l => l.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(l => l.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(l => l.CodLado)
            .IsUnique()
            .HasDatabaseName("ix_lado_peca_cod");

        // Unicidade da descrição (integridade no banco). Checagem case-insensitive no repositório (ILike).
        builder.HasIndex(l => l.Descricao)
            .IsUnique()
            .HasDatabaseName("ix_lado_peca_descricao");

        // Concorrência otimista via system column xmin (mesmo padrão de Marca).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
