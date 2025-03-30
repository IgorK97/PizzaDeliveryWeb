using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzaDeliveryWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class imagesAreRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionTime",
                type: "datetime2",
                table: "Orders",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionTime",
                table: "Orders");
        }
    }
}
