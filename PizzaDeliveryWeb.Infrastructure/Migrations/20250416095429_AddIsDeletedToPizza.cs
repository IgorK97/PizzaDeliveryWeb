using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzaDeliveryWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToPizza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Pizzas",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Pizzas");
        }
    }
}
