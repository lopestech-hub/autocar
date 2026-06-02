using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroDevolucao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devolucao",
                columns: table => new
                {
                    id_devolucao = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_devolucao = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_pre_venda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    vlr_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucao", x => x.id_devolucao);
                    table.ForeignKey(
                        name: "FK_devolucao_pre_venda_id_pre_venda",
                        column: x => x.id_pre_venda,
                        principalTable: "pre_venda",
                        principalColumn: "id_pre_venda",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucao_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "devolucao_item",
                columns: table => new
                {
                    id_devolucao_item = table.Column<Guid>(type: "uuid", nullable: false),
                    id_devolucao = table.Column<Guid>(type: "uuid", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao_produto = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    qtd = table.Column<int>(type: "integer", nullable: false),
                    vlr_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_total_item = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucao_item", x => x.id_devolucao_item);
                    table.ForeignKey(
                        name: "FK_devolucao_item_devolucao_id_devolucao",
                        column: x => x.id_devolucao,
                        principalTable: "devolucao",
                        principalColumn: "id_devolucao",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_devolucao_item_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devolucao_cod",
                table: "devolucao",
                column: "cod_devolucao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devolucao_id_usuario",
                table: "devolucao",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_devolucao_pre_venda",
                table: "devolucao",
                column: "id_pre_venda");

            migrationBuilder.CreateIndex(
                name: "ix_devolucao_item_devolucao",
                table: "devolucao_item",
                column: "id_devolucao");

            migrationBuilder.CreateIndex(
                name: "IX_devolucao_item_id_produto",
                table: "devolucao_item",
                column: "id_produto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devolucao_item");

            migrationBuilder.DropTable(
                name: "devolucao");
        }
    }
}
