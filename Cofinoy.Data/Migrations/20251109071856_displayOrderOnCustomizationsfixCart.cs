using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cofinoy.Data.Migrations
{
    /// <inheritdoc />
    public partial class displayOrderOnCustomizationsfixCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "CartItemCustomizations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "CartItemCustomizations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "CartItemCustomizations");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "CartItemCustomizations");
        }
    }
}
