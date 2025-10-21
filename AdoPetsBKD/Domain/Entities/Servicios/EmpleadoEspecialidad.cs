namespace AdoPetsBKD.Domain.Entities.Servicios;

/// <summary>
/// Relación N:M entre Empleado y Especialidad
/// </summary>
public class EmpleadoEspecialidad
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    public Guid EspecialidadId { get; set; }
    public Especialidad Especialidad { get; set; } = null!;

    public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;
    public string? Certificacion { get; set; }
}
