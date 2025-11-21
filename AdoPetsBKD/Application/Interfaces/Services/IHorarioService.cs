using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Horarios;
using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de horarios
    /// </summary>
    public interface IHorarioService
    {
        Task<PagedResponse<ListHorarioDto>> GetAllAsync(int pageNumber, int pageSize, DateTime? fechaInicio = null, DateTime? fechaFin = null, TipoHorario? tipo = null);
        Task<DetailHorarioDto?> GetByIdAsync(Guid id);
        Task<DetailHorarioDto> CreateAsync(CreateHorarioDto dto); 
        Task<DetailHorarioDto> UpdateAsync(Guid id, UpdateHorarioDto dto);
        Task DeleteAsync(Guid id);
        Task<DetailHorarioDto?> GetHorarioEfectivoAsync(Guid empleadoId, DateTime fecha);
        Task<List<ListHorarioDto>> GetHorariosAplicablesAsync(Guid empleadoId, DateTime fecha);
        Task<List<CalendarioHorarioDto>> GetCalendarioAsync(Guid empleadoId, DateTime fechaInicio, DateTime fechaFin);
        Task<List<CalendarioGeneralDto>> GetCalendarioGeneralAsync(DateTime fechaInicio, DateTime fechaFin, bool incluirInactivos = false);
    }
}
