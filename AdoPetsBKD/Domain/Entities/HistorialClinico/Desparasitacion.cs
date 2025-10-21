using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Domain.Entities.HistorialClinico;

/// <summary>
/// Registro de desparasitación
/// </summary>
public class Desparasitacion : BaseEntity
{
    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public string Product { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public TipoDesparasitante Tipo { get; set; }
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? NextDueAt { get; set; }
    
    public Guid VeterinarioId { get; set; }
    public string? Notes { get; set; }

    public bool RequiereSiguiente => NextDueAt.HasValue && NextDueAt.Value > DateTime.UtcNow;
}

public enum TipoDesparasitante
{
    Interno = 1,
    Externo = 2,
    Combinado = 3
}
