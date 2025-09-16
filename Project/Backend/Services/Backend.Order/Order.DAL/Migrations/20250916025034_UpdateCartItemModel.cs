using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartItemModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AddedAt",
                table: "CartItems",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "CartItems",
                newName: "AddedAt");
        }
    }
}
