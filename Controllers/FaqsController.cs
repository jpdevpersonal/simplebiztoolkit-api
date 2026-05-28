using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/faqs")]
public class FaqsController : ApiControllerBase
{
    private readonly IFaqService _service;

    public FaqsController(IFaqService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> Get(CancellationToken ct)
    {
        var data = await _service.GetPublishedAsync(ct);
        return Ok(new { data });
    }
}
