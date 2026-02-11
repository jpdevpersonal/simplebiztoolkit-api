using Microsoft.EntityFrameworkCore;
using simplebiztoolkit_api.Data;
using simplebiztoolkit_api.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace simplebiztoolkit_api.Services;

public class SeedDataService
{
    private readonly SimpleBizDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _config;

    public SeedDataService(SimpleBizDbContext db, IWebHostEnvironment environment, IConfiguration config)
    {
        _db = db;
        _environment = environment;
        _config = config;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_config.GetValue("SeedData:Enabled", true))
        {
            return;
        }

        if (!await _db.Articles.AnyAsync(cancellationToken))
        {
            var postsPath = Path.Combine(_environment.ContentRootPath, "Files", "posts.ts");
            if (File.Exists(postsPath))
            {
                var postsJson = ExtractJsonArray(postsPath, "export const posts");
                var posts = JsonSerializer.Deserialize<List<PostSeed>>(postsJson) ?? [];

                foreach (var post in posts)
                {
                    _db.Articles.Add(new Article
                    {
                        Id = Guid.NewGuid(),
                        Slug = post.Slug,
                        Title = post.Title,
                        Subtitle = post.Subtitle,
                        Description = post.Description,
                        DateISO = DateOnly.Parse(post.DateISO),
                        DateModified = DateOnly.Parse(post.DateISO),
                        Category = post.Category,
                        ReadingMinutes = post.ReadingMinutes,
                        Badges = post.Badges ?? [],
                        FeaturedImage = post.FeaturedImage,
                        HeaderImage = post.HeaderImage,
                        Status = "published"
                    });
                }
            }
        }

        if (!await _db.Categories.AnyAsync(cancellationToken))
        {
            var productsPath = Path.Combine(_environment.ContentRootPath, "Files", "products.ts");
            if (File.Exists(productsPath))
            {
                var productsJson = ExtractJsonArray(productsPath, "export const categories");
                var categories = JsonSerializer.Deserialize<List<CategorySeed>>(productsJson) ?? [];

                foreach (var category in categories)
                {
                    var categoryEntity = new ProductCategory
                    {
                        Id = Guid.NewGuid(),
                        Slug = category.Slug,
                        Name = category.Name,
                        Summary = category.Summary,
                        HowThisHelps = category.HowThisHelps,
                        HeroImage = category.HeroImage
                    };

                    _db.Categories.Add(categoryEntity);

                    foreach (var item in category.Items ?? [])
                    {
                        var title = item.Title ?? string.Empty;
                        var slugFromUrl = item.ProductPageUrl?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

                        _db.Products.Add(new Product
                        {
                            Id = Guid.NewGuid(),
                            Title = title,
                            Slug = slugFromUrl ?? item.Slug ?? title.ToLowerInvariant().Replace(' ', '-'),
                            Problem = item.Problem,
                            Description = item.Description,
                            Bullets = item.Bullets ?? [],
                            Image = item.Image,
                            EtsyUrl = item.EtsyUrl,
                            Price = item.Price,
                            ProductPageUrl = item.ProductPageUrl,
                            CategoryId = categoryEntity.Id,
                            Status = "published"
                        });
                    }
                }
            }
        }

        if (!await _db.FeaturedProducts.AnyAsync(cancellationToken))
        {
            var featuredPath = Path.Combine(_environment.ContentRootPath, "Files", "featured.ts");
            if (File.Exists(featuredPath))
            {
                var featuredJson = ExtractJsonArray(featuredPath, "export const featuredProducts");
                var featured = JsonSerializer.Deserialize<List<FeaturedSeed>>(featuredJson) ?? [];

                foreach (var item in featured)
                {
                    _db.FeaturedProducts.Add(new FeaturedProduct
                    {
                        Id = Guid.NewGuid(),
                        Title = item.Title,
                        Problem = item.Problem,
                        Bullets = item.Bullets ?? [],
                        Image = item.Image,
                        EtsyUrl = item.EtsyUrl,
                        Price = item.Price,
                        ProductPageUrl = item.ProductPageUrl
                    });
                }
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static string ExtractJsonArray(string filePath, string marker)
    {
        var content = File.ReadAllText(filePath);
        var markerIndex = content.IndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return "[]";
        }

        var startIndex = content.IndexOf('[', markerIndex);
        var endIndex = content.LastIndexOf(']');
        if (startIndex < 0 || endIndex <= startIndex)
        {
            return "[]";
        }

        var array = content.Substring(startIndex, endIndex - startIndex + 1);
        array = Regex.Replace(array, @"(?<=\{|,)\s*(\w+)\s*:", "\"$1\":");
        array = Regex.Replace(array, @",(\s*[}\]])", "$1");
        return array;
    }

    private sealed class PostSeed
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DateISO { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ReadingMinutes { get; set; }
        public List<string>? Badges { get; set; }
        public string? FeaturedImage { get; set; }
        public string? HeaderImage { get; set; }
    }

    private sealed class CategorySeed
    {
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? HowThisHelps { get; set; }
        public string? HeroImage { get; set; }
        public List<ProductSeed>? Items { get; set; }
    }

    private sealed class ProductSeed
    {
        public string? Title { get; set; }
        public string? Slug { get; set; }
        public string? Problem { get; set; }
        public string? Description { get; set; }
        public List<string>? Bullets { get; set; }
        public string? Image { get; set; }
        public string? EtsyUrl { get; set; }
        public string? Price { get; set; }
        public string? ProductPageUrl { get; set; }
    }

    private sealed class FeaturedSeed
    {
        public string Title { get; set; } = string.Empty;
        public string? Problem { get; set; }
        public List<string>? Bullets { get; set; }
        public string? Image { get; set; }
        public string? EtsyUrl { get; set; }
        public string? Price { get; set; }
        public string? ProductPageUrl { get; set; }
    }
}
