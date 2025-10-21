using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.DTOs.Auth;

/// <summary>
/// DTO para información de usuario autenticado
/// </summary>
public class UsuarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public EstatusUsuario Estatus { get; set; }
    public DateTime? UltimoAccesoAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool TienePoliticasAceptadas { get; set; }
    public DateTime CreatedAt { get; set; }
}
