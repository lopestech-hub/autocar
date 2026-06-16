using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PosicaoLadoComoCadastro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sts_lado",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "sts_posicao",
                table: "produto");

            migrationBuilder.AddColumn<Guid>(
                name: "id_lado",
                table: "produto",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id_posicao",
                table: "produto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "lado_peca",
                columns: table => new
                {
                    id_lado = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_lado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lado_peca", x => x.id_lado);
                });

            migrationBuilder.CreateTable(
                name: "posicao_peca",
                columns: table => new
                {
                    id_posicao = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_posicao = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descricao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_posicao_peca", x => x.id_posicao);
                });

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_lado",
                table: "produto",
                column: "id_lado");

            migrationBuilder.CreateIndex(
                name: "IX_produto_id_posicao",
                table: "produto",
                column: "id_posicao");

            migrationBuilder.CreateIndex(
                name: "ix_lado_peca_cod",
                table: "lado_peca",
                column: "cod_lado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lado_peca_descricao",
                table: "lado_peca",
                column: "descricao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_posicao_peca_cod",
                table: "posicao_peca",
                column: "cod_posicao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_posicao_peca_descricao",
                table: "posicao_peca",
                column: "descricao",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_produto_lado_peca_id_lado",
                table: "produto",
                column: "id_lado",
                principalTable: "lado_peca",
                principalColumn: "id_lado",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_produto_posicao_peca_id_posicao",
                table: "produto",
                column: "id_posicao",
                principalTable: "posicao_peca",
                principalColumn: "id_posicao",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_produto_lado_peca_id_lado",
                table: "produto");

            migrationBuilder.DropForeignKey(
                name: "FK_produto_posicao_peca_id_posicao",
                table: "produto");

            migrationBuilder.DropTable(
                name: "lado_peca");

            migrationBuilder.DropTable(
                name: "posicao_peca");

            migrationBuilder.DropIndex(
                name: "IX_produto_id_lado",
                table: "produto");

            migrationBuilder.DropIndex(
                name: "IX_produto_id_posicao",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "id_lado",
                table: "produto");

            migrationBuilder.DropColumn(
                name: "id_posicao",
                table: "produto");

            migrationBuilder.AddColumn<int>(
                name: "sts_lado",
                table: "produto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "sts_posicao",
                table: "produto",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
