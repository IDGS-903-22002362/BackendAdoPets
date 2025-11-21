using System.ComponentModel.DataAnnotations;
using AdoPetsBKD.Application.DTOs.Horarios;
namespace AdoPetsBKD.Application.DTOs.Horarios
{
    /// <summary>
    /// DTO para actualizar un horario existente
    /// </summary>
    public class UpdateHorarioDto
    {
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
