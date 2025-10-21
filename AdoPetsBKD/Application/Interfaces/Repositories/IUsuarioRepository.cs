using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para el repositorio de usuarios
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(Guid id, bool includeRoles = false);
    Task<Usuario?> GetByEmailAsync(string email, bool includeRoles = false);
    Task<List<Usuario>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool includeRoles = false);
    Task<int> GetTotalCountAsync();
    Task<Usuario> CreateAsync(Usuario usuario);
    Task<Usuario> UpdateAsync(Usuario usuario);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null);
    Task SaveChangesAsync();
}
