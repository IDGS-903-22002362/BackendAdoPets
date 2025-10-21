using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Domain.Entities.HistorialClinico;

/// <summary>
/// Registro de vacunación
/// </summary>
public class Vacunacion : BaseEntity
{
    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public string VaccineName { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public string? Lot { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextDueAt { get; set; }
    
    public Guid VeterinarioId { get; set; }
    public string? Notes { get; set; }
    public string? ReaccionAdversa { get; set; }

    public bool RequiereRefuerzo => NextDueAt.HasValue && NextDueAt.Value > DateTime.UtcNow;
}
