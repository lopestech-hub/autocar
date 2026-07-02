using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de FonteDado para a tabela "fonte_dado". Cadastro mestre auxiliar
/// (id + cod_fonte), espelhando <see cref="MarcaConfiguration"/>. Nomes em português com prefixos
/// obrigatórios. A identidade da fonte é (descricao, sistema) — o mesmo catálogo pode vir por
/// métodos de extração diferentes, então a unicidade é composta.
/// </summary>
public class FonteDadoConfiguration : IEntityTypeConfiguration<FonteDado>
{
    public void Configure(EntityTypeBuilder<FonteDado> builder)
    {
        builder.ToTable("fonte_dado");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id_fonte");

        builder.Property(f => f.CodFonte)
            .HasColumnName("cod_fonte")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(f => f.Sistema)
            .HasColumnName("sistema")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(300);

        builder.Property(f => f.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(f => f.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(f => f.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(f => f.CodFonte)
            .IsUnique()
            .HasDatabaseName("ix_fonte_dado_cod");

        // Identidade da fonte = catálogo + método de extração. Índice composto garante que
        // "COFAP" via "CATALOGO EXPRESSO" e "COFAP" via "PROPRIO" coexistam sem duplicar a
        // mesma combinação. A checagem case-insensitive é feita no service (padrão do projeto).
        builder.HasIndex(f => new { f.Descricao, f.Sistema })
            .IsUnique()
            .HasDatabaseName("ix_fonte_dado_descricao_sistema");

        // Concorrência otimista via system column xmin (mesmo padrão de Marca/Usuario/Cliente).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
