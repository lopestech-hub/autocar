using AutoCar.Domain.Entities;
using AutoCar.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API da entidade Usuario para a tabela "usuario".
/// Nomes em português com prefixos obrigatórios (id_, cod_, dat_, sts_, flg_).
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id_usuario");

        builder.Property(u => u.CodUsuario)
            .HasColumnName("cod_usuario")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.Nome)
            .HasColumnName("nome")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(u => u.Login)
            .HasColumnName("usuario")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(u => u.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Perfil)
            .HasColumnName("sts_perfil")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(u => u.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(u => u.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Login único — chave natural de autenticação.
        builder.HasIndex(u => u.Login)
            .IsUnique()
            .HasDatabaseName("ix_usuario_login");

        builder.HasIndex(u => u.CodUsuario)
            .IsUnique()
            .HasDatabaseName("ix_usuario_cod");

        // Controle de concorrência otimista via system column xmin do PostgreSQL.
        // Mapeada como propriedade sombra (rowversion) — padrão estabelecido aqui
        // e reaproveitado no estoque (risco nº 1).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
