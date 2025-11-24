using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;


namespace AdoPetsBKD.Infrastructure.Repositories;


public class MovimientoInventarioRepository : IMovimientoInventarioRepository
{
    private readonly AdoPetsDbContext _ctx;
    public MovimientoInventarioRepository(AdoPetsDbContext ctx) => _ctx = ctx;


    public async Task AddRangeAsync(IEnumerable<MovimientoInventario> movimientos)
    {
        await _ctx.MovimientosInventario.AddRangeAsync(movimientos);
        await _ctx.SaveChangesAsync();
    }
}