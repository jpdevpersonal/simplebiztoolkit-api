using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace simplebiztoolkit_api.Migrations
{
    public partial class AddMenuLayoutSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenuLayoutSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    MenuKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderedMenuItemIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuLayoutSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuLayoutSettings_MenuKey",
                table: "MenuLayoutSettings",
                column: "MenuKey",
                unique: true);

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM MenuLayoutSettings WHERE MenuKey = 'primary')
BEGIN
    INSERT INTO MenuLayoutSettings (MenuKey, OrderedMenuItemIds, IsActive, Version, CreatedAt, UpdatedAt, UpdatedBy)
    VALUES ('primary', '[]', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME(), NULL);
END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenuLayoutSettings");
        }
    }
}
