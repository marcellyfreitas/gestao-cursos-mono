using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSgc.Migrations
{
    /// <inheritdoc />
    public partial class MoveMediaMinimaToTurma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaMinima",
                table: "curso");

            migrationBuilder.AddColumn<decimal>(
                name: "MediaMinima",
                table: "turma",
                type: "decimal(5,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaMinima",
                table: "turma");

            migrationBuilder.AddColumn<decimal>(
                name: "MediaMinima",
                table: "curso",
                type: "decimal(5,2)",
                nullable: true);
        }
    }
}
