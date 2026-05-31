using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "produto",
                columns: table => new
                {
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_produto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cod_barras = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    descricao = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    descricao_complementar = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    cod_fabricante = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    sts_unidade = table.Column<int>(type: "integer", nullable: false),
                    vlr_custo = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_venda = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    id_categoria = table.Column<Guid>(type: "uuid", nullable: false),
                    id_marca = table.Column<Guid>(type: "uuid", nullable: true),
                    id_fornecedor = table.Column<Guid>(type: "uuid", nullable: true),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produto", x => x.id_produto);
                    table.ForeignKey(
                        name: "FK_produto_categoria_produto_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categoria_produto",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_produto_fornecedor_id_fornecedor",
                        column: x => x.id_fornecedor,
                        principalTable: "fornecedor",
                        principalColumn: "id_fornecedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_produto_marca_id_marca",
                        column: x => x.id_marca,
                        principalTable: "marca",
                        principalColumn: "id_marca",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_produto_cod",
                table: "produto",
                column: "cod_produto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produto_cod_barras",
                table: "produto",
                column: "cod_barras",
                unique: true,
                filter: "cod_barras IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_produto_descricao",
                table: "produto",
                column: "descricao");

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_categoria",
                table: "produto",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_fornecedor",
                table: "produto",
                column: "id_fornecedor");

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_marca",
                table: "produto",
                column: "id_marca");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "produto");
        }
    }
}
