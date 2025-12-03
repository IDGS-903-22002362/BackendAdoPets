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
    public async Task<List<LoteInventario>> GetLotesDisponiblesByItemIdAsync(Guid itemId)
    {
        return await _ctx.LotesInventario
            .Where(l => l.ItemId == itemId && l.QtyDisponible > 0)
            .OrderBy(l => l.ExpDate ?? DateTime.MaxValue) // FIFO: primero los que vencen antes
            .ThenBy(l => l.CreatedAt) // Si no tienen fecha de vencimiento, por fecha de creación
            .ToListAsync();
    }
    public async Task UpdateAsync(LoteInventario lote)
    {
        _ctx.LotesInventario.Update(lote);
    }


    public async Task UpdateRangeAsync(IEnumerable<LoteInventario> lotes)
    {
        _ctx.LotesInventario.UpdateRange(lotes);
    }
    public async Task SaveChangesAsync()
    {
        await _ctx.SaveChangesAsync();
    }
}