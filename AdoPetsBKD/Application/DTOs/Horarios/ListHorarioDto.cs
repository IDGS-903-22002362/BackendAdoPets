namespace AdoPetsBKD.Application.DTOs.Horarios
{
    /// <summary>
    /// DTo para listar los horarios de los empleados
    /// </summary>
    public class ListHorarioDto
    {
        public Guid Id { get; set; }
        public Guid EmpleadoId { get; set; }
        public string NombreCompletoEmpleado { get; set; } = string.Empty;
        public string? CedulaEmpleado { get; set; }
        public string? TipoEmpleado { get; set; }
        // Fecha específica (opcional)
        public DateTime? Fecha { get; set; }
        // Rango de fechas (opcional)
        public DateTime? RangoInicio { get; set; }
        public DateTime? RangoFin { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
        // Tipo de horario (1: Turno, 2: Descanso, 3: Vacaciones, 4: Permiso, 5: Guardia)
        public int Tipo { get; set; }
        // Día de la semana (opcional, para horarios recurrentes)
        public DayOfWeek? DiaSemana { get; set; }
    }
}
