using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de EstoqueProduto para a tabela "estoque_produto" (saldo atual por produto,
/// 1:1 com produto). Usa concorrência otimista via system column xmin — este é o caso que justifica o
/// xmin no projeto (dois terminais disputando o último item). Funciona limpo aqui porque o agregado
/// NÃO tem coleção filha editada no mesmo SaveChanges (o movimento gerado é INSERT independente),
/// diferente de Produto/PreVenda. Nomes em português com prefixos obrigatórios.
/// </summary>
public class EstoqueProdutoConfiguration : IEntityTypeConfiguration<EstoqueProduto>
{
    public void Configure(EntityTypeBuilder<EstoqueProduto> builder)
    {
        builder.ToTable("estoque_produto");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id_estoque_produto");

        builder.Property(e => e.IdProduto)
            .HasColumnName("id_produto")
            .IsRequired();

        builder.Property(e => e.QtdSaldo)
            .HasColumnName("qtd_saldo")
            .IsRequired();

        builder.Property(e => e.QtdReservada)
            .HasColumnName("qtd_reservada")
            .IsRequired();

        builder.Property(e => e.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(e => e.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // QtdDisponivel é calculada (saldo − reservada) — não é coluna.
        builder.Ignore(e => e.QtdDisponivel);

        // Produto (1:1) — Restrict, sem navegação reversa. Um registro de saldo por produto.
        builder.HasOne<Produto>()
            .WithMany()
            .HasForeignKey(e => e.IdProduto)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(e => e.IdProduto)
            .IsUnique()
            .HasDatabaseName("ix_estoque_produto_produto");

        // Concorrência otimista via system column xmin (mesmo padrão de Marca/Cliente). É AQUI que
        // o xmin realmente importa: o UPDATE do saldo só afeta a linha se o xmin não mudou desde a
        // leitura — senão lança DbUpdateConcurrencyException, sinalizando que outro terminal moveu.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsRowVersion();
    }
}
