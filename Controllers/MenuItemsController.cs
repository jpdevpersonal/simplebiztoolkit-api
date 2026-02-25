using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/menuitems")]
public class MenuItemsController : ApiControllerBase
{
    private readonly IMenuStore _store;

    public MenuItemsController(IMenuStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll()
    {
        var items = await _store.GetMenuItemsAsync();
        return Ok(new { data = items });
    }

    [HttpGet("items-tree")]
    public async Task<ActionResult> GetItemAndChildren()
    {
        var items = await _store.GetMenuItemsAndChildrenAsync();
        return Ok(new { data = items });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var item = await _store.GetMenuItemByIdAsync(id);
        if (item == null)
        {
            return await ErrorResponse("MenuItem not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = item });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create([FromBody] CreateMenuItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Title is required.", StatusCodes.Status400BadRequest);
        }

        var item = await _store.AddMenuItemAsync(dto);
        return Ok(new { data = item });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Update(Guid id, [FromBody] CreateMenuItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return await ErrorResponse("Title is required.", StatusCodes.Status400BadRequest);
        }

        var item = await _store.UpdateMenuItemAsync(id, dto);
        if (item == null)
        {
            return await ErrorResponse("MenuItem not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = item });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var removed = await _store.DeleteMenuItemAsync(id);
        if (!removed)
        {
            return await ErrorResponse("MenuItem not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { success = true });
    }
}
