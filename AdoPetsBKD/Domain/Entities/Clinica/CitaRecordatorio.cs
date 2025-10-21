using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Recordatorio de cita
/// </summary>
public class CitaRecordatorio : BaseEntity
{
    public Guid CitaId { get; set; }
    public Cita Cita { get; set; } = null!;

    public TipoRecordatorio Tipo { get; set; }
    public DateTime? SentAt { get; set; }
    public bool WasSent => SentAt.HasValue;
}

public enum TipoRecordatorio
{
    Horas24 = 1,
    Horas2 = 2,
    Hora1 = 3
}
