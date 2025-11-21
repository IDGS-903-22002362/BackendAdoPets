namespace AdoPetsBKD.Application.DTOs.Horarios
{
    /// <summary>
    /// DTO para el calendario general de todos los empleados
    /// Agrupa los horarios por empleado
    /// </summary>
    public class CalendarioGeneralDto
    {
        public Guid EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Cedula { get; set; }
        public string TipoEmpleado { get; set; } = string.Empty;
        public string? EmailLaboral { get; set; }
        public List<CalendarioHorarioDto> Dias { get; set; } = new();
        public EstadisticasEmpleadoDto Estadisticas { get; set; } = new();
    }
    
    public class EstadisticasEmpleadoDto
    {
        public int DiasConHorario { get; set; }

        public int DiasSinHorario { get; set; }

        public int DiasVacaciones { get; set; }

        public int DiasPermiso { get; set; }

        public int DiasGuardia { get; set; }

        public int DiasTurno { get; set; }        

        public int DiasDescanso { get; set; }

        public int TotalExcepciones { get; set; }
    }
}
