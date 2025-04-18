using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzaDeliveryWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeOrderDeliveryToOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId",
                table: "Deliveries",
                column: "OrderId");
        }
    }
}
