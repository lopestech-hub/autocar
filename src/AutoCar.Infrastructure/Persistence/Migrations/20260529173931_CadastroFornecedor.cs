using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CadastroFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fornecedor",
                columns: table => new
                {
                    id_fornecedor = table.Column<Guid>(type: "uuid", nullable: false),
                    cod_fornecedor = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sts_tipo_pessoa = table.Column<int>(type: "integer", nullable: false),
                    documento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    razao_social = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    nome_fantasia = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    numero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    complemento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    bairro = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    inscricao_estadual = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    contato = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    flg_ativo = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    dat_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dat_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fornecedor", x => x.id_fornecedor);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_cod",
                table: "fornecedor",
                column: "cod_fornecedor",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_documento",
                table: "fornecedor",
                column: "documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fornecedor_razao_social",
                table: "fornecedor",
                column: "razao_social");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fornecedor");
        }
    }
}
