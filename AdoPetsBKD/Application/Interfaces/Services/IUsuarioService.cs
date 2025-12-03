using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Usuarios;

namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de usuarios
/// </summary>
public interface IUsuarioService
{
    Task<PagedResponse<UsuarioListDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<UsuarioDetailDto?> GetByIdAsync(Guid id);
    Task<UsuarioDetailDto> CreateAsync(CreateUsuarioDto dto, Guid createdBy);
    Task<UsuarioDetailDto> UpdateAsync(Guid id, UpdateUsuarioDto dto, Guid updatedBy);
    Task DeleteAsync(Guid id, Guid deletedBy);
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> AssignRolesAsync(Guid userId, List<Guid> rolesIds);
    
    /// <summary>
    /// Obtiene lista de adoptantes con todas sus mascotas (adoptadas y registradas)
    /// </summary>
    Task<IEnumerable<AdoptanteConMascotasDto>> GetAdoptantesConMascotasAsync();
    
    /// <summary>
    /// Obtiene un adoptante específico con todas sus mascotas
    /// </summary>
    Task<AdoptanteConMascotasDto?> GetAdoptanteConMascotasByIdAsync(Guid usuarioId);
}
