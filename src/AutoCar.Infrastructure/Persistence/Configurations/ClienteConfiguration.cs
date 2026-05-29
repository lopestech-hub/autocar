using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Cliente para a tabela "cliente". Endereço é owned type
/// (colunas na própria tabela). Nomes em português com prefixos obrigatórios.
/// </summary>
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("cliente");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id_cliente");

        builder.Property(c => c.CodCliente)
            .HasColumnName("cod_cliente")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.TipoPessoa)
            .HasColumnName("sts_tipo_pessoa")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Documento)
            .HasColumnName("documento")
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(c => c.RazaoSocial)
            .HasColumnName("razao_social")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.NomeFantasia)
            .HasColumnName("nome_fantasia")
            .HasMaxLength(150);

        builder.Property(c => c.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasColumnName("email")
            .HasMaxLength(160);

        builder.Property(c => c.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(500);

        builder.Property(c => c.LimiteCredito)
            .HasColumnName("vlr_limite_credito")
            .HasColumnType("decimal(10,2)")
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

        // Endereço como owned type — colunas na própria tabela cliente.
        builder.OwnsOne(c => c.Endereco, end =>
        {
            end.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8);
            end.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(150);
            end.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(10);
            end.Property(e => e.Complemento).HasColumnName("complemento").HasMaxLength(60);
            end.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(80);
            end.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(80);
            end.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
        });

        // Documento único — chave natural de cliente.
        builder.HasIndex(c => c.Documento)
            .IsUnique()
            .HasDatabaseName("ix_cliente_documento");

        builder.HasIndex(c => c.CodCliente)
            .IsUnique()
            .HasDatabaseName("ix_cliente_cod");

        // Busca frequente por nome/razão social.
        builder.HasIndex(c => c.RazaoSocial)
            .HasDatabaseName("ix_cliente_razao_social");

        // Concorrência otimista via system column xmin (mesmo padrão de Usuario).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
