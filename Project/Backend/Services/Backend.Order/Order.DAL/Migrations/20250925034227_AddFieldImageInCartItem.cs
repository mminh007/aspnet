using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldImageInCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "CartItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "CartItems");
        }
    }
}
