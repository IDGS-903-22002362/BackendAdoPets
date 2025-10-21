using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de usuarios
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AdoPetsDbContext _context;

    public UsuarioRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, bool includeRoles = false)
    {
        var query = _context.Usuarios.AsQueryable();

        if (includeRoles)
        {
            query = query
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol);
        }

        return await query.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email, bool includeRoles = false)
    {
        var query = _context.Usuarios.AsQueryable();

        if (includeRoles)
        {
            query = query
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol);
        }

        return await query.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<List<Usuario>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool includeRoles = false)
    {
        var query = _context.Usuarios.AsQueryable();

        if (includeRoles)
        {
            query = query
                .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Usuarios.CountAsync();
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        return usuario;
    }

    public Task<Usuario> UpdateAsync(Usuario usuario)
    {
        usuario.UpdatedAt = DateTime.UtcNow;
        _context.Usuarios.Update(usuario);
        return Task.FromResult(usuario);
    }

    public async Task DeleteAsync(Guid id)
    {
        var usuario = await GetByIdAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Usuarios.AnyAsync(u => u.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        var query = _context.Usuarios.Where(u => u.Email.ToLower() == email.ToLower());

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
