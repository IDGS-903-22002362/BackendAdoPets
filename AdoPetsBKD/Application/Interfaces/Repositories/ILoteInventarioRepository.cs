
using AdoPetsBKD.Domain.Entities.Inventario;


namespace AdoPetsBKD.Application.Interfaces.Repositories;


public interface ILoteInventarioRepository
{
    Task AddRangeAsync(IEnumerable<LoteInventario> lotes);



    Task<IEnumerable<LoteInventario>> GetAllAsync();


}