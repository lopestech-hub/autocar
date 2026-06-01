using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AplicacaoMotorCombustivel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motorizacao",
                table: "produto_aplicacao",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sts_combustivel",
                table: "produto_aplicacao",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "motorizacao",
                table: "produto_aplicacao");

            migrationBuilder.DropColumn(
                name: "sts_combustivel",
                table: "produto_aplicacao");
        }
    }
}
