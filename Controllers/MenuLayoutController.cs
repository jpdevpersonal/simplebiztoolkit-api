using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/menu-layout")]
public class MenuLayoutController : ApiControllerBase
{
    private readonly IMenuStore _store;

    public MenuLayoutController(IMenuStore store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] string? menuKey = "primary")
    {
        var key = string.IsNullOrWhiteSpace(menuKey) ? "primary" : menuKey.Trim();
        var settings = await _store.GetMenuLayoutSettingsAsync(key);
        return Ok(new { data = settings });
    }

    [HttpGet("/api/admin/menu-layout")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> GetAdmin([FromQuery] string? menuKey = "primary")
    {
        var key = string.IsNullOrWhiteSpace(menuKey) ? "primary" : menuKey.Trim();
        var settings = await _store.GetMenuLayoutSettingsAsync(key);
        return Ok(new { data = settings });
    }

    [HttpPut("/api/admin/menu-layout")]
    [Authorize]
    [EnableRateLimiting("admin")]
    public async Task<ActionResult> Upsert([FromBody] UpsertMenuLayoutSettingsRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.MenuKey))
        {
            return await ErrorResponse("menuKey is required.", StatusCodes.Status400BadRequest);
        }

        if (request.OrderedMenuItemIds is null)
        {
            return await ErrorResponse("orderedMenuItemIds is required.", StatusCodes.Status400BadRequest);
        }

        if (request.Version <= 0)
        {
            return await ErrorResponse("version must be greater than 0.", StatusCodes.Status400BadRequest);
        }

        var normalizedIds = new List<string>(request.OrderedMenuItemIds.Count);
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawId in request.OrderedMenuItemIds)
        {
            if (string.IsNullOrWhiteSpace(rawId))
            {
                return await ErrorResponse("orderedMenuItemIds must not contain blank values.", StatusCodes.Status400BadRequest);
            }

            var id = rawId.Trim();
            if (!IsValidMenuLayoutOrderId(id))
            {
                return await ErrorResponse($"Invalid menu item id: {rawId}", StatusCodes.Status400BadRequest);
            }

            if (seenIds.Add(id))
            {
                normalizedIds.Add(id);
            }
        }

        var dto = new UpsertMenuLayoutSettingsDto
        {
            MenuKey = request.MenuKey.Trim(),
            OrderedMenuItemIds = normalizedIds,
            IsActive = request.IsActive,
            Version = request.Version,
            UpdatedBy = request.UpdatedBy
        };

        var settings = await _store.UpsertMenuLayoutSettingsAsync(dto);
        return Ok(new { data = settings });
    }

    private static bool IsValidMenuLayoutOrderId(string id)
    {
        if (Guid.TryParse(id, out _))
        {
            return true;
        }

        if (id.StartsWith("static:/", StringComparison.OrdinalIgnoreCase))
        {
            return id.Length > "static:/".Length;
        }

        if (id.StartsWith("hidden-static:/", StringComparison.OrdinalIgnoreCase))
        {
            return id.Length > "hidden-static:/".Length;
        }

        return id.StartsWith("cms:", StringComparison.OrdinalIgnoreCase)
            && Guid.TryParse(id["cms:".Length..], out _);
    }
}
