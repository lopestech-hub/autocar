using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Mecânico para a tabela "mecanico". Cadastro mestre auxiliar da Ordem
/// de Serviço (id + cod_mecanico). O mecânico não é usuário do sistema. Nomes em português.
/// </summary>
public class MecanicoConfiguration : IEntityTypeConfiguration<Mecanico>
{
    public void Configure(EntityTypeBuilder<Mecanico> builder)
    {
        builder.ToTable("mecanico");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id_mecanico");

        builder.Property(m => m.CodMecanico)
            .HasColumnName("cod_mecanico")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Nome)
            .HasColumnName("nome")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(m => m.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20);

        builder.Property(m => m.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(m => m.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(m => m.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(m => m.CodMecanico)
            .IsUnique()
            .HasDatabaseName("ix_mecanico_cod");

        // Unicidade do nome (garantia no banco). Checagem case-insensitive via ILike no repositório.
        builder.HasIndex(m => m.Nome)
            .IsUnique()
            .HasDatabaseName("ix_mecanico_nome");

        // Concorrência otimista via system column xmin (cadastro sem coleção filha).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
