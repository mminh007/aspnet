using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreIdToUserDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StoreId",
                table: "Users",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_StoreId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Users");
        }
    }
}
