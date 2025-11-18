
using AdoPetsBKD.Application.DTOs.Proveedores;
namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IProveedorService
{
    Task<ProveedorDto> CreateProveedorAsync(CreateProveedorDto dto);
    Task<List<ProveedorDto>> GetAllProveedoresAsync();
    Task<ProveedorDto?> GetProveedorByIdAsync(Guid id);
    Task<ProveedorDto> UpdateProveedorAsync(Guid id, UpdateProveedorDto dto);
    Task DesactivarProveedorAsync(Guid id);
    Task<ProveedorDto> CambiarEstatusProveedorAsync(Guid id, int nuevoEstatus);
}
