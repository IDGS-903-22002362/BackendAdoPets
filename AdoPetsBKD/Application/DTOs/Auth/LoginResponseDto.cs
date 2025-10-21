namespace AdoPetsBKD.Application.DTOs.Auth;

/// <summary>
/// DTO para respuesta de autenticaci�n
/// </summary>
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UsuarioDto Usuario { get; set; } = null!;
}
