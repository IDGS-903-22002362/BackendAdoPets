using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Empleados;

namespace AdoPetsBKD.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de empleados
    /// </summary>
    public interface IEmpleadoService
    {
        Task<PagedResponse<EmpleadoListDto>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false);
        Task<EmpleadoDetailDto?> GetByIdAsync(Guid id);
        Task<EmpleadoDetailDto> CreateAsync (CreateEmpleadoDto dto, Guid createdBy);
        Task<EmpleadoDetailDto> UpdateAsync(Guid id, EmpleadoUpdateDto dto, Guid updatedBy);
        Task DeleteAsync (Guid id, Guid deletedBy);
        Task<EmpleadoDetailDto> DarDeBajaAsync(Guid id, Guid performedBy);
        Task<EmpleadoDetailDto> ReactivarAsync(Guid id, Guid performedBy);
    }
}
