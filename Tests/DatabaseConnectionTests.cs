using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using simplebiztoolkit_api.Data;
using Xunit;

namespace simplebiztoolkit_api.Tests;

public class DatabaseConnectionTests
{
    [Fact]
    public async Task CanConnectAndReadArticles()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=JPLAPTOP;Database=simplebiztoolkit;Integrated Security=SSPI;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<SimpleBizDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var context = new SimpleBizDbContext(options);
        await context.Database.MigrateAsync();

        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect);

        await context.Articles.AsNoTracking().Take(1).ToListAsync();
    }
}
