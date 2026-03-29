using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Controllers;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;
using simplebiztoolkit_api.Services;
using Xunit;

namespace simplebiztoolkit_api.Tests;

public class MenuLayoutControllerTests
{
    [Fact]
    public async Task PublicGet_ReturnsDataEnvelope()
    {
        var expected = new MenuLayoutSettingsDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = [],
            IsActive = true,
            Version = 1
        };

        var store = new StubMenuStore
        {
            GetMenuLayoutSettingsFunc = _ => Task.FromResult(expected)
        };

        var controller = new MenuLayoutController(store);

        var actionResult = await controller.Get();

        var ok = Assert.IsType<OkObjectResult>(actionResult);
        var data = ok.Value?.GetType().GetProperty("data")?.GetValue(ok.Value);
        var payload = Assert.IsType<MenuLayoutSettingsDto>(data);

        Assert.Equal("primary", payload.MenuKey);
        Assert.Empty(payload.OrderedMenuItemIds);
    }

    [Fact]
    public void AdminGet_HasAuthorizeAndRateLimitAttributes()
    {
        var method = typeof(MenuLayoutController).GetMethod(nameof(MenuLayoutController.GetAdmin));
        Assert.NotNull(method);

        Assert.NotNull(method!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true).SingleOrDefault());

        var rateLimit = method.GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: true)
            .OfType<EnableRateLimitingAttribute>()
            .SingleOrDefault();

        Assert.NotNull(rateLimit);
        Assert.Equal("admin", rateLimit!.PolicyName);
    }

    [Fact]
    public async Task AdminPut_InvalidPayload_ReturnsBadRequestProblem()
    {
        var controller = new MenuLayoutController(new StubMenuStore());

        var actionResult = await controller.Upsert(new UpsertMenuLayoutSettingsRequestDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = null,
            Version = 1
        });

        var badRequest = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task AdminPut_ValidPayload_UpsertsAndReturnsDataEnvelope()
    {
        var updated = new MenuLayoutSettingsDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = [
                "static:/products",
                "hidden-static:/blog",
                Guid.NewGuid().ToString()
            ],
            IsActive = true,
            Version = 2,
            UpdatedBy = "admin@site.com"
        };

        var store = new StubMenuStore
        {
            UpsertMenuLayoutSettingsFunc = _ => Task.FromResult(updated)
        };

        var controller = new MenuLayoutController(store);

        var actionResult = await controller.Upsert(new UpsertMenuLayoutSettingsRequestDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = [.. updated.OrderedMenuItemIds],
            IsActive = true,
            Version = 1,
            UpdatedBy = "admin@site.com"
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult);
        var data = ok.Value?.GetType().GetProperty("data")?.GetValue(ok.Value);
        var payload = Assert.IsType<MenuLayoutSettingsDto>(data);

        Assert.Equal("primary", payload.MenuKey);
        Assert.Equal(3, payload.OrderedMenuItemIds.Count);
        Assert.Equal("static:/products", payload.OrderedMenuItemIds[0]);
        Assert.Equal("hidden-static:/blog", payload.OrderedMenuItemIds[1]);
    }

    [Fact]
    public async Task AdminPut_InvalidNonGuidAndNonStaticId_ReturnsBadRequestProblem()
    {
        var controller = new MenuLayoutController(new StubMenuStore());

        var actionResult = await controller.Upsert(new UpsertMenuLayoutSettingsRequestDto
        {
            MenuKey = "primary",
            OrderedMenuItemIds = ["not-a-guid-and-not-static"],
            Version = 1
        });

        var badRequest = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, badRequest.StatusCode);
    }

    private sealed class StubMenuStore : IMenuStore
    {
        public Func<string, Task<MenuLayoutSettingsDto>>? GetMenuLayoutSettingsFunc { get; set; }
        public Func<UpsertMenuLayoutSettingsDto, Task<MenuLayoutSettingsDto>>? UpsertMenuLayoutSettingsFunc { get; set; }

        public Task<IEnumerable<MenuItem>> GetMenuItemsAsync() => Task.FromResult(Enumerable.Empty<MenuItem>());
        public Task<IEnumerable<MenuItem>> GetMenuItemsAndChildrenAsync() => Task.FromResult(Enumerable.Empty<MenuItem>());
        public Task<MenuItem?> GetMenuItemByIdAsync(Guid id) => Task.FromResult<MenuItem?>(null);
        public Task<MenuItem> AddMenuItemAsync(CreateMenuItemDto dto) => throw new NotImplementedException();
        public Task<MenuItem?> UpdateMenuItemAsync(Guid id, CreateMenuItemDto dto) => Task.FromResult<MenuItem?>(null);
        public Task<bool> DeleteMenuItemAsync(Guid id) => Task.FromResult(false);
        public Task<IEnumerable<MenuCategory>> GetMenuCategoriesAsync(Guid? menuItemId) => Task.FromResult(Enumerable.Empty<MenuCategory>());
        public Task<MenuCategory?> GetMenuCategoryByIdAsync(Guid id) => Task.FromResult<MenuCategory?>(null);
        public Task<MenuCategory> AddMenuCategoryAsync(CreateMenuCategoryDto dto) => throw new NotImplementedException();
        public Task<MenuCategory?> UpdateMenuCategoryAsync(Guid id, CreateMenuCategoryDto dto) => Task.FromResult<MenuCategory?>(null);
        public Task<bool> DeleteMenuCategoryAsync(Guid id) => Task.FromResult(false);
        public Task<IEnumerable<MenuItemPage>> GetMenuItemPagesAsync(Guid? menuCategoryId, string? status) => Task.FromResult(Enumerable.Empty<MenuItemPage>());
        public Task<MenuItemPage?> GetMenuItemPageByIdAsync(Guid id) => Task.FromResult<MenuItemPage?>(null);
        public Task<MenuItemPage?> GetMenuItemPageBySlugAsync(string slug) => Task.FromResult<MenuItemPage?>(null);
        public Task<MenuItemPage> AddMenuItemPageAsync(CreateMenuItemPageDto dto) => throw new NotImplementedException();
        public Task<MenuItemPage?> UpdateMenuItemPageAsync(Guid id, CreateMenuItemPageDto dto) => Task.FromResult<MenuItemPage?>(null);
        public Task<bool> DeleteMenuItemPageAsync(Guid id) => Task.FromResult(false);

        public Task<MenuLayoutSettingsDto> GetMenuLayoutSettingsAsync(string menuKey)
            => GetMenuLayoutSettingsFunc?.Invoke(menuKey)
               ?? Task.FromResult(new MenuLayoutSettingsDto
               {
                   MenuKey = menuKey,
                   OrderedMenuItemIds = [],
                   IsActive = true,
                   Version = 1
               });

        public Task<MenuLayoutSettingsDto> UpsertMenuLayoutSettingsAsync(UpsertMenuLayoutSettingsDto dto)
            => UpsertMenuLayoutSettingsFunc?.Invoke(dto)
               ?? Task.FromResult(new MenuLayoutSettingsDto
               {
                   MenuKey = dto.MenuKey,
                   OrderedMenuItemIds = [.. dto.OrderedMenuItemIds],
                   IsActive = dto.IsActive,
                   Version = dto.Version,
                   UpdatedBy = dto.UpdatedBy
               });
    }
}
