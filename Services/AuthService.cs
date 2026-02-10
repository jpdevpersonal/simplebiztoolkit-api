using Microsoft.IdentityModel.Tokens;
using simplebiztoolkit_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace simplebiztoolkit_api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;

    public AuthService(IConfiguration config)
    {
        _config = config;
    }

    public AuthUser? ValidateCredentials(string email, string password)
    {
        var adminEmail = _config["Auth:AdminEmail"] ?? "admin@example.com";
        var adminPassword = _config["Auth:AdminPassword"] ?? "password123";
        var adminName = _config["Auth:AdminName"] ?? "Admin User";
        var adminId = _config["Auth:AdminId"] ?? "admin";

        if (!string.Equals(adminEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!string.Equals(adminPassword, password, StringComparison.Ordinal))
        {
            return null;
        }

        return new AuthUser { Id = adminId, Email = adminEmail, Name = adminName };
    }

    public string GenerateToken(AuthUser user)
    {
        var jwtKey = _config["Auth:JwtKey"] ?? "dev-secret-change";
        var jwtIssuer = _config["Auth:Issuer"] ?? "simplebiztoolkit-api";
        var jwtAudience = _config["Auth:Audience"] ?? "simplebiztoolkit-api";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
