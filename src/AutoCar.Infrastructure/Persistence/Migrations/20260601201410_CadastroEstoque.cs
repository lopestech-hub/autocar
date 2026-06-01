using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "estoque_produto",
                columns: table => new
                {
                    id_estoque_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    qtd_saldo = table.Column<int>(type: "integer", nullable: false),
                    qtd_reservada = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estoque_produto", x => x.id_estoque_produto);
                    table.ForeignKey(
                        name: "FK_estoque_produto_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimento_estoque",
                columns: table => new
                {
                    id_movimento_estoque = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_movimento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    sts_tipo = table.Column<int>(type: "integer", nullable: false),
                    qtd = table.Column<int>(type: "integer", nullable: false),
                    qtd_saldo_apos = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    observacao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimento_estoque", x => x.id_movimento_estoque);
                    table.ForeignKey(
                        name: "FK_movimento_estoque_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimento_estoque_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_estoque_produto_produto",
                table: "estoque_produto",
                column: "id_produto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimento_estoque_cod",
                table: "movimento_estoque",
                column: "cod_movimento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimento_estoque_data",
                table: "movimento_estoque",
                column: "dat_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_movimento_estoque_id_usuario",
                table: "movimento_estoque",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_movimento_estoque_produto",
                table: "movimento_estoque",
                column: "id_produto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "estoque_produto");

            migrationBuilder.DropTable(
                name: "movimento_estoque");
        }
    }
}
