using simplebiztoolkit_api.Models;

namespace simplebiztoolkit_api.Services;

public interface IAuthService
{
    AuthUser? ValidateCredentials(string email, string password);
    string GenerateToken(AuthUser user);
}
