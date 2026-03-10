using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using simplebiztoolkit_api.Dtos;
using simplebiztoolkit_api.Services;
using System.IdentityModel.Tokens.Jwt;

namespace simplebiztoolkit_api.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequestDto request)
    {
        var user = _authService.ValidateCredentials(request.Email, request.Password);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return await ErrorResponse("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        var token = _authService.GenerateToken(user);
        var expiresAtUtc = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;

        return Ok(new { token, expiresAtUtc, user = new { id = user.Id, email = user.Email, name = user.Name } });
    }
}
