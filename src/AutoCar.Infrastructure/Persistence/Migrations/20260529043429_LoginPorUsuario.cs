using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoCar.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LoginPorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuario_email",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "email",
                table: "usuario");

            migrationBuilder.AddColumn<string>(
                name: "usuario",
                table: "usuario",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_login",
                table: "usuario",
                column: "usuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_usuario_login",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "usuario",
                table: "usuario");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "usuario",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);
        }
    }
}
