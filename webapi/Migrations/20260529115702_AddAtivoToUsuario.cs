using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSgc.Migrations
{
    /// <inheritdoc />
    public partial class AddAtivoToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "usuario",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            // Migrar usuários existentes: quem tinha EmailValidado=true fica Ativo=true
            migrationBuilder.Sql("UPDATE usuario SET Ativo = EmailValidado WHERE DeletedAt IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "usuario");
        }
    }
}
