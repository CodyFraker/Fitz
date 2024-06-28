using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitz.Migrations
{
    /// <inheritdoc />
    public partial class add_rename_cost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cost",
                table: "renames",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cost",
                table: "renames");
        }
    }
}