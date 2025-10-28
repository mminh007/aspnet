using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Shippings_ShippingId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Shippings_ShippingId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingId1",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shippings_ShippingId",
                table: "Orders",
                column: "ShippingId",
                principalTable: "Shippings",
                principalColumn: "ShippingId",
                onDelete: ReferentialAction.SetNull);
        }

        //    // (1) Đảm bảo cột ShippingId là nullable(an toàn nếu DB cũ vẫn NOT NULL)
        //    migrationBuilder.AlterColumn<Guid>(
        //        name: "ShippingId",
        //        table: "Orders",
        //        type: "uniqueidentifier",
        //        nullable: true,
        //        oldClrType: typeof(Guid),
        //        oldType: "uniqueidentifier",
        //        oldNullable: true // nếu tool than phiền, bạn có thể bỏ dòng oldNullable
        //    );

        //    migrationBuilder.Sql(@"
        //    IF OBJECT_ID(N'[dbo].[FK_Orders_Shippings_ShippingId]', N'F') IS NOT NULL
        //        ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Shippings_ShippingId];
        //    IF OBJECT_ID(N'[dbo].[FK_Orders_Shippings_ShippingId1]', N'F') IS NOT NULL
        //        ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Shippings_ShippingId1];
        //    ");

        //    // 2) Drop index IX_Orders_ShippingId1 (nếu có)
        //    migrationBuilder.Sql(@"
        //    IF EXISTS (SELECT 1 FROM sys.indexes 
        //               WHERE name = N'IX_Orders_ShippingId1' 
        //                 AND object_id = OBJECT_ID(N'[dbo].[Orders]'))
        //        DROP INDEX [IX_Orders_ShippingId1] ON [dbo].[Orders];
        //    ");

        //    // 3) Drop cột ShippingId1 (nếu có)
        //    migrationBuilder.Sql(@"
        //    IF COL_LENGTH('dbo.Orders', 'ShippingId1') IS NOT NULL
        //        ALTER TABLE [dbo].[Orders] DROP COLUMN [ShippingId1];
        //    ");

        //    // (3) Đảm bảo có index trên ShippingId (nếu trước đó bị drop)
        //    migrationBuilder.CreateIndex(
        //        name: "IX_Orders_ShippingId",
        //        table: "Orders",
        //        column: "ShippingId");

        //    migrationBuilder.AddForeignKey(
        //        name: "FK_Orders_Shippings_ShippingId",
        //        table: "Orders",
        //        column: "ShippingId",
        //        principalTable: "Shippings",
        //        principalColumn: "ShippingId");
        
        //}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Shippings_ShippingId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ShippingId1",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingId1",
                table: "Orders",
                column: "ShippingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shippings_ShippingId",
                table: "Orders",
                column: "ShippingId",
                principalTable: "Shippings",
                principalColumn: "ShippingId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Shippings_ShippingId1",
                table: "Orders",
                column: "ShippingId1",
                principalTable: "Shippings",
                principalColumn: "ShippingId");
        }
    }
}
