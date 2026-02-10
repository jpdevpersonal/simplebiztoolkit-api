using Microsoft.AspNetCore.Mvc;

namespace simplebiztoolkit_api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ErrorResponse(string message, int statusCode)
    {
        return StatusCode(statusCode, new { error = message, statusCode });
    }
}
