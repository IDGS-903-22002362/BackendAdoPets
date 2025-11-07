namespace AdoPetsBKD.Application.DTOs.Empleados
{
    /// <summary>
    /// DTO para mostrar una especialidad del empleado
    /// </summary>
    public class EspecialidadEmpleadoDto
    {
        public Guid EspecialidadId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string? Codigo { get; set; }
        public string? Certificacion { get; set; }
        public DateTime ObtainedAt { get; set; }
    }
}
