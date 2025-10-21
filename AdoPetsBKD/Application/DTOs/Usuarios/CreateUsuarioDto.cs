using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Usuarios;

/// <summary>
/// DTO para crear un nuevo usuario
/// </summary>
public class CreateUsuarioDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido paterno es requerido")]
    [StringLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(100)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public List<Guid> RolesIds { get; set; } = new();
}
