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
    public ActionResult GetCategories()
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

    [HttpGet("allCategories")]
    public ActionResult GetAllCategories()
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
    public ActionResult GetCategoryBySlug(string slug)
    {
        var category = _store.GetCategoryBySlug(slug);
        if (category == null)
        {
            return ErrorResponse("Category not found", StatusCodes.Status404NotFound);
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
    public ActionResult GetProductBySlug(string categorySlug, string productSlug)
    {
        var product = _store.GetProductBySlug(categorySlug, productSlug);
        if (product == null || !string.Equals(product.Status, "published", StringComparison.OrdinalIgnoreCase))
        {
            return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = product });
    }

    [HttpGet("{id:guid}")]
    public ActionResult GetProductById(Guid id)
    {
        var product = _store.GetProductById(id);
        if (product == null)
        {
            return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        // Return the product regardless of its status
        return Ok(new { data = product });
    }

    [HttpPost]
    [Authorize]
    public ActionResult CreateProduct([FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Slug))
        {
            return ErrorResponse("Title and slug are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var product = _store.AddProduct(dto);
            return Ok(new { data = product });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public ActionResult UpdateProduct(Guid id, [FromBody] CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Slug))
        {
            return ErrorResponse("Title and slug are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var product = _store.UpdateProduct(id, dto);
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
    public ActionResult DeleteProduct(Guid id)
    {
        var removed = _store.DeleteProduct(id);
        if (!removed)
        {
            return ErrorResponse("Product not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }

    [HttpPost("categories")]
    [Authorize]
    public ActionResult CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return ErrorResponse("Slug and name are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var category = _store.AddCategory(dto);
            return Ok(new { data = category });
        }
        catch (InvalidOperationException ex)
        {
            return ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("categories/{id:guid}")]
    [Authorize]
    public ActionResult UpdateCategory(Guid id, [FromBody] CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Name))
        {
            return ErrorResponse("Slug and name are required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var category = _store.UpdateCategory(id, dto);
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

