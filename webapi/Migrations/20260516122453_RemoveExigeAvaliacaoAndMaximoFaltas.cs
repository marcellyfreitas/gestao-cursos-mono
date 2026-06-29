using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSgc.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExigeAvaliacaoAndMaximoFaltas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExigeAvaliacao",
                table: "curso");

            migrationBuilder.DropColumn(
                name: "MaximoFaltas",
                table: "curso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExigeAvaliacao",
                table: "curso",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximoFaltas",
                table: "curso",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
