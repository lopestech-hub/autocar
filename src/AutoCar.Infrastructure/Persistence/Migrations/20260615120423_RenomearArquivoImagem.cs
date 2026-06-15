using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenomearArquivoImagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A coluna foi criada FORA do EF como "url_imagem" (varchar 300) com caminhos absolutos
            // já corrompidos (o "\a" de "E:\autocar" virou o caractere de controle BEL ASCII 7).
            // Em vez de AddColumn (gerado pelo scaffold), RENOMEAR preserva os dados e, em seguida,
            // LIMPAR para guardar só o nome do arquivo (ex: "27022.jpg") — a pasta-base é configurável
            // por terminal. Defensivo: só renomeia se "url_imagem" existir e "arquivo_imagem" não.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'produto' AND column_name = 'url_imagem'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'produto' AND column_name = 'arquivo_imagem'
                    ) THEN
                        ALTER TABLE produto RENAME COLUMN url_imagem TO arquivo_imagem;
                    END IF;
                END $$;
            ");

            // Garante a coluna mesmo em bancos onde "url_imagem" nunca existiu (idempotente).
            migrationBuilder.Sql(
                "ALTER TABLE produto ADD COLUMN IF NOT EXISTS arquivo_imagem character varying(300);");

            // Limpa os valores legados: mantém só o nome do arquivo (tudo após a última barra \ ou /).
            // Descarta o prefixo de caminho — e com ele o caractere BEL corrompido. Idempotente:
            // rodar de novo num valor já limpo (sem barra) o deixa inalterado.
            migrationBuilder.Sql(@"
                UPDATE produto
                SET arquivo_imagem = regexp_replace(arquivo_imagem, '^.*[\\/]', '')
                WHERE arquivo_imagem IS NOT NULL AND arquivo_imagem <> '';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverte só o nome da coluna (a limpeza dos dados não é reversível — o caminho original
            // estava corrompido e não vale restaurar). Defensivo com IF EXISTS.
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'produto' AND column_name = 'arquivo_imagem'
                    ) THEN
                        ALTER TABLE produto RENAME COLUMN arquivo_imagem TO url_imagem;
                    END IF;
                END $$;
            ");
        }
    }
}
