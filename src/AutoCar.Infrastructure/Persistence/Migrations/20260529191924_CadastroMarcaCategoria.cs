using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroMarcaCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria_produto",
                columns: table => new
                {
                    id_categoria = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_produto", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "marca",
                columns: table => new
                {
                    id_marca = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_marca = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marca", x => x.id_marca);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categoria_produto_cod",
                table: "categoria_produto",
                column: "cod_categoria",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categoria_produto_descricao",
                table: "categoria_produto",
                column: "descricao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_marca_cod",
                table: "marca",
                column: "cod_marca",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_marca_descricao",
                table: "marca",
                column: "descricao",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categoria_produto");

            migrationBuilder.DropTable(
                name: "marca");
        }
    }
}
