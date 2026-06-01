using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de MovimentoEstoque para a tabela "movimento_estoque" (livro-razão).
/// Cada linha é um fato imutável (entrada/saída/ajuste); nunca é atualizado, só inserido e lido —
/// por isso NÃO tem xmin. Tem cod_ (rastreável na tela). Nomes em português com prefixos.
/// </summary>
public class MovimentoEstoqueConfiguration : IEntityTypeConfiguration<MovimentoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentoEstoque> builder)
    {
        builder.ToTable("movimento_estoque");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id_movimento_estoque");

        builder.Property(m => m.CodMovimento)
            .HasColumnName("cod_movimento")
            .ValueGeneratedOnAdd();

        builder.Property(m => m.IdProduto)
            .HasColumnName("id_produto")
            .IsRequired();

        builder.Property(m => m.Tipo)
            .HasColumnName("sts_tipo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.Qtd)
            .HasColumnName("qtd")
            .IsRequired();

        builder.Property(m => m.QtdSaldoApos)
            .HasColumnName("qtd_saldo_apos")
            .IsRequired();

        builder.Property(m => m.IdUsuario)
            .HasColumnName("id_usuario")
            .IsRequired();

        builder.Property(m => m.Observacao)
            .HasColumnName("observacao")
            .HasMaxLength(255);

        builder.Property(m => m.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        // AtualizadoEm não faz sentido num registro imutável, mas vem da EntidadeBase — mapeado
        // para não virar coluna sombra; nunca é alterado após a criação.
        builder.Property(m => m.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Produto — Restrict, sem navegação. Não se apaga produto com histórico de movimentos.
        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(m => m.IdProduto)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Usuário que movimentou — Restrict, sem navegação.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(m => m.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(m => m.CodMovimento)
            .IsUnique()
            .HasDatabaseName("ix_movimento_estoque_cod");

        builder.HasIndex(m => m.IdProduto)
            .HasDatabaseName("ix_movimento_estoque_produto");

        builder.HasIndex(m => m.CriadoEm)
            .HasDatabaseName("ix_movimento_estoque_data");
    }
}
