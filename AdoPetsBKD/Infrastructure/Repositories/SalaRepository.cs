using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class SalaRepository : ISalaRepository
{
    private readonly AdoPetsDbContext _context;

    public SalaRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Sala?> GetByIdAsync(Guid id)
    {
        return await _context.Salas
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sala?> GetByNombreAsync(string nombre)
    {
        return await _context.Salas
            .FirstOrDefaultAsync(s => s.Nombre == nombre);
    }

    public async Task<List<Sala>> GetAllAsync()
    {
        return await _context.Salas
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task<List<Sala>> GetActiveAsync()
    {
        return await _context.Salas
            .Where(s => s.Activa)
            .OrderBy(s => s.Nombre)
            .ToListAsync();
    }

    public async Task AddAsync(Sala sala)
    {
        await _context.Salas.AddAsync(sala);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Sala sala)
    {
        _context.Salas.Update(sala);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Sala sala)
    {
        _context.Salas.Remove(sala);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNombreAsync(string nombre, Guid? excludeId = null)
    {
        var query = _context.Salas
            .Where(s => s.Nombre == nombre);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
