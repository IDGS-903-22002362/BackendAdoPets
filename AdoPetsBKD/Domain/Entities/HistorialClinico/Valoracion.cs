using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Domain.Entities.HistorialClinico;

/// <summary>
/// Valoración de signos vitales de una mascota
/// </summary>
public class Valoracion : BaseEntity
{
    public Guid CitaId { get; set; }
    public Cita Cita { get; set; } = null!;

    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public decimal? Peso { get; set; } // kg con 2 decimales
    public decimal? Temperatura { get; set; } // °C con 1 decimal
    public int? FrecuenciaCardiaca { get; set; } // latidos por minuto
    public int? FrecuenciaRespiratoria { get; set; }
    public string? CondicionCorporal { get; set; }
    public string? Observaciones { get; set; }
    
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;
    public Guid TakenBy { get; set; }

    public bool TieneSignosVitalesNormales()
    {
        // Valores aproximados, ajustar según especie y tamaño
        return Temperatura is >= 37.5m and <= 39.5m
            && FrecuenciaCardiaca is >= 60 and <= 140;
    }
}
