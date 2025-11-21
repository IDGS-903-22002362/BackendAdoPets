using AdoPetsBKD.Domain.Entities.Servicios;
namespace AdoPetsBKD.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de horarios
    /// </summary>
    public interface IHorarioRepositoy
    {
        Task<Horario?> GetByIdAsync(Guid id);
        Task<Horario> CreateAsync(Horario horario);
        Task<Horario> UpdateAsync(Horario horario);
        Task<Horario> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<List<Horario>> GetAllAsync(int pageNumber = 1, int pageSize = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null, TipoHorario? tipo = null);
        Task<int> GetTotalCountAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, TipoHorario? tipo = null);
        Task<Horario?> GetHorarioEfectivoAsync(Guid empleadoId, DateTime fecha);
        Task<List<Horario>> GetHorariosAplicablesAsync(Guid empleadoId, DateTime fecha);
        Task<List<Horario>> GetConflictosAsync(Guid empleadoId, DateTime? fecha, DateTime? rangoInicio, DateTime? rangoFin, DayOfWeek? diaSemana, Guid? horarioIdExcluir = null);
        
        Task SaveChangesAsync();
    }

}
