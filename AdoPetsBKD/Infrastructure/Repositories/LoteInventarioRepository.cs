using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace AdoPetsBKD.Infrastructure.Repositories;


public class LoteInventarioRepository : ILoteInventarioRepository
{
    private readonly AdoPetsDbContext _ctx;
    public LoteInventarioRepository(AdoPetsDbContext ctx) => _ctx = ctx;


    public async Task AddRangeAsync(IEnumerable<LoteInventario> lotes)
    {
        await _ctx.LotesInventario.AddRangeAsync(lotes);
        await _ctx.SaveChangesAsync();
    }
    public async Task<IEnumerable<LoteInventario>> GetAllAsync()
    {
        return await _ctx.LotesInventario.ToListAsync();
    }
}