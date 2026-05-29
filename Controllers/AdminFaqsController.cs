using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/admin/faqs")]
[Authorize]
[EnableRateLimiting("admin")]
public class AdminFaqsController : ApiControllerBase
{
    private readonly IFaqService _service;
    private readonly IRevalidationService _revalidationService;

    public AdminFaqsController(IFaqService service, IRevalidationService revalidationService)
    {
        _service = service;
        _revalidationService = revalidationService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken ct)
    {
        var data = await _service.GetAllAsync(ct);
        return Ok(new { data });
    }

    [HttpGet("{id:guid}", Name = nameof(GetById))]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        var faq = await _service.GetByIdAsync(id, ct);
        if (faq is null)
        {
            return await ErrorResponse("FAQ not found", StatusCodes.Status404NotFound);
        }

        return Ok(new { data = faq });
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] FaqInputDto input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _service.CreateAsync(input, ct);
        TriggerRevalidation();

        return CreatedAtRoute(nameof(GetById), new { id = created.Id }, new { data = created });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] FaqInputDto input, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var updated = await _service.UpdateAsync(id, input, ct);
        if (updated is null)
        {
            return await ErrorResponse("FAQ not found", StatusCodes.Status404NotFound);
        }

        TriggerRevalidation();
        return Ok(new { data = updated });
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var removed = await _service.DeleteAsync(id, ct);
        if (!removed)
        {
            return await ErrorResponse("FAQ not found", StatusCodes.Status404NotFound);
        }

        TriggerRevalidation();
        return NoContent();
    }

    private void TriggerRevalidation()
    {
        _ = Task.Run(() => _revalidationService.RevalidatePathsAsync(new[] { "/faq" }));
    }
}
