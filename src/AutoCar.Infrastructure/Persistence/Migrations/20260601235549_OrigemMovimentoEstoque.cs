using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrigemMovimentoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cod_documento_origem",
                table: "movimento_estoque",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "id_documento_origem",
                table: "movimento_estoque",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sts_origem",
                table: "movimento_estoque",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "ix_movimento_estoque_documento_origem",
                table: "movimento_estoque",
                column: "id_documento_origem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_movimento_estoque_documento_origem",
                table: "movimento_estoque");

            migrationBuilder.DropColumn(
                name: "cod_documento_origem",
                table: "movimento_estoque");

            migrationBuilder.DropColumn(
                name: "id_documento_origem",
                table: "movimento_estoque");

            migrationBuilder.DropColumn(
                name: "sts_origem",
                table: "movimento_estoque");
        }
    }
}
