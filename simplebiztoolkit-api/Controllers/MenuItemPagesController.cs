using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/menuitempages")]
public class MenuItemPagesController : ApiControllerBase
{
    private readonly IMenuStore _store;

    public MenuItemPagesController(IMenuStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] Guid? menuCategoryId, [FromQuery] string? status)
    {
        if (!string.Equals(status, "published", StringComparison.OrdinalIgnoreCase))
        {
            status = "published";
        }

        var pages = await _store.GetMenuItemPagesAsync(menuCategoryId, status);
        return Ok(new { data = pages });
    }

    [HttpGet("/api/admin/pages")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAllAdmin([FromQuery] Guid? menuCategoryId, [FromQuery] string? status)
    {
        var pages = await _store.GetMenuItemPagesAsync(menuCategoryId, status);
        return Ok(new { data = pages });
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult> GetBySlug(string slug)
    {
        var page = await _store.GetMenuItemPageBySlugAsync(slug);
        var isAuthenticated = User?.Identity?.IsAuthenticated == true;

        if (page == null || (!isAuthenticated && !string.Equals(page.Status, "published", StringComparison.OrdinalIgnoreCase)))
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = page });
    }

    [HttpGet("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var page = await _store.GetMenuItemPageByIdAsync(id);
        if (page == null)
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = page });
    }

    [HttpPost("/api/admin/pages")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Create([FromBody] CreateMenuItemPageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var page = await _store.AddMenuItemPageAsync(dto);
        return Ok(new { data = page });
    }

    [HttpPut("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateMenuItemPageDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Slug and title are required.", StatusCodes.Status400BadRequest);
        }

        var page = await _store.UpdateMenuItemPageAsync(id, dto);
        if (page == null)
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = page });
    }

    [HttpDelete("/api/admin/pages/{id:guid}")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var removed = await _store.DeleteMenuItemPageAsync(id);
        if (!removed)
        {
            return await ErrorResponse("Page not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }
}
