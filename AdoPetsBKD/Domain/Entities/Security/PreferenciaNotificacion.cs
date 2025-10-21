using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Preferencias de notificación por usuario
/// </summary>
public class PreferenciaNotificacion : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public CanalNotificacion Canal { get; set; }
    public CategoriaNotificacion Categoria { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum CanalNotificacion
{
    Push = 1,
    Email = 2,
    SMS = 3
}

public enum CategoriaNotificacion
{
    Adopciones = 1,
    Citas = 2,
    Inventario = 3,
    Donaciones = 4,
    Sistema = 5
}
