using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroFonteDado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id_fonte",
                table: "produto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fonte_dado",
                columns: table => new
                {
                    id_fonte = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_fonte = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    sistema = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    observacao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fonte_dado", x => x.id_fonte);
                });

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_fonte",
                table: "produto",
                column: "id_fonte");

            migrationBuilder.CreateIndex(
                name: "ix_fonte_dado_cod",
                table: "fonte_dado",
                column: "cod_fonte",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fonte_dado_descricao_sistema",
                table: "fonte_dado",
                columns: new[] { "descricao", "sistema" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_produto_fonte_dado_id_fonte",
                table: "produto",
                column: "id_fonte",
                principalTable: "fonte_dado",
                principalColumn: "id_fonte",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_produto_fonte_dado_id_fonte",
                table: "produto");

            migrationBuilder.DropTable(
                name: "fonte_dado");

            migrationBuilder.DropIndex(
                name: "IX_produto_id_fonte",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "id_fonte",
                table: "produto");
        }
    }
}
