using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;

namespace simplebiztoolkit_api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public ActionResult Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return ErrorResponse("Email and password are required.", StatusCodes.Status400BadRequest);
        }

        var user = _authService.ValidateCredentials(request.Email, request.Password);
        if (user == null)
        {
            return ErrorResponse("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        var token = _authService.GenerateToken(user);
        return Ok(new { token, user = new { id = user.Id, email = user.Email, name = user.Name } });
    }
}
