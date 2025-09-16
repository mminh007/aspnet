using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.DAL.Migrations
{
    /// <inheritdoc />
    public partial class FixSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentityModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHashing = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokenModels",
                columns: table => new
                {
                    RefreshTokenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokenModels", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_RefreshTokenModels_IdentityModels_IdentityId",
                        column: x => x.IdentityId,
                        principalTable: "IdentityModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenModels_IdentityId",
                table: "RefreshTokenModels",
                column: "IdentityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokenModels");

            migrationBuilder.DropTable(
                name: "IdentityModels");
        }
    }
}
