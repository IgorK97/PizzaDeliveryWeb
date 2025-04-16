using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzaDeliveryWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToIngredient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Ingredients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Ingredients");
        }
    }
}
