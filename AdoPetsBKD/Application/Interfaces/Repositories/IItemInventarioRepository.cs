using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface IItemInventarioRepository
{
    Task AddAsync(ItemInventario entity);

    Task<ItemInventario?> GetByIdAsync(Guid id);
    Task<List<ItemInventario>> GetAllAsync();
    Task SaveChangesAsync();
}
