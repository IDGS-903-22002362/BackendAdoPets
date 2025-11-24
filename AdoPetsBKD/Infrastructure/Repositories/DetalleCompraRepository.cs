using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;


namespace AdoPetsBKD.Infrastructure.Repositories;


public class DetalleCompraRepository : IDetalleCompraRepository
{
    private readonly AdoPetsDbContext _ctx;
    public DetalleCompraRepository(AdoPetsDbContext ctx) => _ctx = ctx;


    public async Task AddRangeAsync(IEnumerable<DetalleCompra> detalles)
    {
        await _ctx.DetallesCompras.AddRangeAsync(detalles);
        await _ctx.SaveChangesAsync();
    }
}