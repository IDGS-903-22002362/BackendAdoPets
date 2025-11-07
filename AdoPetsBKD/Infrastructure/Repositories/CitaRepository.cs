using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class CitaRepository : ICitaRepository
{
    private readonly AdoPetsDbContext _context;

    public CitaRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Cita?> GetByIdAsync(Guid id)
    {
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Include(c => c.Recordatorios)
            .Include(c => c.HistorialEstados)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cita>> GetAllAsync()
    {
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetByVeterinarioAsync(Guid veterinarioId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.VeterinarioId == veterinarioId);

        if (startDate.HasValue)
            query = query.Where(c => c.StartAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.EndAt <= endDate.Value);

        return await query
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetByMascotaAsync(Guid mascotaId)
    {
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.MascotaId == mascotaId)
            .OrderByDescending(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetByPropietarioAsync(Guid propietarioId)
    {
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.PropietarioId == propietarioId)
            .OrderByDescending(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.StartAt >= startDate && c.EndAt <= endDate)
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetByStatusAsync(StatusCita status)
    {
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.Status == status)
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetBySalaAsync(Guid salaId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.SalaId == salaId);

        if (startDate.HasValue)
            query = query.Where(c => c.StartAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.EndAt <= endDate.Value);

        return await query
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetUpcomingByVeterinarioAsync(Guid veterinarioId, int days)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Where(c => c.VeterinarioId == veterinarioId)
            .Where(c => c.StartAt >= DateTime.UtcNow && c.StartAt <= endDate)
            .Where(c => c.Status == StatusCita.Programada)
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task<List<Cita>> GetPendingRemindersAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Citas
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Recordatorios)
            .Where(c => c.Status == StatusCita.Programada)
            .Where(c => c.StartAt > now)
            .Where(c => c.Recordatorios.Any(r => !r.WasSent))
            .OrderBy(c => c.StartAt)
            .ToListAsync();
    }

    public async Task AddAsync(Cita cita)
    {
        await _context.Citas.AddAsync(cita);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cita cita)
    {
        _context.Citas.Update(cita);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Cita cita)
    {
        _context.Citas.Remove(cita);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasOverlappingAppointmentAsync(Guid veterinarioId, DateTime startAt, DateTime endAt, Guid? excludeCitaId = null)
    {
        var query = _context.Citas
            .Where(c => c.VeterinarioId == veterinarioId)
            .Where(c => c.Status == StatusCita.Programada)
            .Where(c => c.StartAt < endAt && c.EndAt > startAt);

        if (excludeCitaId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCitaId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> HasSalaOverlappingAsync(Guid salaId, DateTime startAt, DateTime endAt, Guid? excludeCitaId = null)
    {
        var query = _context.Citas
            .Where(c => c.SalaId == salaId)
            .Where(c => c.Status == StatusCita.Programada)
            .Where(c => c.StartAt < endAt && c.EndAt > startAt);

        if (excludeCitaId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCitaId.Value);
        }

        return await query.AnyAsync();
    }
}
