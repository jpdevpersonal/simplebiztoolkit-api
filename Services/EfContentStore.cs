using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public class EfContentStore : IContentStore
{
    private readonly SimpleBizDbContext _db;

    public EfContentStore(SimpleBizDbContext db)
    {
        _db = db;
    }

    public async Task<List<Article>> GetArticlesAsync(string? status, bool includeAll, CancellationToken ct = default)
    {
        var query = _db.Articles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(article => article.Status == status);
        }
        else if (!includeAll)
        {
            query = query.Where(article => article.Status == "published");
        }

        return await query.OrderByDescending(article => article.DateISO).ToListAsync(ct);
    }

    public Task<Article?> GetArticleByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Articles.AsNoTracking().FirstOrDefaultAsync(article => article.Id == id, ct);

    public Task<Article?> GetArticleBySlugAsync(string slug, CancellationToken ct = default)
        => _db.Articles.AsNoTracking().FirstOrDefaultAsync(article => article.Slug == slug, ct);

    public async Task<Article> AddArticleAsync(CreateArticleDto dto, CancellationToken ct = default)
    {
        if (await _db.Articles.AnyAsync(article => article.Slug == dto.Slug, ct))
        {
            throw new InvalidOperationException("Article slug already exists.");
        }

        var now = DateOnly.FromDateTime(DateTime.UtcNow);
        var article = new Article
        {
            Id = Guid.NewGuid(),
            Slug = dto.Slug,
            Title = dto.Title,
            Subtitle = dto.Subtitle,
            Description = dto.Description,
            Content = dto.Content,
            Category = dto.Category,
            ReadingMinutes = dto.ReadingMinutes,
            Badges = dto.Badges ?? [],
            FeaturedImage = dto.FeaturedImage,
            HeaderImage = dto.HeaderImage,
            Status = dto.Status,
            SeoTitle = dto.SeoTitle,
            SeoDescription = dto.SeoDescription,
            OgImage = dto.OgImage,
            CanonicalUrl = dto.CanonicalUrl,
            DateISO = now,
            DateModified = now
        };

        _db.Articles.Add(article);
        await _db.SaveChangesAsync(ct);
        return article;
    }

    public async Task<Article?> UpdateArticleAsync(Guid id, CreateArticleDto dto, CancellationToken ct = default)
    {
        var existing = await _db.Articles.FirstOrDefaultAsync(article => article.Id == id, ct);
        if (existing == null)
        {
            return null;
        }

        if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
            && await _db.Articles.AnyAsync(article => article.Id != id && article.Slug == dto.Slug, ct))
        {
            throw new InvalidOperationException("Article slug already exists.");
        }

        existing.Slug = dto.Slug;
        existing.Title = dto.Title;
        existing.Subtitle = dto.Subtitle;
        existing.Description = dto.Description;
        existing.Content = dto.Content;
        existing.Category = dto.Category;
        existing.ReadingMinutes = dto.ReadingMinutes;
        existing.Badges = dto.Badges ?? [];
        existing.FeaturedImage = dto.FeaturedImage;
        existing.HeaderImage = dto.HeaderImage;
        existing.Status = dto.Status;
        existing.SeoTitle = dto.SeoTitle;
        existing.SeoDescription = dto.SeoDescription;
        existing.OgImage = dto.OgImage;
        existing.CanonicalUrl = dto.CanonicalUrl;
        existing.DateModified = DateOnly.FromDateTime(DateTime.UtcNow);

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteArticleAsync(Guid id, CancellationToken ct = default)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (article == null)
        {
            return false;
        }

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public Task<List<ProductCategory>> GetCategoriesAsync(CancellationToken ct = default)
        => _db.Categories.AsNoTracking().OrderBy(category => category.Name).ToListAsync(ct);

    public Task<ProductCategory?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == id, ct);

    public Task<ProductCategory?> GetCategoryBySlugAsync(string slug, CancellationToken ct = default)
        => _db.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Slug == slug, ct);

    public async Task<ProductCategory> AddCategoryAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        if (await _db.Categories.AnyAsync(category => category.Slug == dto.Slug, ct))
        {
            throw new InvalidOperationException("Category slug already exists.");
        }

        var category = new ProductCategory
        {
            Id = Guid.NewGuid(),
            Slug = dto.Slug,
            Name = dto.Name,
            Summary = dto.Summary,
            HowThisHelps = dto.HowThisHelps,
            HeroImage = dto.HeroImage
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<ProductCategory?> UpdateCategoryAsync(Guid id, CreateCategoryDto dto, CancellationToken ct = default)
    {
        var existing = await _db.Categories.FirstOrDefaultAsync(category => category.Id == id, ct);
        if (existing == null)
        {
            return null;
        }

        if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
            && await _db.Categories.AnyAsync(category => category.Id != id && category.Slug == dto.Slug, ct))
        {
            throw new InvalidOperationException("Category slug already exists.");
        }

        existing.Slug = dto.Slug;
        existing.Name = dto.Name;
        existing.Summary = dto.Summary;
        existing.HowThisHelps = dto.HowThisHelps;
        existing.HeroImage = dto.HeroImage;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public Task<List<Product>> GetProductsAsync(CancellationToken ct = default)
        => _db.Products.AsNoTracking().ToListAsync(ct);

    public Task<List<Product>> GetProductsByCategoryIdAsync(Guid categoryId, bool publishedOnly, CancellationToken ct = default)
    {
        var query = _db.Products.AsNoTracking().Where(product => product.CategoryId == categoryId);
        if (publishedOnly)
        {
            query = query.Where(product => product.Status == "published");
        }
        return query.ToListAsync(ct);
    }

    public Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Products.AsNoTracking().FirstOrDefaultAsync(product => product.Id == id, ct);

    // Single-query lookup: avoids a separate round-trip to fetch the category first
    public Task<Product?> GetProductBySlugAsync(string categorySlug, string productSlug, CancellationToken ct = default)
        => _db.Products.AsNoTracking()
            .Where(product => product.Slug == productSlug
                && _db.Categories.Any(category => category.Id == product.CategoryId && category.Slug == categorySlug))
            .FirstOrDefaultAsync(ct);

    public async Task<Product> AddProductAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        if (!await _db.Categories.AnyAsync(category => category.Id == dto.CategoryId, ct))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (await _db.Products.AnyAsync(product => product.CategoryId == dto.CategoryId && product.Slug == dto.Slug, ct))
        {
            throw new InvalidOperationException("Product slug already exists in this category.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Slug = dto.Slug,
            Problem = dto.Problem,
            Description = dto.Description,
            Bullets = dto.Bullets ?? [],
            Image = dto.Image,
            EtsyUrl = dto.EtsyUrl,
            Price = dto.Price,
            ProductPageUrl = dto.ProductPageUrl,
            CategoryId = dto.CategoryId,
            Status = dto.Status
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product?> UpdateProductAsync(Guid id, CreateProductDto dto, CancellationToken ct = default)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(product => product.Id == id, ct);
        if (existing == null)
        {
            return null;
        }

        if (!await _db.Categories.AnyAsync(category => category.Id == dto.CategoryId, ct))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (await _db.Products.AnyAsync(product => product.Id != id && product.CategoryId == dto.CategoryId && product.Slug == dto.Slug, ct))
        {
            throw new InvalidOperationException("Product slug already exists in this category.");
        }

        existing.Title = dto.Title;
        existing.Slug = dto.Slug;
        existing.Problem = dto.Problem;
        existing.Description = dto.Description;
        existing.Bullets = dto.Bullets ?? [];
        existing.Image = dto.Image;
        existing.EtsyUrl = dto.EtsyUrl;
        existing.Price = dto.Price;
        existing.ProductPageUrl = dto.ProductPageUrl;
        existing.CategoryId = dto.CategoryId;
        existing.Status = dto.Status;

        await _db.SaveChangesAsync(ct);
        return existing;
    }

    public async Task<bool> DeleteProductAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (product == null)
        {
            return false;
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
