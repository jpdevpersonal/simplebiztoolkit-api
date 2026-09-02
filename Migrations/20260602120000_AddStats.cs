using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using simplebiztoolkit_api.Data;

#nullable disable

namespace simplebiztoolkit_api.Migrations
{
    [DbContext(typeof(SimpleBizDbContext))]
    [Migration("20260602120000_AddStats")]
    public partial class AddStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[Stats]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[Stats]
                    (
                        [id] int IDENTITY(1,1) NOT NULL,
                        [name] varchar(50) NOT NULL,
                        [value] nvarchar(10) NOT NULL,
                        [hidden] bit NULL,
                        CONSTRAINT [PK_Stats] PRIMARY KEY ([id])
                    );

                    CREATE UNIQUE INDEX [IX_Stats_name] ON [dbo].[Stats] ([name]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stats",
                schema: "dbo");
        }
    }
}
