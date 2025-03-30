﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzaDeliveryWeb.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class newdatecolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancellationTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationTime",
                table: "Orders");
        }
    }
}
