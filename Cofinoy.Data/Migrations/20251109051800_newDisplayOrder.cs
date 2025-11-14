using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cofinoy.Data.Migrations
{
    /// <inheritdoc />
    public partial class newDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "CustomizationOptions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "CustomizationOptions");
        }
    }
}
