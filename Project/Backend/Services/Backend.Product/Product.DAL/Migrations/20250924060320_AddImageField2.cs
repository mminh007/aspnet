using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Product.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddImageField2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
               name: "ProductImage",
               table: "Products");
        }
    }
}
