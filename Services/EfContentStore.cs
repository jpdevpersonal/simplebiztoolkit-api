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

    public bool DeleteCategory(Guid id)
    {
        var category = _db.Categories.FirstOrDefault(item => item.Id == id);
        if (category == null)
        {
            return false;
        }

        _db.Categories.Remove(category);
        _db.SaveChanges();
        return true;
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
