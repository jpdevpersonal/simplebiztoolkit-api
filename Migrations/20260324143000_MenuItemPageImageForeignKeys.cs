using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace simplebiztoolkit_api.Migrations
{
    public partial class MenuItemPageImageForeignKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FeaturedImageId",
                table: "MenuItemPages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HeaderImageId",
                table: "MenuItemPages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE page
SET FeaturedImageId = image.Id
FROM MenuItemPages AS page
INNER JOIN Images AS image ON image.Url = page.FeaturedImage
WHERE page.FeaturedImage IS NOT NULL
  AND page.FeaturedImageId IS NULL;");

            migrationBuilder.Sql(@"
UPDATE page
SET HeaderImageId = image.Id
FROM MenuItemPages AS page
INNER JOIN Images AS image ON image.Url = page.HeaderImage
WHERE page.HeaderImage IS NOT NULL
  AND page.HeaderImageId IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemPages_FeaturedImageId",
                table: "MenuItemPages",
                column: "FeaturedImageId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItemPages_HeaderImageId",
                table: "MenuItemPages",
                column: "HeaderImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemPages_Images_FeaturedImageId",
                table: "MenuItemPages",
                column: "FeaturedImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItemPages_Images_HeaderImageId",
                table: "MenuItemPages",
                column: "HeaderImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.DropColumn(
                name: "FeaturedImage",
                table: "MenuItemPages");

            migrationBuilder.DropColumn(
                name: "HeaderImage",
                table: "MenuItemPages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturedImage",
                table: "MenuItemPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderImage",
                table: "MenuItemPages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE page
SET FeaturedImage = image.Url
FROM MenuItemPages AS page
INNER JOIN Images AS image ON image.Id = page.FeaturedImageId
WHERE page.FeaturedImageId IS NOT NULL;");

            migrationBuilder.Sql(@"
UPDATE page
SET HeaderImage = image.Url
FROM MenuItemPages AS page
INNER JOIN Images AS image ON image.Id = page.HeaderImageId
WHERE page.HeaderImageId IS NOT NULL;");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemPages_Images_FeaturedImageId",
                table: "MenuItemPages");

            migrationBuilder.DropForeignKey(
                name: "FK_MenuItemPages_Images_HeaderImageId",
                table: "MenuItemPages");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemPages_FeaturedImageId",
                table: "MenuItemPages");

            migrationBuilder.DropIndex(
                name: "IX_MenuItemPages_HeaderImageId",
                table: "MenuItemPages");

            migrationBuilder.DropColumn(
                name: "FeaturedImageId",
                table: "MenuItemPages");

            migrationBuilder.DropColumn(
                name: "HeaderImageId",
                table: "MenuItemPages");
        }
    }
}
