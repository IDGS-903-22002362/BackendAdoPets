using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Servicios;

/// <summary>
/// Especialidad veterinaria
/// </summary>
public class Especialidad : BaseEntity
{
    public string Descripcion { get; set; } = string.Empty;
    public string? Codigo { get; set; }

    // Navigation properties
    public ICollection<EmpleadoEspecialidad> EmpleadoEspecialidades { get; set; } = new List<EmpleadoEspecialidad>();
}
