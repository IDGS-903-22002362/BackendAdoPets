namespace AdoPetsBKD.Application.DTOs.Horarios
{
    /// <summary>
    /// DTO para mostrar horarios en un calendario (día por día)
    /// </summary>
    public class CalendarioHorarioDto
    {
        public DateTime Fecha { get; set; }
        public DayOfWeek DiaSemana { get; set; }

        public Guid? HorarioId { get; set; }

        public Guid EmpleadoId { get; set; }

        public string NombreCompletoEmpleado { get; set; } = string.Empty;

        public TimeSpan? HoraEntrada { get; set; }

        public TimeSpan? HoraSalida { get; set; }

        public int? Tipo { get; set; }

        public string? TipoNombre { get; set; }

        public string? Notas { get; set; }

        public bool TieneHorario { get; set; }

        public bool EsExcepcion { get; set; }

        public Guid? HorarioAnuladoId { get; set; }

        public int Prioridad { get; set; }
    }
}
