using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitUserDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateTable(
            //    name: "Users",
            //    columns: table => new
            //    {
            //        UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
            //        Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        PhoneNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
            //        CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Users", x => x.UserId);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_Email",
            //    table: "Users",
            //    column: "Email",
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Users_PhoneNumber",
            //    table: "Users",
            //    column: "PhoneNumber",
            //    unique: true,
            //    filter: "[PhoneNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
