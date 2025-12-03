using AdoPetsBKD.Domain.Entities.Inventario;


namespace AdoPetsBKD.Application.Interfaces.Repositories;


public interface ILoteInventarioRepository
{
    Task AddRangeAsync(IEnumerable<LoteInventario> lotes);



    Task<IEnumerable<LoteInventario>> GetAllAsync();


    Task<List<LoteInventario>> GetLotesDisponiblesByItemIdAsync(Guid itemId);


    Task UpdateAsync(LoteInventario lote);


    Task UpdateRangeAsync(IEnumerable<LoteInventario> lotes);


    Task SaveChangesAsync();


}