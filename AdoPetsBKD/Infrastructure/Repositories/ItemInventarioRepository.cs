using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class ItemInventarioRepository : IItemInventarioRepository
{
    private readonly AdoPetsDbContext _context;

    public ItemInventarioRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ItemInventario entity)
    {
        await _context.ItemsInventario.AddAsync(entity);
    }

    public async Task<ItemInventario?> GetByIdAsync(Guid id)
    {
        return await _context.ItemsInventario
            .Include(i => i.Lotes)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<ItemInventario>> GetAllAsync()
    {
        return await _context.ItemsInventario
            .Include(i => i.Lotes)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
