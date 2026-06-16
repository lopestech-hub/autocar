using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GrupoProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id_grupo",
                table: "produto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "grupo_produto",
                columns: table => new
                {
                    id_grupo = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_grupo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    id_categoria = table.Column<Guid>(type: "uuid", nullable: false),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupo_produto", x => x.id_grupo);
                    table.ForeignKey(
                        name: "FK_grupo_produto_categoria_produto_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categoria_produto",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_grupo",
                table: "produto",
                column: "id_grupo");

            migrationBuilder.CreateIndex(
                name: "ix_grupo_produto_categoria_descricao",
                table: "grupo_produto",
                columns: new[] { "id_categoria", "descricao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_grupo_produto_cod",
                table: "grupo_produto",
                column: "cod_grupo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_produto_grupo_produto_id_grupo",
                table: "produto",
                column: "id_grupo",
                principalTable: "grupo_produto",
                principalColumn: "id_grupo",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_produto_grupo_produto_id_grupo",
                table: "produto");

            migrationBuilder.DropTable(
                name: "grupo_produto");

            migrationBuilder.DropIndex(
                name: "IX_produto_id_grupo",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "id_grupo",
                table: "produto");
        }
    }
}
