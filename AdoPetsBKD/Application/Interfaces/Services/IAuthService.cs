using AdoPetsBKD.Application.DTOs.Auth;

namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de autenticación
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
    Task LogoutAsync(Guid userId);
}
