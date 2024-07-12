using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitz.Migrations
{
    /// <inheritdoc />
    public partial class update_renames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "renames",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "start_date",
                table: "renames");
        }
    }
}