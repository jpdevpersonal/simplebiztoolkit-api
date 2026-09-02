using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/stats")]
public sealed class StatsController : ApiControllerBase
{
    private readonly IStatService _service;

    public StatsController(IStatService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> Get(CancellationToken ct)
    {
        var data = await _service.GetVisibleAsync(ct);
        return Ok(new { data });
    }

    [HttpGet("{name}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetByName(string name, CancellationToken ct)
    {
        if (!_service.IsSupportedName(name))
        {
            return await ErrorResponse("Unsupported stat name.", StatusCodes.Status400BadRequest);
        }

        var stat = await _service.GetByNameAsync(name, includeHidden: false, ct);
        if (stat is null)
        {
            return await ErrorResponse("Stat not found.", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = stat });
    }
}
