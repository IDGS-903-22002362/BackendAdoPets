using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.DTOs.Usuarios;

/// <summary>
/// DTO para listado de usuarios
/// </summary>
public class UsuarioListDto
{
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public EstatusUsuario Estatus { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? UltimoAccesoAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
