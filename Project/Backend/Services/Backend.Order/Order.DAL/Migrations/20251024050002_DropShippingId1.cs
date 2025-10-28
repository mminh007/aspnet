using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Order.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DropShippingId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[FK_Orders_Shippings_ShippingId1]', N'F') IS NOT NULL
                    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Shippings_ShippingId1];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Orders_ShippingId1' AND object_id = OBJECT_ID(N'[dbo].[Orders]'))
                    DROP INDEX [IX_Orders_ShippingId1] ON [dbo].[Orders];
                ");

                            // Xóa cột thừa
            migrationBuilder.Sql(@"
                IF COL_LENGTH('dbo.Orders', 'ShippingId1') IS NOT NULL
                    ALTER TABLE [dbo].[Orders] DROP COLUMN [ShippingId1];
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
