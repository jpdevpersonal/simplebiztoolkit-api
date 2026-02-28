using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public class EfMenuStore : IMenuStore
{
    private readonly SimpleBizDbContext _db;

    public EfMenuStore(SimpleBizDbContext db)
    {
        _db = db;
    }

    // ── MenuItem ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync()
        => await _db.MenuItems.AsNoTracking().OrderBy(m => m.Title).ToListAsync();

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAndChildrenAsync()
        => await _db.MenuItems
            .AsNoTracking()
            .Include(m => m.Categories)
                .ThenInclude(c => c.Pages)
            .AsSplitQuery()
            .OrderBy(m => m.Title)
            .ToListAsync();

    public async Task<MenuItem?> GetMenuItemByIdAsync(Guid id)
        => await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<MenuItem> AddMenuItemAsync(CreateMenuItemDto dto)
    {
        var item = new MenuItem
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status
        };

        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public async Task<MenuItem?> UpdateMenuItemAsync(Guid id, CreateMenuItemDto dto)
    {
        var existing = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
        if (existing == null)
        {
            return null;
        }

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Status = dto.Status;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteMenuItemAsync(Guid id)
    {
        var item = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == id);
        if (item == null)
        {
            return false;
        }

        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── MenuCategory ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<MenuCategory>> GetMenuCategoriesAsync(Guid? menuItemId)
    {
        var query = _db.MenuCategories.AsNoTracking();

        if (menuItemId.HasValue)
        {
            query = query.Where(c => c.MenuItemId == menuItemId.Value);
        }

        return await query.OrderBy(c => c.Title).ToListAsync();
    }

    public async Task<MenuCategory?> GetMenuCategoryByIdAsync(Guid id)
        => await _db.MenuCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public async Task<MenuCategory> AddMenuCategoryAsync(CreateMenuCategoryDto dto)
    {
        if (!await _db.MenuItems.AnyAsync(m => m.Id == dto.MenuItemId))
        {
            throw new InvalidOperationException("MenuItem not found.");
        }

        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            MenuItemId = dto.MenuItemId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status
        };

        _db.MenuCategories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<MenuCategory?> UpdateMenuCategoryAsync(Guid id, CreateMenuCategoryDto dto)
    {
        var existing = await _db.MenuCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (existing == null)
        {
            return null;
        }

        if (!await _db.MenuItems.AnyAsync(m => m.Id == dto.MenuItemId))
        {
            throw new InvalidOperationException("MenuItem not found.");
        }

        existing.MenuItemId = dto.MenuItemId;
        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Status = dto.Status;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteMenuCategoryAsync(Guid id)
    {
        var category = await _db.MenuCategories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
        {
            return false;
        }

        _db.MenuCategories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    // ── MenuItemPage ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<MenuItemPage>> GetMenuItemPagesAsync(Guid? menuCategoryId, string? status)
    {
        var query = _db.MenuItemPages.AsNoTracking();

        if (menuCategoryId.HasValue)
        {
            query = query.Where(p => p.MenuCategoryId == menuCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        return await query.OrderByDescending(p => p.DateISO).ToListAsync();
    }

    public async Task<MenuItemPage?> GetMenuItemPageByIdAsync(Guid id)
        => await _db.MenuItemPages.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<MenuItemPage?> GetMenuItemPageBySlugAsync(string slug)
        => await _db.MenuItemPages.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<MenuItemPage> AddMenuItemPageAsync(CreateMenuItemPageDto dto)
    {
        if (!await _db.MenuCategories.AnyAsync(c => c.Id == dto.MenuCategoryId))
        {
            throw new InvalidOperationException("MenuCategory not found.");
        }

        if (await _db.MenuItemPages.AnyAsync(p => p.Slug == dto.Slug))
        {
            throw new InvalidOperationException("Page slug already exists.");
        }

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var page = new MenuItemPage
        {
            Id = Guid.NewGuid(),
            MenuCategoryId = dto.MenuCategoryId,
            Slug = dto.Slug,
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            Description = dto.Description,
            Content = dto.Content,
            FeaturedImage = dto.FeaturedImage,
            HeaderImage = dto.HeaderImage,
            Status = dto.Status,
            SeoTitle = dto.SeoTitle,
            SeoDescription = dto.SeoDescription,
            OgImage = dto.OgImage,
            CanonicalUrl = dto.CanonicalUrl,
            DateISO = now,
            DateModified = System.DateTime.Now
        };

        _db.MenuItemPages.Add(page);
        await _db.SaveChangesAsync();
        return page;
    }

    public async Task<MenuItemPage?> UpdateMenuItemPageAsync(Guid id, CreateMenuItemPageDto dto)
    {
        var existing = await _db.MenuItemPages.FirstOrDefaultAsync(p => p.Id == id);
        if (existing == null)
        {
            return null;
        }

        if (!await _db.MenuCategories.AnyAsync(c => c.Id == dto.MenuCategoryId))
        {
            throw new InvalidOperationException("MenuCategory not found.");
        }

        if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
            && await _db.MenuItemPages.AnyAsync(p => p.Id != id && p.Slug == dto.Slug))
        {
            throw new InvalidOperationException("Page slug already exists.");
        }

        existing.MenuCategoryId = dto.MenuCategoryId;
        existing.Slug = dto.Slug;
        existing.Title = dto.Title;
        existing.Subtitle = dto.Subtitle;
        existing.Description = dto.Description;
        existing.Content = dto.Content;
        existing.FeaturedImage = dto.FeaturedImage;
        existing.HeaderImage = dto.HeaderImage;
        existing.Status = dto.Status;
        existing.SeoTitle = dto.SeoTitle;
        existing.SeoDescription = dto.SeoDescription;
        existing.OgImage = dto.OgImage;
        existing.CanonicalUrl = dto.CanonicalUrl;
        existing.DateModified = System.DateTime.Now;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteMenuItemPageAsync(Guid id)
    {
        var page = await _db.MenuItemPages.FirstOrDefaultAsync(p => p.Id == id);
        if (page == null)
        {
            return false;
        }

        _db.MenuItemPages.Remove(page);
        await _db.SaveChangesAsync();
        return true;
    }
}
