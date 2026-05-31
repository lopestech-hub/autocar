using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AplicacaoProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produto_aplicacao",
                columns: table => new
                {
                    id_aplicacao = table.Column<Guid>(type: "uuid", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    montadora = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    modelo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ano_inicio = table.Column<int>(type: "integer", nullable: true),
                    ano_fim = table.Column<int>(type: "integer", nullable: true),
                    observacao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_aplicacao", x => x.id_aplicacao);
                    table.ForeignKey(
                        name: "FK_produto_aplicacao_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_aplicacao_produto",
                table: "produto_aplicacao",
                column: "id_produto");

            migrationBuilder.CreateIndex(
                name: "ix_produto_aplicacao_veiculo",
                table: "produto_aplicacao",
                columns: new[] { "montadora", "modelo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produto_aplicacao");
        }
    }
}
