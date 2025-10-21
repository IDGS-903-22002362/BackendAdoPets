using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.DTOs.Usuarios;

/// <summary>
/// DTO detallado de usuario
/// </summary>
public class UsuarioDetailDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public EstatusUsuario Estatus { get; set; }
    public DateTime? UltimoAccesoAt { get; set; }
    public string? AceptoPoliticasVersion { get; set; }
    public DateTime? AceptoPoliticasAt { get; set; }
    public List<RolDto> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class RolDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
