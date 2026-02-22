using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IContentStore _store;

    public ProductsController(IContentStore store)
    {
        _store = store;
    }

    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _store.GetCategoriesAsync(cancellationToken);
        var allProducts = await _store.GetProductsAsync(cancellationToken);

        var result = categories.Select(category => new
        {
            category.Id,
            category.Slug,
            category.Name,
            category.Summary,
            category.HowThisHelps,
            category.HeroImage,
            items = allProducts
                .Where(product => product.CategoryId == category.Id
                    && string.Equals(product.Status, "published", StringComparison.OrdinalIgnoreCase))
                .ToList()
        });

        return Ok(new { data = result });
    }

    [HttpGet("allCategories")]
    public async Task<ActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var categories = await _store.GetCategoriesAsync(cancellationToken);
        var allProducts = await _store.GetProductsAsync(cancellationToken);

        var result = categories.Select(category => new
        {
            category.Id,
            category.Slug,
            category.Name,
            category.Summary,
            category.HowThisHelps,
            category.HeroImage,
            items = allProducts
                .Where(product => product.CategoryId == category.Id)
                .ToList()
        });

        return Ok(new { data = result });
    }

    [HttpGet("categories/slug/{slug}")]
    public async Task<ActionResult> GetCategoryBySlug(string slug, CancellationToken cancellationToken)
    {
        var category = await _store.GetCategoryBySlugAsync(slug, cancellationToken);
        if (category == null)
        {
            return ErrorResponse("Category not found", StatusCodes.Status404NotFound);
        }

        var items = await _store.GetProductsByCategoryIdAsync(category.Id, publishedOnly: true, cancellationToken);

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
    public async Task<ActionResult> GetProductBySlug(string categorySlug, string productSlug, CancellationToken cancellationToken)
    {
        var product = await _store.GetProductBySlugAsync(categorySlug, productSlug, cancellationToken);
        if (product == null || !string.Equals(product.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = product });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _store.GetProductByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = product });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Slug))
        {
            return ErrorResponse("Title and slug are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var product = await _store.AddProductAsync(dto, cancellationToken);
            return Ok(new { data = product });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> UpdateProduct(Guid id, [FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Slug))
        {
            return ErrorResponse("Title and slug are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var product = await _store.UpdateProductAsync(id, dto, cancellationToken);
            if (product == null)
            {
                return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
            }

            return Ok(new { data = product });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var removed = await _store.DeleteProductAsync(id, cancellationToken);
        if (!removed)
        {
            return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }

    [HttpPost("categories")]
    [Authorize]
    public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return ErrorResponse("Slug and name are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var category = await _store.AddCategoryAsync(dto, cancellationToken);
            return Ok(new { data = category });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("categories/{id:guid}")]
    [Authorize]
    public async Task<ActionResult> UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return ErrorResponse("Slug and name are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var category = await _store.UpdateCategoryAsync(id, dto, cancellationToken);
            if (category == null)
            {
                return ErrorResponse("Category not found", StatusCodes.Status404NotFound);
            }

            return Ok(new { data = category });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("health")]
    public ActionResult Health()
    {
        return Ok();
    }
}

