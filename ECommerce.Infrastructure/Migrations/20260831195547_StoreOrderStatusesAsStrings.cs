using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreOrderStatusesAsStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "SellerOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("""
                UPDATE SellerOrders
                SET Status =
                    CASE Status
                        WHEN '0' THEN 'Pending'
                        WHEN '1' THEN 'Confirmed'
                        WHEN '2' THEN 'Rejected'
                        WHEN '3' THEN 'Shipped'
                        WHEN '4' THEN 'Delivered'
                        WHEN '5' THEN 'Cancelled'
                        ELSE Status
                    END;
                """);

            migrationBuilder.Sql("""
                UPDATE Orders
                SET Status =
                    CASE Status
                        WHEN '0' THEN 'Pending'
                        WHEN '1' THEN 'Paid'
                        WHEN '2' THEN 'Confirmed'
                        WHEN '3' THEN 'PartiallyRejected'
                        WHEN '4' THEN 'Shipped'
                        WHEN '5' THEN 'Delivered'
                        WHEN '6' THEN 'Cancelled'
                        ELSE Status
                    END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE SellerOrders
                SET Status =
                    CASE Status
                        WHEN 'Pending' THEN '0'
                        WHEN 'Confirmed' THEN '1'
                        WHEN 'Rejected' THEN '2'
                        WHEN 'Shipped' THEN '3'
                        WHEN 'Delivered' THEN '4'
                        WHEN 'Cancelled' THEN '5'
                        ELSE Status
                    END;
                """);

            migrationBuilder.Sql("""
                UPDATE Orders
                SET Status =
                    CASE Status
                        WHEN 'Pending' THEN '0'
                        WHEN 'Paid' THEN '1'
                        WHEN 'Confirmed' THEN '2'
                        WHEN 'PartiallyRejected' THEN '3'
                        WHEN 'Shipped' THEN '4'
                        WHEN 'Delivered' THEN '5'
                        WHEN 'Cancelled' THEN '6'
                        ELSE Status
                    END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "SellerOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}