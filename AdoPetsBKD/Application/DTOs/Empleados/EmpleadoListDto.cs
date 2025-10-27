namespace AdoPetsBKD.Application.DTOs.Empleados
{
    /// <summary>
    /// DTO para listar empleados
    /// </summary>
    public class EmpleadoListDto
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string  EmailLaboral { get; set; } = string.Empty;
        public string? TelefonoLaboral { get; set; } = string.Empty; 
        public string? TipoEmpleado { get; set; } = string.Empty;
        public decimal? Sueldo { get; set; }
        public DateTime? FechaContratacion { get; set; }
        public List<string> Especialidades { get; set; } = new();
    }
}
