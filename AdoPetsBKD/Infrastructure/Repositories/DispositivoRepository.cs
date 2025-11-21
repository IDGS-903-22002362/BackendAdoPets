using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class DispositivoRepository : IDispositivoRepository
{
    private readonly AdoPetsDbContext _context;

    public DispositivoRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<Dispositivo>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.Dispositivos
            .Where(d => d.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<Dispositivo?> GetByTokenAsync(string token)
    {
        return await _context.Dispositivos
            .FirstOrDefaultAsync(d => d.Token == token);
    }

    public async Task AddAsync(Dispositivo dispositivo)
    {
        await _context.Dispositivos.AddAsync(dispositivo);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Dispositivo dispositivo)
    {
        _context.Dispositivos.Update(dispositivo);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var dispositivo = await _context.Dispositivos.FindAsync(id);
        if (dispositivo != null)
        {
            _context.Dispositivos.Remove(dispositivo);
            await _context.SaveChangesAsync();
        }
    }
}
