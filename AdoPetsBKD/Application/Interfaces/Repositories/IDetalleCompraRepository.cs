using AdoPetsBKD.Domain.Entities.Inventario;


namespace AdoPetsBKD.Application.Interfaces.Repositories;


public interface IDetalleCompraRepository
{
    Task AddRangeAsync(IEnumerable<DetalleCompra> detalles);
}