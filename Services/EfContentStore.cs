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

    public IEnumerable<Article> GetArticles(string? status, bool includeAll)
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

        return query.OrderByDescending(article => article.DateISO).ToList();
    }

    public Article? GetArticleById(Guid id)
        => _db.Articles.AsNoTracking().FirstOrDefault(article => article.Id == id);

    public Article? GetArticleBySlug(string slug)
        => _db.Articles.AsNoTracking().FirstOrDefault(article => article.Slug == slug);

    public Article AddArticle(CreateArticleDto dto)
    {
        if (_db.Articles.Any(article => article.Slug == dto.Slug))
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
        _db.SaveChanges();
        return article;
    }

    public Article? UpdateArticle(Guid id, CreateArticleDto dto)
    {
        var existing = _db.Articles.FirstOrDefault(article => article.Id == id);
        if (existing == null)
        {
            return null;
        }

        if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
            && _db.Articles.Any(article => article.Id != id && article.Slug == dto.Slug))
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

        _db.SaveChanges();
        return existing;
    }

    public bool DeleteArticle(Guid id)
    {
        var article = _db.Articles.FirstOrDefault(item => item.Id == id);
        if (article == null)
        {
            return false;
        }

        _db.Articles.Remove(article);
        _db.SaveChanges();
        return true;
    }

    public IEnumerable<ProductCategory> GetCategories()
        => _db.Categories.AsNoTracking().OrderBy(category => category.Name).ToList();

    public ProductCategory? GetCategoryById(Guid id)
        => _db.Categories.AsNoTracking().FirstOrDefault(category => category.Id == id);

    public ProductCategory? GetCategoryBySlug(string slug)
        => _db.Categories.AsNoTracking().FirstOrDefault(category => category.Slug == slug);

    public ProductCategory AddCategory(CreateCategoryDto dto)
    {
        if (_db.Categories.Any(category => category.Slug == dto.Slug))
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
        _db.SaveChanges();
        return category;
    }

    public ProductCategory? UpdateCategory(Guid id, CreateCategoryDto dto)
    {
        var existing = _db.Categories.FirstOrDefault(category => category.Id == id);
        if (existing == null)
        {
            return null;
        }

        if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
            && _db.Categories.Any(category => category.Id != id && category.Slug == dto.Slug))
        {
            throw new InvalidOperationException("Category slug already exists.");
        }

        existing.Slug = dto.Slug;
        existing.Name = dto.Name;
        existing.Summary = dto.Summary;
        existing.HowThisHelps = dto.HowThisHelps;
        existing.HeroImage = dto.HeroImage;

        _db.SaveChanges();
        return existing;
    }

    public IEnumerable<Product> GetProducts()
        => _db.Products.AsNoTracking().ToList();

    public Product? GetProductById(Guid id)
        => _db.Products.AsNoTracking().FirstOrDefault(product => product.Id == id);

    public Product? GetProductBySlug(string categorySlug, string productSlug)
    {
        var category = _db.Categories.AsNoTracking().FirstOrDefault(item => item.Slug == categorySlug);
        if (category == null)
        {
            return null;
        }

        return _db.Products.AsNoTracking()
            .FirstOrDefault(product => product.CategoryId == category.Id && product.Slug == productSlug);
    }

    public Product AddProduct(CreateProductDto dto)
    {
        if (!_db.Categories.Any(category => category.Id == dto.CategoryId))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (_db.Products.Any(product => product.CategoryId == dto.CategoryId && product.Slug == dto.Slug))
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
        _db.SaveChanges();
        return product;
    }

    public Product? UpdateProduct(Guid id, CreateProductDto dto)
    {
        var existing = _db.Products.FirstOrDefault(product => product.Id == id);
        if (existing == null)
        {
            return null;
        }

        if (!_db.Categories.Any(category => category.Id == dto.CategoryId))
        {
            throw new InvalidOperationException("Category not found.");
        }

        if (_db.Products.Any(product => product.Id != id && product.CategoryId == dto.CategoryId && product.Slug == dto.Slug))
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

        _db.SaveChanges();
        return existing;
    }

    public bool DeleteProduct(Guid id)
    {
        var product = _db.Products.FirstOrDefault(item => item.Id == id);
        if (product == null)
        {
            return false;
        }

        _db.Products.Remove(product);
        _db.SaveChanges();
        return true;
    }
}
