using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CruzamentoSimilar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produto_similar",
                columns: table => new
                {
                    id_similar = table.Column<Guid>(type: "uuid", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    marca = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    cod_referencia = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    id_produto_equivalente = table.Column<Guid>(type: "uuid", nullable: true),
                    observacao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto_similar", x => x.id_similar);
                    table.ForeignKey(
                        name: "FK_produto_similar_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_produto_similar_produto_id_produto_equivalente",
                        column: x => x.id_produto_equivalente,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_similar_equivalente",
                table: "produto_similar",
                column: "id_produto_equivalente");

            migrationBuilder.CreateIndex(
                name: "ix_produto_similar_produto",
                table: "produto_similar",
                column: "id_produto");

            migrationBuilder.CreateIndex(
                name: "ix_produto_similar_referencia",
                table: "produto_similar",
                columns: new[] { "marca", "cod_referencia" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produto_similar");
        }
    }
}
