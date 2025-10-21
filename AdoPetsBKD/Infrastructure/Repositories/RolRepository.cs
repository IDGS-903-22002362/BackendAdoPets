using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de roles
/// </summary>
public class RolRepository : IRolRepository
{
    private readonly AdoPetsDbContext _context;

    public RolRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Rol?> GetByIdAsync(Guid id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Rol?> GetByNameAsync(string nombre)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<List<Rol>> GetAllAsync()
    {
        return await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
    }

    public async Task<List<Rol>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Roles.Where(r => ids.Contains(r.Id)).ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Roles.AnyAsync(r => r.Id == id);
    }
}
