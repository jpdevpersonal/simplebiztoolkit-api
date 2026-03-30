using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;
using Xunit;

namespace simplebiztoolkit_api.Tests;

public class MenuLayoutStoreTests
{
    [Fact]
    public async Task GetMenuLayoutSettingsAsync_ReturnsDefault_WhenMissing()
    {
        await using var db = CreateDbContext();
        var store = new EfMenuStore(db);

        var settings = await store.GetMenuLayoutSettingsAsync("primary");

        Assert.Equal("primary", settings.MenuKey);
        Assert.Empty(settings.OrderedMenuItemIds);
        Assert.True(settings.IsActive);
        Assert.Equal(1, settings.Version);
    }

    [Fact]
    public async Task UpsertMenuLayoutSettingsAsync_InsertsAndUpdates()
    {
        await using var db = CreateDbContext();
        var store = new EfMenuStore(db);

        var firstId = Guid.NewGuid().ToString();
        var secondId = "hidden-static:/products";

        var created = await store.UpsertMenuLayoutSettingsAsync(new UpsertMenuLayoutSettingsDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = [firstId],
            IsActive = true,
            Version = 1,
            UpdatedBy = "admin@site.com"
        });

        var updated = await store.UpsertMenuLayoutSettingsAsync(new UpsertMenuLayoutSettingsDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = [secondId],
            IsActive = false,
            Version = 2,
            UpdatedBy = "admin2@site.com"
        });

        Assert.Equal("primary", created.MenuKey);
        Assert.Single(created.OrderedMenuItemIds);

        Assert.Equal("primary", updated.MenuKey);
        Assert.Single(updated.OrderedMenuItemIds);
        Assert.Equal(secondId, updated.OrderedMenuItemIds[0]);
        Assert.False(updated.IsActive);
        Assert.True(updated.Version >= 2);
        Assert.Equal("admin2@site.com", updated.UpdatedBy);
    }

    private static SimpleBizDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SimpleBizDbContext>()
            .UseInMemoryDatabase($"menu-layout-tests-{Guid.NewGuid()}")
            .Options;

        return new SimpleBizDbContext(options);
    }
}
