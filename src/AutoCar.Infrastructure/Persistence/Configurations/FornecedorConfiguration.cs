using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Fornecedor para a tabela "fornecedor". Endereço é owned type
/// (colunas na própria tabela). Nomes em português com prefixos obrigatórios.
/// </summary>
public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedor");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id_fornecedor");

        builder.Property(f => f.CodFornecedor)
            .HasColumnName("cod_fornecedor")
            .ValueGeneratedOnAdd();

        builder.Property(f => f.TipoPessoa)
            .HasColumnName("sts_tipo_pessoa")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(f => f.Documento)
            .HasColumnName("documento")
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(f => f.RazaoSocial)
            .HasColumnName("razao_social")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(f => f.NomeFantasia)
            .HasColumnName("nome_fantasia")
            .HasMaxLength(150);

        builder.Property(f => f.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20);

        builder.Property(f => f.Email)
            .HasColumnName("email")
            .HasMaxLength(160);

        builder.Property(f => f.InscricaoEstadual)
            .HasColumnName("inscricao_estadual")
            .HasMaxLength(20);

        builder.Property(f => f.Contato)
            .HasColumnName("contato")
            .HasMaxLength(100);

        builder.Property(f => f.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(500);

        builder.Property(f => f.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(f => f.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(f => f.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Endereço como owned type — colunas na própria tabela fornecedor.
        builder.OwnsOne(f => f.Endereco, end =>
        {
            end.Property(e => e.Cep).HasColumnName("cep").HasMaxLength(8);
            end.Property(e => e.Logradouro).HasColumnName("logradouro").HasMaxLength(150);
            end.Property(e => e.Numero).HasColumnName("numero").HasMaxLength(10);
            end.Property(e => e.Complemento).HasColumnName("complemento").HasMaxLength(60);
            end.Property(e => e.Bairro).HasColumnName("bairro").HasMaxLength(80);
            end.Property(e => e.Cidade).HasColumnName("cidade").HasMaxLength(80);
            end.Property(e => e.Uf).HasColumnName("uf").HasMaxLength(2);
        });

        // Documento único — chave natural de fornecedor.
        builder.HasIndex(f => f.Documento)
            .IsUnique()
            .HasDatabaseName("ix_fornecedor_documento");

        builder.HasIndex(f => f.CodFornecedor)
            .IsUnique()
            .HasDatabaseName("ix_fornecedor_cod");

        // Busca frequente por nome/razão social.
        builder.HasIndex(f => f.RazaoSocial)
            .HasDatabaseName("ix_fornecedor_razao_social");

        // Concorrência otimista via system column xmin (mesmo padrão de Cliente/Usuario).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
