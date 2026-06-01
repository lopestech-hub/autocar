using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de PreVenda para a tabela "pre_venda" (cabeçalho do documento).
/// Cliente é FK opcional (balcão avulso); veículo é texto livre. Os itens (1:N) apagam junto
/// com o documento (Cascade). Nomes em português com prefixos obrigatórios.
/// </summary>
public class PreVendaConfiguration : IEntityTypeConfiguration<PreVenda>
{
    public void Configure(EntityTypeBuilder<PreVenda> builder)
    {
        builder.ToTable("pre_venda");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id_pre_venda");

        builder.Property(p => p.CodPreVenda)
            .HasColumnName("cod_pre_venda")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Situacao)
            .HasColumnName("sts_situacao")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.IdCliente)
            .HasColumnName("id_cliente");

        builder.Property(p => p.NomeClienteAvulso)
            .HasColumnName("nome_cliente_avulso")
            .HasMaxLength(120);

        builder.Property(p => p.VeiculoMontadora)
            .HasColumnName("veiculo_montadora")
            .HasMaxLength(60);

        builder.Property(p => p.VeiculoModelo)
            .HasColumnName("veiculo_modelo")
            .HasMaxLength(60);

        builder.Property(p => p.VeiculoAno)
            .HasColumnName("veiculo_ano")
            .HasMaxLength(9);

        builder.Property(p => p.VeiculoPlaca)
            .HasColumnName("veiculo_placa")
            .HasMaxLength(8);

        builder.Property(p => p.VlrDesconto)
            .HasColumnName("vlr_desconto")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.VlrTotal)
            .HasColumnName("vlr_total")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(255);

        builder.Property(p => p.IdUsuario)
            .HasColumnName("id_usuario")
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

        // Cliente opcional; Restrict evita apagar cliente com documentos vinculados.
        builder.HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.IdCliente)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Usuário (vendedor) — Restrict, sem navegação.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(p => p.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(p => p.CodPreVenda)
            .IsUnique()
            .HasDatabaseName("ix_pre_venda_cod");

        builder.HasIndex(p => p.IdCliente)
            .HasDatabaseName("ix_pre_venda_cliente");

        builder.HasIndex(p => p.Situacao)
            .HasDatabaseName("ix_pre_venda_situacao");

        builder.HasIndex(p => p.CriadoEm)
            .HasDatabaseName("ix_pre_venda_data");

        // Itens (1:N) — apaga os itens junto com o documento pai (Cascade). A coleção é exposta
        // como somente leitura; o EF acessa o backing field _itens.
        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.IdPreVenda)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // NOTA: a PreVenda NÃO usa concorrência otimista via xmin. Com a coleção de itens (1:N)
        // editada no mesmo SaveChanges, o batch UPDATE+DELETE+INSERT do Npgsql faz o
        // "UPDATE pre_venda ... WHERE xmin = @p" afetar 0 linhas → DbUpdateConcurrencyException.
        // Mesmo padrão do Produto (ver ProdutoConfiguration). Documento provisório editado numa
        // tela por vez no MVP — concorrência crítica fica para o estoque.
    }
}
