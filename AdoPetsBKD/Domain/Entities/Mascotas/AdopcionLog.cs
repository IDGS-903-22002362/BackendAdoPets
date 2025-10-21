using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Mascotas;

/// <summary>
/// Log de cambios de estado en solicitud de adopción
/// </summary>
public class AdopcionLog : BaseEntity
{
    public Guid SolicitudId { get; set; }
    public SolicitudAdopcion Solicitud { get; set; } = null!;

    public EstadoSolicitudAdopcion FromEstado { get; set; }
    public EstadoSolicitudAdopcion ToEstado { get; set; }
    public string? Reason { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
