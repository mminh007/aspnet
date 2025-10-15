using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateIdentityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "IdentityModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VerificationCode",
                table: "IdentityModels",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationExpiry",
                table: "IdentityModels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "IdentityModels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityModels_Email",
                table: "IdentityModels",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdentityModels_Email",
                table: "IdentityModels");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "IdentityModels");

            migrationBuilder.DropColumn(
                name: "VerificationCode",
                table: "IdentityModels");

            migrationBuilder.DropColumn(
                name: "VerificationExpiry",
                table: "IdentityModels");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "IdentityModels");
        }
    }
}
