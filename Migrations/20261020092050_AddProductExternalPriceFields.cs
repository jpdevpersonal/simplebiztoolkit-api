using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace simplebiztoolkit_api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductExternalPriceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuyerPrice",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EtsyListingId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrency",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PriceLastSyncedUtc",
                table: "Products",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "EtsyListingId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PriceLastSyncedUtc",
                table: "Products");
        }
    }
}
