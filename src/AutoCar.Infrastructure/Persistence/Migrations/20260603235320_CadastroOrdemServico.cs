using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ordem_servico",
                columns: table => new
                {
                    id_ordem_servico = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_ordem_servico = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sts_situacao = table.Column<int>(type: "integer", nullable: false),
                    id_cliente = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_cliente_avulso = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    id_usuario_mecanico = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_ordem_servico", x => x.id_ordem_servico);
                    table.ForeignKey(
                        name: "FK_ordem_servico_cliente_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "cliente",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordem_servico_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordem_servico_usuario_id_usuario_mecanico",
                        column: x => x.id_usuario_mecanico,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ordem_servico_item",
                columns: table => new
                {
                    id_ordem_servico_item = table.Column<Guid>(type: "uuid", nullable: false),
                    id_ordem_servico = table.Column<Guid>(type: "uuid", nullable: false),
                    sts_tipo_item = table.Column<int>(type: "integer", nullable: false),
                    id_produto = table.Column<Guid>(type: "uuid", nullable: true),
                    id_servico = table.Column<Guid>(type: "uuid", nullable: true),
                    descricao_item = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    qtd = table.Column<int>(type: "integer", nullable: false),
                    vlr_unitario = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_desconto = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vlr_total_item = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordem_servico_item", x => x.id_ordem_servico_item);
                    table.ForeignKey(
                        name: "FK_ordem_servico_item_ordem_servico_id_ordem_servico",
                        column: x => x.id_ordem_servico,
                        principalTable: "ordem_servico",
                        principalColumn: "id_ordem_servico",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ordem_servico_item_produto_id_produto",
                        column: x => x.id_produto,
                        principalTable: "produto",
                        principalColumn: "id_produto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ordem_servico_item_servico_id_servico",
                        column: x => x.id_servico,
                        principalTable: "servico",
                        principalColumn: "id_servico",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_cliente",
                table: "ordem_servico",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_cod",
                table: "ordem_servico",
                column: "cod_ordem_servico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_data",
                table: "ordem_servico",
                column: "dat_criacao");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_id_usuario",
                table: "ordem_servico",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_mecanico",
                table: "ordem_servico",
                column: "id_usuario_mecanico");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_situacao",
                table: "ordem_servico",
                column: "sts_situacao");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_item_id_produto",
                table: "ordem_servico_item",
                column: "id_produto");

            migrationBuilder.CreateIndex(
                name: "IX_ordem_servico_item_id_servico",
                table: "ordem_servico_item",
                column: "id_servico");

            migrationBuilder.CreateIndex(
                name: "ix_ordem_servico_item_os",
                table: "ordem_servico_item",
                column: "id_ordem_servico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordem_servico_item");

            migrationBuilder.DropTable(
                name: "ordem_servico");
        }
    }
}
