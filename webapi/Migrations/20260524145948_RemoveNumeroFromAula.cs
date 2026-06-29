using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSgc.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNumeroFromAula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Numero",
                table: "aula");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Numero",
                table: "aula",
                type: "int",
                nullable: true);
        }
    }
}
