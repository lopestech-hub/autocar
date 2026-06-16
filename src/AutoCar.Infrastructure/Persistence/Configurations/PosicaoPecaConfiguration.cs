using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de PosicaoPeca para a tabela "posicao_peca". Cadastro mestre
/// auxiliar editável (id + cod_posicao). Substitui o enum fixo anterior. Nomes em
/// português com prefixos obrigatórios.
/// </summary>
public class PosicaoPecaConfiguration : IEntityTypeConfiguration<PosicaoPeca>
{
    public void Configure(EntityTypeBuilder<PosicaoPeca> builder)
    {
        builder.ToTable("posicao_peca");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id_posicao");

        builder.Property(p => p.CodPosicao)
            .HasColumnName("cod_posicao")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(p => p.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(p => p.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(p => p.CodPosicao)
            .IsUnique()
            .HasDatabaseName("ix_posicao_peca_cod");

        // Unicidade da descrição (integridade no banco). Checagem case-insensitive no repositório (ILike).
        builder.HasIndex(p => p.Descricao)
            .IsUnique()
            .HasDatabaseName("ix_posicao_peca_descricao");

        // Concorrência otimista via system column xmin (mesmo padrão de Marca).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
