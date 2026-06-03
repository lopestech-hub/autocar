using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de OrdemServico para a tabela "ordem_servico" (cabeçalho do documento).
/// Cliente e mecânico são FKs opcionais; veículo é texto livre. Os itens (1:N) apagam junto com o
/// documento (Cascade). Nomes em português com prefixos obrigatórios.
/// </summary>
public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("ordem_servico");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasColumnName("id_ordem_servico");

        builder.Property(o => o.CodOrdemServico)
            .HasColumnName("cod_ordem_servico")
            .ValueGeneratedOnAdd();

        builder.Property(o => o.Situacao)
            .HasColumnName("sts_situacao")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.IdCliente)
            .HasColumnName("id_cliente");

        builder.Property(o => o.NomeClienteAvulso)
            .HasColumnName("nome_cliente_avulso")
            .HasMaxLength(120);

        builder.Property(o => o.IdUsuarioMecanico)
            .HasColumnName("id_usuario_mecanico");

        builder.Property(o => o.VeiculoMontadora)
            .HasColumnName("veiculo_montadora")
            .HasMaxLength(60);

        builder.Property(o => o.VeiculoModelo)
            .HasColumnName("veiculo_modelo")
            .HasMaxLength(60);

        builder.Property(o => o.VeiculoAno)
            .HasColumnName("veiculo_ano")
            .HasMaxLength(9);

        builder.Property(o => o.VeiculoPlaca)
            .HasColumnName("veiculo_placa")
            .HasMaxLength(8);

        builder.Property(o => o.VlrDesconto)
            .HasColumnName("vlr_desconto")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.VlrTotal)
            .HasColumnName("vlr_total")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(o => o.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(255);

        builder.Property(o => o.IdUsuario)
            .HasColumnName("id_usuario")
            .IsRequired();

        builder.Property(o => o.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(o => o.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(o => o.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Cliente opcional; Restrict evita apagar cliente com documentos vinculados.
        builder.HasOne(o => o.Cliente)
            .WithMany()
            .HasForeignKey(o => o.IdCliente)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Atendente/vendedor que abriu a OS — Restrict, sem navegação.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(o => o.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Mecânico responsável (opcional) — Restrict, sem navegação. FK distinta do atendente.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(o => o.IdUsuarioMecanico)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(o => o.CodOrdemServico)
            .IsUnique()
            .HasDatabaseName("ix_ordem_servico_cod");

        builder.HasIndex(o => o.IdCliente)
            .HasDatabaseName("ix_ordem_servico_cliente");

        builder.HasIndex(o => o.Situacao)
            .HasDatabaseName("ix_ordem_servico_situacao");

        builder.HasIndex(o => o.IdUsuarioMecanico)
            .HasDatabaseName("ix_ordem_servico_mecanico");

        builder.HasIndex(o => o.CriadoEm)
            .HasDatabaseName("ix_ordem_servico_data");

        // Itens (1:N) — apaga os itens junto com o documento pai (Cascade). A coleção é exposta
        // como somente leitura; o EF acessa o backing field _itens.
        builder.HasMany(o => o.Itens)
            .WithOne()
            .HasForeignKey(i => i.IdOrdemServico)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // NOTA: a OrdemServico NÃO usa concorrência otimista via xmin — coleção de itens (1:N) editada
        // no mesmo SaveChanges faz o "UPDATE ordem_servico ... WHERE xmin = @p" afetar 0 linhas
        // (DbUpdateConcurrencyException). Mesmo padrão de Produto/PreVenda. Ver lição global EF+Npgsql.
    }
}
