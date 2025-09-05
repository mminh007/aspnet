using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTokenRefreshWithSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refreshTokenModels_IdentityModels_IdentityId",
                table: "refreshTokenModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refreshTokenModels",
                table: "refreshTokenModels");

            migrationBuilder.DropIndex(
                name: "IX_refreshTokenModels_IdentityId",
                table: "refreshTokenModels");

            migrationBuilder.RenameTable(
                name: "refreshTokenModels",
                newName: "RefreshTokenModels");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "RefreshTokenModels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "RefreshTokenModels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SessionExpiry",
                table: "RefreshTokenModels",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokenModels",
                table: "RefreshTokenModels",
                column: "RefreshTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenModels_IdentityId",
                table: "RefreshTokenModels",
                column: "IdentityId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokenModels_IdentityModels_IdentityId",
                table: "RefreshTokenModels",
                column: "IdentityId",
                principalTable: "IdentityModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokenModels_IdentityModels_IdentityId",
                table: "RefreshTokenModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokenModels",
                table: "RefreshTokenModels");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokenModels_IdentityId",
                table: "RefreshTokenModels");

            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "RefreshTokenModels");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "RefreshTokenModels");

            migrationBuilder.DropColumn(
                name: "SessionExpiry",
                table: "RefreshTokenModels");

            migrationBuilder.RenameTable(
                name: "RefreshTokenModels",
                newName: "refreshTokenModels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refreshTokenModels",
                table: "refreshTokenModels",
                column: "RefreshTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_refreshTokenModels_IdentityId",
                table: "refreshTokenModels",
                column: "IdentityId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_refreshTokenModels_IdentityModels_IdentityId",
                table: "refreshTokenModels",
                column: "IdentityId",
                principalTable: "IdentityModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
