using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Auth;

/// <summary>
/// DTO para autenticación con Firebase ID Token
/// </summary>
public class FirebaseLoginRequestDto
{
    /// <summary>
    /// Token de Firebase ID obtenido desde la app móvil
    /// </summary>
    [Required(ErrorMessage = "El token de Firebase es requerido")]
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Opcional: Información adicional del dispositivo
    /// </summary>
    public string? DeviceInfo { get; set; }
}
