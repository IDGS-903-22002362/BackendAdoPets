using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Usuarios;

/// <summary>
/// DTO para actualizar un usuario existente
/// </summary>
public class UpdateUsuarioDto
{
    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ApellidoPaterno { get; set; } = string.Empty;

    [StringLength(100)]
    public string ApellidoMaterno { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? Telefono { get; set; }

    public List<Guid> RolesIds { get; set; } = new();
}
