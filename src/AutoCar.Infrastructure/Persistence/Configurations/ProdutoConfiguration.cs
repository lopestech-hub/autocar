using AutoCar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoCar.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento Fluent API de Produto para a tabela "produto". Categoria é FK obrigatória;
/// marca e fornecedor são FKs opcionais. Código de barras é único quando informado
/// (índice único parcial — ignora nulos). Nomes em português com prefixos obrigatórios.
/// </summary>
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produto");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id_produto");

        builder.Property(p => p.CodProduto)
            .HasColumnName("cod_produto")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.CodBarras)
            .HasColumnName("cod_barras")
            .HasMaxLength(20);

        builder.Property(p => p.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(p => p.DescricaoComplementar)
            .HasColumnName("descricao_complementar")
            .HasMaxLength(160);

        builder.Property(p => p.CodFabricante)
            .HasColumnName("cod_fabricante")
            .HasMaxLength(40);

        builder.Property(p => p.Unidade)
            .HasColumnName("sts_unidade")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Posicao)
            .HasColumnName("sts_posicao")
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.PosicaoPeca.NaoAplica)
            .IsRequired();

        builder.Property(p => p.Lado)
            .HasColumnName("sts_lado")
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.LadoPeca.NaoAplica)
            .IsRequired();

        builder.Property(p => p.VlrCusto)
            .HasColumnName("vlr_custo")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.VlrVenda)
            .HasColumnName("vlr_venda")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        // Nome do arquivo de imagem (só o nome, ex: "27022.jpg"; a pasta-base é configurável por
        // terminal). Opcional. Criada inicialmente fora do EF como "url_imagem" — a migration
        // RenomearArquivoImagem renomeia para "arquivo_imagem" e limpa os caminhos legados.
        builder.Property(p => p.ArquivoImagem)
            .HasColumnName("arquivo_imagem")
            .HasMaxLength(300);

        builder.Property(p => p.IdCategoria)
            .HasColumnName("id_categoria")
            .IsRequired();

        builder.Property(p => p.IdMarca)
            .HasColumnName("id_marca");

        builder.Property(p => p.IdFornecedor)
            .HasColumnName("id_fornecedor");

        builder.Property(p => p.FlgAtivo)
            .HasColumnName("flg_ativo")
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .HasColumnName("dat_criacao")
            .IsRequired();

        builder.Property(p => p.AtualizadoEm)
            .HasColumnName("dat_atualizacao")
            .IsRequired();

        // Categoria obrigatória; Restrict evita apagar categoria com produtos vinculados.
        builder.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Marca opcional.
        builder.HasOne(p => p.Marca)
            .WithMany()
            .HasForeignKey(p => p.IdMarca)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Fornecedor opcional.
        builder.HasOne(p => p.Fornecedor)
            .WithMany()
            .HasForeignKey(p => p.IdFornecedor)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(p => p.CodProduto)
            .IsUnique()
            .HasDatabaseName("ix_produto_cod");

        // Código de barras único só quando informado: índice único parcial (ignora nulos).
        builder.HasIndex(p => p.CodBarras)
            .IsUnique()
            .HasFilter("cod_barras IS NOT NULL")
            .HasDatabaseName("ix_produto_cod_barras");

        // Busca frequente por descrição.
        builder.HasIndex(p => p.Descricao)
            .HasDatabaseName("ix_produto_descricao");

        // Aplicações por veículo (1:N) — apaga as aplicações junto com o produto pai (Cascade).
        // A coleção é exposta como somente leitura; o EF acessa o backing field _aplicacoes.
        builder.HasMany(p => p.Aplicacoes)
            .WithOne()
            .HasForeignKey(a => a.IdProduto)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Aplicacoes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Equivalências/cruzamento (1:N) — apaga junto com o produto pai (Cascade). Mesma mecânica
        // da coleção de aplicações: backing field, salva substituindo a lista inteira.
        builder.HasMany(p => p.Similares)
            .WithOne()
            .HasForeignKey(s => s.IdProduto)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Similares)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // NOTA: o Produto NÃO usa concorrência otimista via xmin (diferente de Cliente/Marca).
        // Com a coleção de aplicações (1:N) carregada via Include, o batch UPDATE+DELETE+INSERT do
        // Npgsql fazia o "UPDATE produto ... WHERE xmin = @p" afetar 0 linhas → DbUpdateConcurrency-
        // Exception, travando o save. O produto é editado numa tela única por vez no MVP; reavaliar
        // quando houver edição concorrente real (ex: Fase 3 / estoque).
    }
}
