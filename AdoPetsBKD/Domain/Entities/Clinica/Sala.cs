using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Sala de atención veterinaria
/// </summary>
public class Sala : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public bool Activa { get; set; } = true;
    public string? Descripcion { get; set; }
    public int? CapacidadMaxima { get; set; }

    // Navigation properties
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
