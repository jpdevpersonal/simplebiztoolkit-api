using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace simplebiztoolkit_api.Migrations
{
    public partial class SeedInitialData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var basePath = ResolveBasePath();

            var postsPath = Path.Combine(basePath, "InitialData", "posts.ts");
            var productsPath = Path.Combine(basePath, "InitialData", "products.ts");
            var featuredPath = Path.Combine(basePath, "InitialData", "featured.ts");

            var posts = LoadPosts(postsPath);
            var categories = LoadCategories(productsPath);
            var featured = LoadFeatured(featuredPath);

            if (posts.Count > 0)
            {
                migrationBuilder.Sql(BuildArticlesSql(posts));
            }

            if (categories.Count > 0)
            {
                migrationBuilder.Sql(BuildCategoriesSql(categories));
                migrationBuilder.Sql(BuildProductsSql(categories));
            }

            if (featured.Count > 0)
            {
                migrationBuilder.Sql(BuildFeaturedSql(featured));
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'[FeaturedProducts]', N'U') IS NOT NULL DELETE FROM [FeaturedProducts];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL DELETE FROM [Products];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[Categories]', N'U') IS NOT NULL DELETE FROM [Categories];");
            migrationBuilder.Sql("IF OBJECT_ID(N'[Articles]', N'U') IS NOT NULL DELETE FROM [Articles];");
        }

        private static string ResolveBasePath()
        {
            var current = Directory.GetCurrentDirectory();
            if (Directory.Exists(Path.Combine(current, "InitialData")))
            {
                return current;
            }

            var baseDir = AppContext.BaseDirectory;
            if (Directory.Exists(Path.Combine(baseDir, "InitialData")))
            {
                return baseDir;
            }

            var parent = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
            if (Directory.Exists(Path.Combine(parent, "InitialData")))
            {
                return parent;
            }

            return current;
        }

        private static List<PostSeed> LoadPosts(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<PostSeed>();
            }

            var json = ExtractJsonArray(filePath);
            return JsonSerializer.Deserialize<List<PostSeed>>(json, JsonOptions) ?? new List<PostSeed>();
        }

        private static List<CategorySeed> LoadCategories(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<CategorySeed>();
            }

            var json = ExtractJsonArray(filePath);
            return JsonSerializer.Deserialize<List<CategorySeed>>(json, JsonOptions) ?? new List<CategorySeed>();
        }

        private static List<FeaturedSeed> LoadFeatured(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new List<FeaturedSeed>();
            }

            var json = ExtractJsonArray(filePath);
            return JsonSerializer.Deserialize<List<FeaturedSeed>>(json, JsonOptions) ?? new List<FeaturedSeed>();
        }

        private static string BuildArticlesSql(IEnumerable<PostSeed> posts)
        {
            var rows = posts
                .Where(post => !string.IsNullOrWhiteSpace(post.Slug))
                .Select(post =>
                {
                    var id = CreateDeterministicGuid($"article:{post.Slug}");
                    var date = ParseDate(post.DateISO);
                    var badgesJson = JsonSerializer.Serialize(post.Badges ?? new List<string>());
                    return $"({SqlGuid(id)}, {SqlLiteral(post.Slug)}, {SqlLiteral(post.Title)}, {SqlLiteral(post.Subtitle)}, {SqlLiteral(post.Description)}, {SqlDate(date)}, {SqlDate(date)}, {SqlLiteral(post.Category)}, {post.ReadingMinutes}, {SqlLiteral(badgesJson)}, {SqlLiteral(post.FeaturedImage)}, {SqlLiteral(post.HeaderImage)}, {SqlLiteral("published")})";
                })
                .ToList();

            if (rows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("IF OBJECT_ID(N'[Articles]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Articles])");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [Articles] ([Id], [Slug], [Title], [Subtitle], [Description], [DateISO], [DateModified], [Category], [ReadingMinutes], [Badges], [FeaturedImage], [HeaderImage], [Status])");
            sb.AppendLine("    VALUES");
            sb.AppendLine("    " + string.Join(",\n    ", rows) + ";");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private static string BuildCategoriesSql(IEnumerable<CategorySeed> categories)
        {
            var rows = categories
                .Where(category => !string.IsNullOrWhiteSpace(category.Slug))
                .Select(category =>
                {
                    var id = CreateDeterministicGuid($"category:{category.Slug}");
                    return $"({SqlGuid(id)}, {SqlLiteral(category.Slug)}, {SqlLiteral(category.Name)}, {SqlLiteral(category.Summary)}, {SqlLiteral(category.HowThisHelps)}, {SqlLiteral(category.HeroImage)})";
                })
                .ToList();

            if (rows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("IF OBJECT_ID(N'[Categories]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Categories])");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [Categories] ([Id], [Slug], [Name], [Summary], [HowThisHelps], [HeroImage])");
            sb.AppendLine("    VALUES");
            sb.AppendLine("    " + string.Join(",\n    ", rows) + ";");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private static string BuildProductsSql(IEnumerable<CategorySeed> categories)
        {
            var rows = new List<string>();

            foreach (var category in categories.Where(category => !string.IsNullOrWhiteSpace(category.Slug)))
            {
                var categoryId = CreateDeterministicGuid($"category:{category.Slug}");
                foreach (var item in category.Items ?? new List<ProductSeed>())
                {
                    var title = item.Title ?? string.Empty;
                    var slugFromUrl = item.ProductPageUrl?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    var slug = slugFromUrl ?? item.Slug ?? Slugify(title);
                    var id = CreateDeterministicGuid($"product:{category.Slug}:{slug}");
                    var bulletsJson = JsonSerializer.Serialize(item.Bullets ?? new List<string>());

                    rows.Add($"({SqlGuid(id)}, {SqlLiteral(title)}, {SqlLiteral(slug)}, {SqlLiteral(item.Problem)}, {SqlLiteral(item.Description)}, {SqlLiteral(bulletsJson)}, {SqlLiteral(item.Image)}, {SqlLiteral(item.EtsyUrl)}, {SqlLiteral(item.Price)}, {SqlLiteral(item.ProductPageUrl)}, {SqlGuid(categoryId)}, {SqlLiteral("published")})");
                }
            }

            if (rows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("IF OBJECT_ID(N'[Products]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [Products])");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [Products] ([Id], [Title], [Slug], [Problem], [Description], [Bullets], [Image], [EtsyUrl], [Price], [ProductPageUrl], [CategoryId], [Status])");
            sb.AppendLine("    VALUES");
            sb.AppendLine("    " + string.Join(",\n    ", rows) + ";");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private static string BuildFeaturedSql(IEnumerable<FeaturedSeed> featured)
        {
            var rows = featured
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .Select(item =>
                {
                    var idSource = item.ProductPageUrl ?? item.Title;
                    var id = CreateDeterministicGuid($"featured:{idSource}");
                    var bulletsJson = JsonSerializer.Serialize(item.Bullets ?? new List<string>());
                    return $"({SqlGuid(id)}, {SqlLiteral(item.Title)}, {SqlLiteral(item.Problem)}, {SqlLiteral(bulletsJson)}, {SqlLiteral(item.Image)}, {SqlLiteral(item.EtsyUrl)}, {SqlLiteral(item.Price)}, {SqlLiteral(item.ProductPageUrl)})";
                })
                .ToList();

            if (rows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.AppendLine("IF OBJECT_ID(N'[FeaturedProducts]', N'U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM [FeaturedProducts])");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    INSERT INTO [FeaturedProducts] ([Id], [Title], [Problem], [Bullets], [Image], [EtsyUrl], [Price], [ProductPageUrl])");
            sb.AppendLine("    VALUES");
            sb.AppendLine("    " + string.Join(",\n    ", rows) + ";");
            sb.AppendLine("END");
            return sb.ToString();
        }

        private static string ExtractJsonArray(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var markerIndex = content.IndexOf("[", StringComparison.Ordinal);
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
            array = Regex.Replace(array, @"(?<=[:\[,]\s*)'((?:\\'|[^'])*)'", match =>
            {
                var value = match.Groups[1].Value.Replace("\\'", "'");
                return $"\"{value.Replace("\"", "\\\"")}\"";
            });
            array = Regex.Replace(array, @",(\s*[}\]])", "$1");
            return array;
        }

        private static Guid CreateDeterministicGuid(string value)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
            var guidBytes = new byte[16];
            Array.Copy(bytes, guidBytes, 16);
            return new Guid(guidBytes);
        }

        private static string Slugify(string value)
        {
            return value.Trim().ToLowerInvariant().Replace(' ', '-');
        }

        private static DateTime ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var date))
            {
                return DateTime.SpecifyKind(date, DateTimeKind.Utc);
            }

            return DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        }

        private static string SqlLiteral(string? value)
        {
            return value == null ? "NULL" : $"N'{value.Replace("'", "''")}'";
        }

        private static string SqlGuid(Guid value)
        {
            return $"'{value}'";
        }

        private static string SqlDate(DateTime value)
        {
            return $"'{value:yyyy-MM-ddTHH:mm:ss}'";
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class PostSeed
        {
            public string Slug { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public string? Subtitle { get; set; }
            public string? Description { get; set; }
            public string? DateISO { get; set; }
            public string? Category { get; set; }
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
}
