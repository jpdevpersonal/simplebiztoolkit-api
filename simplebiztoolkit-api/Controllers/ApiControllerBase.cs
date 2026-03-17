using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace simplebiztoolkit_api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<ActionResult> ErrorResponse(string message, int statusCode)
    {
        return await Task.FromResult<ActionResult>(Problem(
            title: ReasonPhrases.GetReasonPhrase(statusCode),
            detail: message,
            statusCode: statusCode));
    }
}
