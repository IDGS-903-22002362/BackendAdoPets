using System.Security.Claims;

namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de generación de tokens JWT
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, List<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}
