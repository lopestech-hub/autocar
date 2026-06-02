using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compra",
                columns: table => new
                {
                    id_compra = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_compra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_fornecedor = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    num_documento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    observacao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    vlr_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compra", x => x.id_compra);
                    table.ForeignKey(
                        name: "FK_compra_fornecedor_id_fornecedor",
                        column: x => x.id_fornecedor,
                        principalTable: "fornecedor",
                        principalColumn: "id_fornecedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_compra_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "compra_item",
                columns: table => new
                {
                    id_compra_item = table.Column<Guid>(type: "uuid", nullable: false),
                    id_compra = table.Column<Guid>(type: "uuid", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao_produto = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    qtd = table.Column<int>(type: "integer", nullable: false),
                    vlr_custo_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_total_item = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_compra_item", x => x.id_compra_item);
                    table.ForeignKey(
                        name: "FK_compra_item_compra_id_compra",
                        column: x => x.id_compra,
                        principalTable: "compra",
                        principalColumn: "id_compra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_compra_item_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_compra_cod",
                table: "compra",
                column: "cod_compra",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_compra_fornecedor",
                table: "compra",
                column: "id_fornecedor");

            migrationBuilder.CreateIndex(
                name: "IX_compra_id_usuario",
                table: "compra",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_compra_item_compra",
                table: "compra_item",
                column: "id_compra");

            migrationBuilder.CreateIndex(
                name: "IX_compra_item_id_produto",
                table: "compra_item",
                column: "id_produto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compra_item");

            migrationBuilder.DropTable(
                name: "compra");
        }
    }
}
