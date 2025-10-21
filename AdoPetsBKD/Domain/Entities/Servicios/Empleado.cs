using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Domain.Entities.Servicios;

/// <summary>
/// Información extendida de empleados (veterinarios, asistentes, etc.)
/// </summary>
public class Empleado : AuditableEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string? Cedula { get; set; }
    public string? Disponibilidad { get; set; }
    public string? EmailLaboral { get; set; }
    public string? TelefonoLaboral { get; set; }
    public TipoEmpleado Tipo { get; set; }
    public decimal? Sueldo { get; set; }
    
    public DateTime? FechaContratacion { get; set; }
    public DateTime? FechaBaja { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation properties
    public ICollection<EmpleadoEspecialidad> Especialidades { get; set; } = new List<EmpleadoEspecialidad>();
    public ICollection<Horario> Horarios { get; set; } = new List<Horario>();

    public void DarDeBaja(Guid userId)
    {
        Activo = false;
        FechaBaja = DateTime.UtcNow;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reactivar(Guid userId)
    {
        Activo = true;
        FechaBaja = null;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum TipoEmpleado
{
    Veterinario = 1,
    Asistente = 2,
    Recepcionista = 3,
    Administrador = 4,
    Voluntario = 5
}
