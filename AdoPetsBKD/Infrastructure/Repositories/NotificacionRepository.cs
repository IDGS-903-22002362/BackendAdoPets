using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly AdoPetsDbContext _context;

    public NotificacionRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Notificacion?> GetByIdAsync(Guid id)
    {
        return await _context.Notificaciones
            .Include(n => n.Usuario)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<List<Notificacion>> GetByUsuarioIdAsync(Guid usuarioId, int page = 1, int pageSize = 50)
    {
        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid usuarioId)
    {
        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && n.ReadAt == null)
            .CountAsync();
    }

    public async Task AddAsync(Notificacion notificacion)
    {
        await _context.Notificaciones.AddAsync(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Update(notificacion);
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var notificacion = await GetByIdAsync(id);
        if (notificacion != null && notificacion.ReadAt == null)
        {
            notificacion.ReadAt = DateTime.UtcNow;
            await UpdateAsync(notificacion);
        }
    }

    public async Task MarkAllAsReadAsync(Guid usuarioId)
    {
        var notificaciones = await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId && n.ReadAt == null)
            .ToListAsync();

        foreach (var notificacion in notificaciones)
        {
            notificacion.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
