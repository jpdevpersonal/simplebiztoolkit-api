using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/menucategories")]
public class MenuCategoriesController : ApiControllerBase
{
    private readonly IMenuStore _store;

    public MenuCategoriesController(IMenuStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] Guid? menuItemId)
    {
        var categories = await _store.GetMenuCategoriesAsync(menuItemId);
        return Ok(new { data = categories });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var category = await _store.GetMenuCategoryByIdAsync(id);
        if (category == null)
        {
            return await ErrorResponse("MenuCategory not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = category });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromBody] CreateMenuCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Title is required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var category = await _store.AddMenuCategoryAsync(dto);
            return Ok(new { data = category });
        }
        catch (InvalidOperationException ex)
        {
            return await ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateMenuCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Title is required.", StatusCodes.Status400BadRequest);
        }

        try
        {
            var category = await _store.UpdateMenuCategoryAsync(id, dto);
            if (category == null)
            {
                return await ErrorResponse("MenuCategory not found", StatusCodes.Status404NotFound);
            }

            return Ok(new { data = category });
        }
        catch (InvalidOperationException ex)
        {
            return await ErrorResponse(ex.Message, StatusCodes.Status400BadRequest);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var removed = await _store.DeleteMenuCategoryAsync(id);
        if (!removed)
        {
            return await ErrorResponse("MenuCategory not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }
}
