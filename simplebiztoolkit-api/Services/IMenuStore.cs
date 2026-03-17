using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public interface IMenuStore
{
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync();
    Task<IEnumerable<MenuItem>> GetMenuItemsAndChildrenAsync();
    Task<MenuItem?> GetMenuItemByIdAsync(Guid id);
    Task<MenuItem> AddMenuItemAsync(CreateMenuItemDto dto);
    Task<MenuItem?> UpdateMenuItemAsync(Guid id, CreateMenuItemDto dto);
    Task<bool> DeleteMenuItemAsync(Guid id);

    Task<IEnumerable<MenuCategory>> GetMenuCategoriesAsync(Guid? menuItemId);
    Task<MenuCategory?> GetMenuCategoryByIdAsync(Guid id);
    Task<MenuCategory> AddMenuCategoryAsync(CreateMenuCategoryDto dto);
    Task<MenuCategory?> UpdateMenuCategoryAsync(Guid id, CreateMenuCategoryDto dto);
    Task<bool> DeleteMenuCategoryAsync(Guid id);

    Task<IEnumerable<MenuItemPage>> GetMenuItemPagesAsync(Guid? menuCategoryId, string? status);
    Task<MenuItemPage?> GetMenuItemPageByIdAsync(Guid id);
    Task<MenuItemPage?> GetMenuItemPageBySlugAsync(string slug);
    Task<MenuItemPage> AddMenuItemPageAsync(CreateMenuItemPageDto dto);
    Task<MenuItemPage?> UpdateMenuItemPageAsync(Guid id, CreateMenuItemPageDto dto);
    Task<bool> DeleteMenuItemPageAsync(Guid id);
}
