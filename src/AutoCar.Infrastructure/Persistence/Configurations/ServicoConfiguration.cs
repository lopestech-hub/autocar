using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Serviço para a tabela "servico". Cadastro mestre auxiliar
/// da Ordem de Serviço (id + cod_servico). Nomes em português com prefixos obrigatórios.
/// </summary>
public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("servico");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id_servico");

        builder.Property(s => s.CodServico)
            .HasColumnName("cod_servico")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(s => s.VlrPadrao)
            .HasColumnName("vlr_padrao")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(s => s.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(s => s.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(s => s.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        builder.HasIndex(s => s.CodServico)
            .IsUnique()
            .HasDatabaseName("ix_servico_cod");

        // Índice de unicidade da descrição (garantia de integridade no banco). A checagem
        // case-insensitive é feita no repositório via ILike, seguindo o padrão do projeto.
        builder.HasIndex(s => s.Descricao)
            .IsUnique()
            .HasDatabaseName("ix_servico_descricao");

        // Concorrência otimista via system column xmin (cadastro sem coleção filha, mesmo
        // padrão de Marca/Categoria).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
