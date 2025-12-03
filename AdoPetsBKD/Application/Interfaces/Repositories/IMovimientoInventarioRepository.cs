using AdoPetsBKD.Domain.Entities.Inventario;


namespace AdoPetsBKD.Application.Interfaces.Repositories;


public interface IMovimientoInventarioRepository
{
    Task AddRangeAsync(IEnumerable<MovimientoInventario> movimientos);
    Task AddAsync(MovimientoInventario movimiento);
    Task SaveChangesAsync();
}