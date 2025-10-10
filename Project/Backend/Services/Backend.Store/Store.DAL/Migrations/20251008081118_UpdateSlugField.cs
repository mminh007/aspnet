using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Store.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSlugField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreCategorySlug",
                table: "Stores",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreCategorySlug",
                table: "Stores",
                column: "StoreCategorySlug");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreCategorySlug",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "StoreCategorySlug",
                table: "Stores");
        }
    }
}
