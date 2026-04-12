using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IContentStore _store;
    private readonly IRevalidationService _revalidationService;

    public ProductsController(IContentStore store, IRevalidationService revalidationService)
    {
        _store = store;
        _revalidationService = revalidationService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = _store.GetCategories().Select(category => new
        {
            category.Id,
            category.Slug,
            category.Name,
            category.Summary,
            category.HowThisHelps,
            category.HeroImage,
            items = _store.GetProducts()
                .Where(product => product.CategoryId == category.Id
                    && string.Equals(product.Status, "published", StringComparison.OrdinalIgnoreCase))
                .ToList()
        });

        return Ok(new { data = categories });
    }

    [HttpGet("/api/admin/categories")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAllCategories()
    {
        var categories = _store.GetCategories().Select(category => new
        {
            category.Id,
            category.Slug,
            category.Name,
            category.Summary,
            category.HowThisHelps,
            category.HeroImage,
            items = _store.GetProducts()
                .Where(product => product.CategoryId == category.Id)
                .ToList()
        });

        return Ok(new { data = categories });
    }

    [HttpGet("categories/slug/{slug}")]
    public async Task<ActionResult> GetCategoryBySlug(string slug)
    {
        var category = _store.GetCategoryBySlug(slug);
        if (category == null)
        {
            return await ErrorResponse("Category not found", StatusCodes.Status404NotFound);
        }

        var items = _store.GetProducts()
            .Where(product => product.CategoryId == category.Id
                && string.Equals(product.Status, "published", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Ok(new
        {
            data = new
            {
                category.Id,
                category.Slug,
                category.Name,
                category.Summary,
                category.HowThisHelps,
                category.HeroImage,
                items
            }
        });
    }

    [HttpGet("slug/{categorySlug}/{productSlug}")]
    public async Task<ActionResult> GetProductBySlug(string categorySlug, string productSlug)
    {
        var product = _store.GetProductBySlug(categorySlug, productSlug);
        if (product == null || !string.Equals(product.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return await ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = product });
    }

    [HttpGet("/api/admin/products/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetProductById(Guid id)
    {
        var product = _store.GetProductById(id);
        if (product == null)
        {
            return await ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        // Return the product regardless of its status
        return Ok(new { data = product });
    }

    [HttpPost("/api/admin/products")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Slug))
        {
            return await ErrorResponse("Title and slug are required.", StatusCodes.Status400BadRequest);
        }

        var product = _store.AddProduct(dto);
        TriggerRevalidation(GetProductRevalidationPaths(product.CategoryId, product.Slug));
        return Ok(new { data = product });
    }

    [HttpPut("/api/admin/products/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> UpdateProduct(Guid id, [FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Slug))
        {
            return await ErrorResponse("Title and slug are required.", StatusCodes.Status400BadRequest);
        }

        var existingProduct = _store.GetProductById(id);
        if (existingProduct == null)
        {
            return await ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        var product = _store.UpdateProduct(id, dto);
        if (product == null)
        {
            return await ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        TriggerRevalidation(
            GetProductRevalidationPaths(existingProduct.CategoryId, existingProduct.Slug)
                .Concat(GetProductRevalidationPaths(product.CategoryId, product.Slug)));

        return Ok(new { data = product });
    }

    [HttpDelete("/api/admin/products/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var existingProduct = _store.GetProductById(id);
        if (existingProduct == null)
        {
            return await ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        var removed = _store.DeleteProduct(id);
        if (!removed)
        {
            return await ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        TriggerRevalidation(GetProductRevalidationPaths(existingProduct.CategoryId, existingProduct.Slug));

        return Ok(new { success = true });
    }

    [HttpPost("/api/admin/categories")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return await ErrorResponse("Slug and name are required.", StatusCodes.Status400BadRequest);
        }

        var category = _store.AddCategory(dto);
        TriggerRevalidation(GetCategoryRevalidationPaths(category.Slug));
        return Ok(new { data = category });
    }

    [HttpPut("/api/admin/categories/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return await ErrorResponse("Slug and name are required.", StatusCodes.Status400BadRequest);
        }

        var existingCategory = _store.GetCategoryById(id);
        if (existingCategory == null)
        {
            return await ErrorResponse("Category not found", StatusCodes.Status404NotFound);
        }

        var category = _store.UpdateCategory(id, dto);
        if (category == null)
        {
            return await ErrorResponse("Category not found", StatusCodes.Status404NotFound);
        }

        TriggerRevalidation(
            GetCategoryRevalidationPaths(existingCategory.Slug)
                .Concat(GetCategoryRevalidationPaths(category.Slug)));

        return Ok(new { data = category });
    }

    [HttpDelete("/api/admin/categories/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var existingCategory = _store.GetCategoryById(id);
        if (existingCategory == null)
        {
            return await ErrorResponse("Category not found", StatusCodes.Status404NotFound);
        }

        var removed = _store.DeleteCategory(id);
        if (!removed)
        {
            return await ErrorResponse("Category not found", StatusCodes.Status404NotFound);
        }

        TriggerRevalidation(GetCategoryRevalidationPaths(existingCategory.Slug));

        return Ok(new { success = true });
    }

    private IEnumerable<string> GetProductRevalidationPaths(Guid categoryId, string productSlug)
    {
        var categorySlug = _store.GetCategoryById(categoryId)?.Slug;
        if (string.IsNullOrWhiteSpace(categorySlug) || string.IsNullOrWhiteSpace(productSlug))
        {
            return GetCategoryRevalidationPaths(categorySlug);
        }

        return GetCategoryRevalidationPaths(categorySlug)
            .Concat([$"/templates/{NormalizePathSegment(categorySlug)}/{NormalizePathSegment(productSlug)}"]);
    }

    private static IEnumerable<string> GetCategoryRevalidationPaths(string? categorySlug)
    {
        if (string.IsNullOrWhiteSpace(categorySlug))
        {
            return [];
        }

        return [$"/templates/{NormalizePathSegment(categorySlug)}"];
    }

    private void TriggerRevalidation(IEnumerable<string> paths)
    {
        var revalidationPaths = paths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (revalidationPaths.Length == 0)
        {
            return;
        }

        _ = Task.Run(() => _revalidationService.RevalidatePathsAsync(revalidationPaths));
    }

    private static string NormalizePathSegment(string value)
        => value.Trim().Trim('/');

}

