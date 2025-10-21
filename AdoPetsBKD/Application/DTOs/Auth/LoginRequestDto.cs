using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Auth;

/// <summary>
/// DTO para solicitud de inicio de sesi�n
/// </summary>
public class LoginRequestDto
{
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es v�lido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrase�a es requerida")]
    [MinLength(6, ErrorMessage = "La contrase�a debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
