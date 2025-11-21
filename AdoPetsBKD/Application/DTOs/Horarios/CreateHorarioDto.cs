using AdoPetsBKD.Application.DTOs.Horarios;
using System.ComponentModel.DataAnnotations;
namespace AdoPetsBKD.Application.DTOs.Horarios
{
    /// <summary>
    /// DTO para crear un nuevo horario
    /// </summary>
    public class CreateHorarioDto
    {
        [Required(ErrorMessage = "El horario debe estar asociado a un empleado")]
        public Guid EmpleadoId { get; set; }
        // Fecha específica (opcional)
        public DateTime? Fecha { get; set; }
        // Rango de fechas (opcional)
        public DateTime? RangoInicio { get; set; }
        public DateTime? RangoFin { get; set; }
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }
        // Tipo de horario (1: Turno, 2: Descanso, 3: Vacaciones, 4: Permiso, 5: Guardia)
        [Required(ErrorMessage = "El tipo de horario es requerido")]
        public int Tipo { get; set; }
        // Día de la semana (opcional, para horarios recurrentes)
        public DayOfWeek? DiaSemana { get; set; }
        // Notas adicionales (opcional)
        public string? Notas { get; set; } = null;
    }
}
