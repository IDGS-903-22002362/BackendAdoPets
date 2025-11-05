using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface ISalaRepository
{
    Task<Sala?> GetByIdAsync(Guid id);
    Task<List<Sala>> GetAllAsync();
    Task<List<Sala>> GetActiveAsync();
    Task<Sala?> GetByNombreAsync(string nombre);
    Task AddAsync(Sala sala);
    Task UpdateAsync(Sala sala);
    Task DeleteAsync(Sala sala);
    Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null);
}
