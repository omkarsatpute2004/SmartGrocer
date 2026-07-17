using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartGrocer.Migrations
{
    /// <inheritdoc />
    public partial class AddShopStatusAndUserBlocked : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Shops",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Shops");
        }
    }
}
