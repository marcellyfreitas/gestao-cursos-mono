using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiSgc.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotaMaximaFromAvaliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotaMaxima",
                table: "avaliacao");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "avaliacao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "NotaMaxima",
                table: "avaliacao",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Peso",
                table: "avaliacao",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
