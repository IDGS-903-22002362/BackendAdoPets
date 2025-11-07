namespace AdoPetsBKD.Application.DTOs.Empleados
{
    /// <summary>
    /// DTO para asignar especialidades a un empleado
    /// </summary>
    public class AsignarEspecialidadesDto
    {
        public List<EspecialidadAsignacionDto> Especialidades { get; set; } = new();
    }

    public class EspecialidadAsignacionDto
    {
        public Guid EspecialidadId { get; set; }
        public string? Certificacion { get; set; }
    }
}
