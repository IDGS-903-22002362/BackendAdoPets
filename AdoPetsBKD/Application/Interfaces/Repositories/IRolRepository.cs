using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para el repositorio de roles
/// </summary>
public interface IRolRepository
{
    Task<Rol?> GetByIdAsync(Guid id);
    Task<Rol?> GetByNameAsync(string nombre);
    Task<List<Rol>> GetAllAsync();
    Task<List<Rol>> GetByIdsAsync(List<Guid> ids);
    Task<bool> ExistsAsync(Guid id);
}
