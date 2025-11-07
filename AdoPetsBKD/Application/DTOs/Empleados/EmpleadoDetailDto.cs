namespace AdoPetsBKD.Application.DTOs.Empleados
{

    /// <summary>
    /// DTO para ver los detalles de un empleado
    /// </summary>
    public class EmpleadoDetailDto
    {
        public Guid Id { get; set; }
        public string Cedula { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string EmailLaboral { get; set; } = string.Empty;
        public string? TelefonoLaboral { get; set; }
        public DateTime? FechaContratacion { get; set; }

        public string TipoEmpleado { get; set; } = string.Empty;

        public decimal? Sueldo { get; set; }

        public string Disponibilidad { get; set; } = string.Empty;
        
        public List<EspecialidadEmpleadoDto> Especialidades { get; set; } = new();
    }
}
