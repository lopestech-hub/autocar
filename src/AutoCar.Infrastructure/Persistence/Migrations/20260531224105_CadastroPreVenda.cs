using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroPreVenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pre_venda",
                columns: table => new
                {
                    id_pre_venda = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_pre_venda = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sts_situacao = table.Column<int>(type: "integer", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_cliente_avulso = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    veiculo_montadora = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    veiculo_modelo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    veiculo_ano = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    veiculo_placa = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    vlr_desconto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_total = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    observacao = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pre_venda", x => x.id_pre_venda);
                    table.ForeignKey(
                        name: "FK_pre_venda_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pre_venda_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pre_venda_item",
                columns: table => new
                {
                    id_pre_venda_item = table.Column<Guid>(type: "uuid", nullable: false),
                    id_pre_venda = table.Column<Guid>(type: "uuid", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: false),
                    descricao_produto = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    qtd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_desconto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_total_item = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pre_venda_item", x => x.id_pre_venda_item);
                    table.ForeignKey(
                        name: "FK_pre_venda_item_pre_venda_id_pre_venda",
                        column: x => x.id_pre_venda,
                        principalTable: "pre_venda",
                        principalColumn: "id_pre_venda",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pre_venda_item_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pre_venda_cliente",
                table: "pre_venda",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_pre_venda_cod",
                table: "pre_venda",
                column: "cod_pre_venda",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pre_venda_data",
                table: "pre_venda",
                column: "dat_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_pre_venda_id_usuario",
                table: "pre_venda",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_pre_venda_situacao",
                table: "pre_venda",
                column: "sts_situacao");

            migrationBuilder.CreateIndex(
                name: "IX_pre_venda_item_id_produto",
                table: "pre_venda_item",
                column: "id_produto");

            migrationBuilder.CreateIndex(
                name: "ix_pre_venda_item_pre_venda",
                table: "pre_venda_item",
                column: "id_pre_venda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pre_venda_item");

            migrationBuilder.DropTable(
                name: "pre_venda");
        }
    }
}
