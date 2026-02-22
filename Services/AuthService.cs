using Microsoft.IdentityModel.Tokens;
using simplebiztoolkit_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace simplebiztoolkit_api.Services;

public class AuthService : IAuthService
{
    private readonly string _adminEmail;
    private readonly byte[] _adminPasswordBytes;
    private readonly string _adminName;
    private readonly string _adminId;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _tokenExpiryHours;

    public AuthService(IConfiguration config)
    {
        _adminEmail = config["Auth:AdminEmail"] ?? "admin@example.com";
        _adminPasswordBytes = Encoding.UTF8.GetBytes(config["Auth:AdminPassword"] ?? "password123");
        _adminName = config["Auth:AdminName"] ?? "Admin User";
        _adminId = config["Auth:AdminId"] ?? "admin";
        _jwtKey = config["Auth:JwtKey"] ?? "dev-secret-change";
        _jwtIssuer = config["Auth:Issuer"] ?? "simplebiztoolkit-api";
        _jwtAudience = config["Auth:Audience"] ?? "simplebiztoolkit-api";
        _tokenExpiryHours = int.TryParse(config["Auth:TokenExpiryHours"], out var h) ? h : 2;
    }

    public AuthUser? ValidateCredentials(string email, string password)
    {
        if (!string.Equals(_adminEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Constant-time comparison prevents timing attacks
        var providedBytes = Encoding.UTF8.GetBytes(password);
        if (!CryptographicOperations.FixedTimeEquals(_adminPasswordBytes, providedBytes))
        {
            return null;
        }

        return new AuthUser { Id = _adminId, Email = _adminEmail, Name = _adminName };
    }

    public string GenerateToken(AuthUser user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_tokenExpiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
