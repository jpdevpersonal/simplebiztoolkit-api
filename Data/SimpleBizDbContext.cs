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
    public DbSet<ImageAsset> Images => Set<ImageAsset>();
    public DbSet<FeaturedProduct> FeaturedProducts => Set<FeaturedProduct>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItemPage> MenuItemPages => Set<MenuItemPage>();
    public DbSet<MenuLayoutSettings> MenuLayoutSettings => Set<MenuLayoutSettings>();

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

        modelBuilder.Entity<ImageAsset>(entity =>
        {
            entity.HasKey(image => image.Id);
            entity.Property(image => image.Url).IsRequired();
            entity.Property(image => image.BlobName).IsRequired();
            entity.HasIndex(image => image.BlobName).IsUnique();
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Title).IsRequired();
        });

        modelBuilder.Entity<MenuCategory>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.MenuItemId);
            entity.Property(c => c.Title).IsRequired();
            entity.HasOne(c => c.MenuItem)
                .WithMany(m => m.Categories)
                .HasForeignKey(c => c.MenuItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MenuItemPage>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.HasIndex(p => p.MenuCategoryId);
            entity.HasIndex(p => p.MenuItemId);
            entity.HasIndex(p => p.FeaturedImageId);
            entity.HasIndex(p => p.HeaderImageId);
            entity.Property(p => p.DateISO).HasConversion(dateOnlyConverter);
            entity.HasOne(p => p.MenuCategory)
                .WithMany(c => c.Pages)
                .HasForeignKey(p => p.MenuCategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.MenuItem)
                .WithMany()
                .HasForeignKey(p => p.MenuItemId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.FeaturedImageAsset)
                .WithMany()
                .HasForeignKey(p => p.FeaturedImageId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(p => p.HeaderImageAsset)
                .WithMany()
                .HasForeignKey(p => p.HeaderImageId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MenuLayoutSettings>(entity =>
        {
            entity.HasKey(layout => layout.Id);
            entity.Property(layout => layout.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(layout => layout.MenuKey)
                .HasMaxLength(100)
                .IsRequired();
            entity.HasIndex(layout => layout.MenuKey).IsUnique();
            entity.Property(layout => layout.OrderedMenuItemIds)
                .HasConversion(listConverter)
                .HasColumnType("nvarchar(max)")
                .IsRequired();
            entity.Property(layout => layout.IsActive)
                .HasDefaultValue(true)
                .IsRequired();
            entity.Property(layout => layout.Version)
                .HasDefaultValue(1)
                .IsRequired();
            entity.Property(layout => layout.UpdatedBy)
                .HasMaxLength(320);
        });

        // Note: seeding via migrations was removed here to avoid referencing a missing EfTsSeedLoader.
        // The project already includes a runtime seeder (SeedDataService) that will populate data on startup.
    }
}
