using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Domain.Entities.HistorialClinico;

/// <summary>
/// Expediente médico o nota clínica
/// </summary>
public class Expediente : BaseEntity
{
    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public Guid VeterinarioId { get; set; }
    public Guid? CitaId { get; set; }
    public Cita? Cita { get; set; }

    public string? MotivoConsulta { get; set; }
    public string? Anamnesis { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string? Tratamiento { get; set; }
    public string? Notas { get; set; }
    public string? Pronostico { get; set; }
    
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<AdjuntoMedico> Adjuntos { get; set; } = new List<AdjuntoMedico>();
}
