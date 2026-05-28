using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;

#nullable disable

namespace simplebiztoolkit_api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SimpleBizDbContext))]
    [Migration("20260601120000_AddShowLastUpdatedToMenuItemPages")]
    public partial class AddShowLastUpdatedToMenuItemPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowLastUpdated",
                table: "MenuItemPages",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowLastUpdated",
                table: "MenuItemPages");
        }
    }
}
