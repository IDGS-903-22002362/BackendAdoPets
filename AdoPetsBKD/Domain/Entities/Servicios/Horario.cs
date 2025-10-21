using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Servicios;

/// <summary>
/// Horario de trabajo de empleados
/// </summary>
public class Horario : BaseEntity
{
    public Guid EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;

    // Opción 1: Fecha específica
    public DateTime? Fecha { get; set; }

    // Opción 2: Rango de fechas (para horarios recurrentes)
    public DateTime? RangoInicio { get; set; }
    public DateTime? RangoFin { get; set; }

    public TimeSpan? HoraEntrada { get; set; }
    public TimeSpan? HoraSalida { get; set; }

    public TipoHorario Tipo { get; set; }
    public DayOfWeek? DiaSemana { get; set; }
    public string? Notas { get; set; }

    public bool EsValido(DateTime fechaConsulta)
    {
        if (Fecha.HasValue)
            return Fecha.Value.Date == fechaConsulta.Date;

        if (RangoInicio.HasValue && RangoFin.HasValue)
            return fechaConsulta.Date >= RangoInicio.Value.Date && fechaConsulta.Date <= RangoFin.Value.Date;

        return false;
    }
}

public enum TipoHorario
{
    Turno = 1,
    Descanso = 2,
    Vacaciones = 3,
    Permiso = 4,
    Guardia = 5
}
