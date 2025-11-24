using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace AdoPetsBKD.Infrastructure.Repositories;


public class CompraRepository : ICompraRepository
{
    private readonly AdoPetsDbContext _context;

    public CompraRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Compra compra)
    {
        await _context.Compras.AddAsync(compra);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Compra compra)
    {
        _context.Compras.Update(compra);
        await _context.SaveChangesAsync();
    }

    public async Task<Compra?> GetByIdAsync(Guid id)
    {
        return await _context.Compras.FindAsync(id);
    }
}
