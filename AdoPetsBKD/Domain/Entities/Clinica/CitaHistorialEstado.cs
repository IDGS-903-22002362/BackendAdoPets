using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Historial de cambios de estado de una cita
/// </summary>
public class CitaHistorialEstado : BaseEntity
{
    public Guid CitaId { get; set; }
    public Cita Cita { get; set; } = null!;

    public StatusCita FromStatus { get; set; }
    public StatusCita ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Notas { get; set; }
}
