using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Models;
using System.Collections.Concurrent;

namespace simplebiztoolkit_api.Services;

public class InMemoryContentStore : IContentStore
{
    private readonly ConcurrentDictionary<Guid, Article> _articles = new();
    private readonly ConcurrentDictionary<Guid, ProductCategory> _categories = new();
    private readonly ConcurrentDictionary<Guid, Product> _products = new();
    private readonly object _lock = new();

    public IEnumerable<Article> GetArticles(string? status, bool includeAll)
    {
        var query = _articles.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(article => string.Equals(article.Status, status, StringComparison.OrdinalIgnoreCase));
        }
        else if (!includeAll)
        {
            query = query.Where(article => string.Equals(article.Status, "published", StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderByDescending(article => article.DateISO);
    }

    public Article? GetArticleById(Guid id) => _articles.TryGetValue(id, out var article) ? article : null;

    public Article? GetArticleBySlug(string slug)
        => _articles.Values.FirstOrDefault(article => string.Equals(article.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public Article AddArticle(CreateArticleDto dto)
    {
        lock (_lock)
        {
            if (_articles.Values.Any(article => string.Equals(article.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)))
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

            _articles[article.Id] = article;
            return article;
        }
    }

    public Article? UpdateArticle(Guid id, CreateArticleDto dto)
    {
        lock (_lock)
        {
            if (!_articles.TryGetValue(id, out var existing))
            {
                return null;
            }

            if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
                && _articles.Values.Any(article => article.Id != id && string.Equals(article.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)))
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

            return existing;
        }
    }

    public bool DeleteArticle(Guid id)
    {
        return _articles.TryRemove(id, out _);
    }

    public IEnumerable<ProductCategory> GetCategories() => _categories.Values.OrderBy(category => category.Name);

    public ProductCategory? GetCategoryById(Guid id) => _categories.TryGetValue(id, out var category) ? category : null;

    public ProductCategory? GetCategoryBySlug(string slug)
        => _categories.Values.FirstOrDefault(category => string.Equals(category.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public ProductCategory AddCategory(CreateCategoryDto dto)
    {
        lock (_lock)
        {
            if (_categories.Values.Any(category => string.Equals(category.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)))
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

            _categories[category.Id] = category;
            return category;
        }
    }

    public ProductCategory? UpdateCategory(Guid id, CreateCategoryDto dto)
    {
        lock (_lock)
        {
            if (!_categories.TryGetValue(id, out var existing))
            {
                return null;
            }

            if (!string.Equals(existing.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)
                && _categories.Values.Any(category => category.Id != id && string.Equals(category.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("Category slug already exists.");
            }

            existing.Slug = dto.Slug;
            existing.Name = dto.Name;
            existing.Summary = dto.Summary;
            existing.HowThisHelps = dto.HowThisHelps;
            existing.HeroImage = dto.HeroImage;

            return existing;
        }
    }

    public IEnumerable<Product> GetProducts() => _products.Values;

    public Product? GetProductById(Guid id) => _products.TryGetValue(id, out var product) ? product : null;

    public Product? GetProductBySlug(string categorySlug, string productSlug)
    {
        var category = GetCategoryBySlug(categorySlug);
        if (category == null)
        {
            return null;
        }

        return _products.Values.FirstOrDefault(product => product.CategoryId == category.Id
            && string.Equals(product.Slug, productSlug, StringComparison.OrdinalIgnoreCase));
    }

    public Product AddProduct(CreateProductDto dto)
    {
        lock (_lock)
        {
            if (!_categories.ContainsKey(dto.CategoryId))
            {
                throw new InvalidOperationException("Category not found.");
            }

            if (_products.Values.Any(product => product.CategoryId == dto.CategoryId
                && string.Equals(product.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase)))
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
                CategoryId = dto.CategoryId,
                Status = dto.Status
            };

            _products[product.Id] = product;
            return product;
        }
    }

    public Product? UpdateProduct(Guid id, CreateProductDto dto)
    {
        lock (_lock)
        {
            if (!_products.TryGetValue(id, out var existing))
            {
                return null;
            }

            if (!_categories.ContainsKey(dto.CategoryId))
            {
                throw new InvalidOperationException("Category not found.");
            }

            if ((_products.Values.Any(product => product.Id != id && product.CategoryId == dto.CategoryId
                && string.Equals(product.Slug, dto.Slug, StringComparison.OrdinalIgnoreCase))))
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
            existing.CategoryId = dto.CategoryId;
            existing.Status = dto.Status;

            return existing;
        }
    }

    public bool DeleteProduct(Guid id)
    {
        return _products.TryRemove(id, out _);
    }
}
