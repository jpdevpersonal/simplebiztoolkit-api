using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using simplebiztoolkit_api.Models;
using System.Text.Json;

namespace simplebiztoolkit_api.Data;

public class SimpleBizDbContext : DbContext
{
    public SimpleBizDbContext(DbContextOptions<SimpleBizDbContext> options) : base(options)
    {
    }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<ProductCategory> Categories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<FeaturedProduct> FeaturedProducts => Set<FeaturedProduct>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
            dateTime => DateOnly.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));

        var listConverter = new ValueConverter<List<string>, string>(
            list => JsonSerializer.Serialize(list ?? new List<string>(), (JsonSerializerOptions?)null),
            json => string.IsNullOrWhiteSpace(json)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>());

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(article => article.Id);
            entity.HasIndex(article => article.Slug).IsUnique();
            entity.HasIndex(article => article.Status);
            entity.Property(article => article.DateISO).HasConversion(dateOnlyConverter);
            entity.Property(article => article.DateModified).HasConversion(dateOnlyConverter);
            entity.Property(article => article.Badges).HasConversion(listConverter);
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(category => category.Id);
            entity.HasIndex(category => category.Slug).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Id);
            entity.HasIndex(product => product.Slug);
            entity.HasIndex(product => product.CategoryId);
            entity.HasIndex(product => new { product.CategoryId, product.Slug }).IsUnique();
            entity.Property(product => product.Bullets).HasConversion(listConverter);
            entity.HasOne(product => product.Category)
                .WithMany(category => category.Items)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeaturedProduct>(entity =>
        {
            entity.HasKey(featured => featured.Id);
            entity.Property(featured => featured.Bullets).HasConversion(listConverter);
        });

        // Note: seeding via migrations was removed here to avoid referencing a missing EfTsSeedLoader.
        // The project already includes a runtime seeder (SeedDataService) that will populate data on startup.
    }
}
