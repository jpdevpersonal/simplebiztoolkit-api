using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/admin/stats")]
[Authorize]
[EnableRateLimiting("admin")]
public sealed class AdminStatsController : ApiControllerBase
{
    private readonly IStatService _service;

    public AdminStatsController(IStatService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var data = await _service.GetAllAsync(ct);
        return Ok(new { data });
    }

    [HttpGet("{name}")]
    public async Task<ActionResult> GetByName(string name, CancellationToken ct)
    {
        if (!_service.IsSupportedName(name))
        {
            return await ErrorResponse("Unsupported stat name.", StatusCodes.Status400BadRequest);
        }

        var stat = await _service.GetByNameAsync(name, includeHidden: true, ct);
        if (stat is null)
        {
            return await ErrorResponse("Stat not found.", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = stat });
    }

    [HttpPut("{name}")]
    public async Task<ActionResult> Upsert(
        string name,
        [FromBody] StatValueInputDto input,
        CancellationToken ct)
    {
        if (!_service.IsSupportedName(name))
        {
            return await ErrorResponse("Unsupported stat name.", StatusCodes.Status400BadRequest);
        }

        var data = await _service.UpsertAsync(name, input, ct);
        return Ok(new { data });
    }

    [HttpPut]
    public async Task<ActionResult> UpsertMany(
        [FromBody] List<UpsertStatDto> inputs,
        CancellationToken ct)
    {
        if (inputs.Count == 0)
        {
            return await ErrorResponse("At least one stat is required.", StatusCodes.Status400BadRequest);
        }

        var unsupportedNames = inputs
            .Where(input => !_service.IsSupportedName(input.Name))
            .Select(input => input.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (unsupportedNames.Count > 0)
        {
            return await ErrorResponse(
                $"Unsupported stat name(s): {string.Join(", ", unsupportedNames)}.",
                StatusCodes.Status400BadRequest);
        }

        var data = await _service.UpsertManyAsync(inputs, ct);
        return Ok(new { data });
    }
}
