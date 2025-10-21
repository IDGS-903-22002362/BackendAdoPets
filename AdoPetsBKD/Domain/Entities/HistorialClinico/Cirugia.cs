using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Domain.Entities.HistorialClinico;

/// <summary>
/// Registro de cirugía
/// </summary>
public class Cirugia : BaseEntity
{
    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public string Tipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    
    public Guid VeterinarioId { get; set; }
    public string? Anesthesia { get; set; }
    public int? DuracionMin { get; set; }
    public bool Complications { get; set; }
    public string? Notes { get; set; }
    public string? Medicacion { get; set; }
    public string? CuidadosPostoperatorios { get; set; }

    public DateTime? FechaRevision { get; set; }
}
