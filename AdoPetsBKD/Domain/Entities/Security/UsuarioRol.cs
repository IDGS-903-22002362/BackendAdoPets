using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Relación N:M entre Usuario y Rol
/// </summary>
public class UsuarioRol
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public Guid RolId { get; set; }
    public Rol Rol { get; set; } = null!;

    public DateTime AsignadoAt { get; set; } = DateTime.UtcNow;
    public Guid? AsignadoPor { get; set; }
}
