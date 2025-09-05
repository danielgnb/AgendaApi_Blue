using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaApi_Blue.Migrations
{
    /// <inheritdoc />
    public partial class UsuariosComContatos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdUsuario",
                table: "Contatos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Contatos_IdUsuario",
                table: "Contatos",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Contatos_Usuarios_IdUsuario",
                table: "Contatos",
                column: "IdUsuario",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contatos_Usuarios_IdUsuario",
                table: "Contatos");

            migrationBuilder.DropIndex(
                name: "IX_Contatos_IdUsuario",
                table: "Contatos");

            migrationBuilder.DropColumn(
                name: "IdUsuario",
                table: "Contatos");
        }
    }
}
