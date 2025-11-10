using AdoPetsBKD.Application.DTOs.Servicios;

namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestionar servicios veterinarios
/// </summary>
public interface IServicioService
{
    Task<List<ServicioDto>> GetAllAsync(bool incluirInactivos = false);
    Task<List<ServicioDto>> GetActivosAsync();
    Task<ServicioDto?> GetByIdAsync(Guid id);
    Task<ServicioDto> CreateAsync(CreateServicioDto dto, Guid createdBy);
    Task<ServicioDto> UpdateAsync(Guid id, UpdateServicioDto dto, Guid updatedBy);
    Task<bool> DeleteAsync(Guid id, Guid deletedBy);
    Task<bool> ActivarAsync(Guid id, Guid updatedBy);
}
