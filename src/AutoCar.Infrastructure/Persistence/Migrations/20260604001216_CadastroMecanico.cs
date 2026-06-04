using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroMecanico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordem_servico_usuario_id_usuario_mecanico",
                table: "ordem_servico");

            migrationBuilder.RenameColumn(
                name: "id_usuario_mecanico",
                table: "ordem_servico",
                newName: "id_mecanico");

            migrationBuilder.CreateTable(
                name: "mecanico",
                columns: table => new
                {
                    id_mecanico = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_mecanico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mecanico", x => x.id_mecanico);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mecanico_cod",
                table: "mecanico",
                column: "cod_mecanico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mecanico_nome",
                table: "mecanico",
                column: "nome",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ordem_servico_mecanico_id_mecanico",
                table: "ordem_servico",
                column: "id_mecanico",
                principalTable: "mecanico",
                principalColumn: "id_mecanico",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ordem_servico_mecanico_id_mecanico",
                table: "ordem_servico");

            migrationBuilder.DropTable(
                name: "mecanico");

            migrationBuilder.RenameColumn(
                name: "id_mecanico",
                table: "ordem_servico",
                newName: "id_usuario_mecanico");

            migrationBuilder.AddForeignKey(
                name: "FK_ordem_servico_usuario_id_usuario_mecanico",
                table: "ordem_servico",
                column: "id_usuario_mecanico",
                principalTable: "usuario",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
