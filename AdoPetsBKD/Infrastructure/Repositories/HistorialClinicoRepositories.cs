using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.HistorialClinico;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class ExpedienteRepository : IExpedienteRepository
{
    private readonly AdoPetsDbContext _context;

    public ExpedienteRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Expediente?> GetByIdAsync(Guid id)
    {
        return await _context.Expedientes
            .Include(e => e.Mascota)
            .Include(e => e.Adjuntos)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Expediente>> GetByMascotaAsync(Guid mascotaId)
    {
        return await _context.Expedientes
            .Include(e => e.Mascota)
            .Include(e => e.Adjuntos)
            .Where(e => e.MascotaId == mascotaId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
    }

    public async Task<List<Expediente>> GetByVeterinarioAsync(Guid veterinarioId)
    {
        return await _context.Expedientes
            .Include(e => e.Mascota)
            .Include(e => e.Adjuntos)
            .Where(e => e.VeterinarioId == veterinarioId)
            .OrderByDescending(e => e.Fecha)
            .ToListAsync();
    }

    public async Task<Expediente?> GetByCitaAsync(Guid citaId)
    {
        return await _context.Expedientes
            .Include(e => e.Mascota)
            .Include(e => e.Adjuntos)
            .FirstOrDefaultAsync(e => e.CitaId == citaId);
    }

    public async Task AddAsync(Expediente expediente)
    {
        await _context.Expedientes.AddAsync(expediente);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Expediente expediente)
    {
        _context.Expedientes.Update(expediente);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Expediente expediente)
    {
        _context.Expedientes.Remove(expediente);
        await _context.SaveChangesAsync();
    }
}

public class AdjuntoMedicoRepository : IAdjuntoMedicoRepository
{
    private readonly AdoPetsDbContext _context;

    public AdjuntoMedicoRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<AdjuntoMedico?> GetByIdAsync(Guid id)
    {
        return await _context.AdjuntosMedicos
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AdjuntoMedico>> GetByExpedienteAsync(Guid expedienteId)
    {
        return await _context.AdjuntosMedicos
            .Where(a => a.EntryType == TipoEntryMedico.Expediente && a.EntryId == expedienteId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task AddAsync(AdjuntoMedico adjunto)
    {
        await _context.AdjuntosMedicos.AddAsync(adjunto);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AdjuntoMedico adjunto)
    {
        _context.AdjuntosMedicos.Remove(adjunto);
        await _context.SaveChangesAsync();
    }
}

public class VacunacionRepository : IVacunacionRepository
{
    private readonly AdoPetsDbContext _context;

    public VacunacionRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Vacunacion?> GetByIdAsync(Guid id)
    {
        return await _context.Vacunaciones
            .Include(v => v.Mascota)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Vacunacion>> GetByMascotaAsync(Guid mascotaId)
    {
        return await _context.Vacunaciones
            .Include(v => v.Mascota)
            .Where(v => v.MascotaId == mascotaId)
            .OrderByDescending(v => v.AppliedAt)
            .ToListAsync();
    }

    public async Task<List<Vacunacion>> GetUpcomingDueAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);
        return await _context.Vacunaciones
            .Include(v => v.Mascota)
            .Where(v => v.NextDueAt.HasValue && v.NextDueAt.Value <= cutoffDate && v.NextDueAt.Value >= DateTime.UtcNow)
            .OrderBy(v => v.NextDueAt)
            .ToListAsync();
    }

    public async Task AddAsync(Vacunacion vacunacion)
    {
        await _context.Vacunaciones.AddAsync(vacunacion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Vacunacion vacunacion)
    {
        _context.Vacunaciones.Update(vacunacion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Vacunacion vacunacion)
    {
        _context.Vacunaciones.Remove(vacunacion);
        await _context.SaveChangesAsync();
    }
}

public class DesparasitacionRepository : IDesparasitacionRepository
{
    private readonly AdoPetsDbContext _context;

    public DesparasitacionRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Desparasitacion?> GetByIdAsync(Guid id)
    {
        return await _context.Desparasitaciones
            .Include(d => d.Mascota)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Desparasitacion>> GetByMascotaAsync(Guid mascotaId)
    {
        return await _context.Desparasitaciones
            .Include(d => d.Mascota)
            .Where(d => d.MascotaId == mascotaId)
            .OrderByDescending(d => d.AppliedAt)
            .ToListAsync();
    }

    public async Task<List<Desparasitacion>> GetUpcomingDueAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(days);
        return await _context.Desparasitaciones
            .Include(d => d.Mascota)
            .Where(d => d.NextDueAt.HasValue && d.NextDueAt.Value <= cutoffDate && d.NextDueAt.Value >= DateTime.UtcNow)
            .OrderBy(d => d.NextDueAt)
            .ToListAsync();
    }

    public async Task AddAsync(Desparasitacion desparasitacion)
    {
        await _context.Desparasitaciones.AddAsync(desparasitacion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Desparasitacion desparasitacion)
    {
        _context.Desparasitaciones.Update(desparasitacion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Desparasitacion desparasitacion)
    {
        _context.Desparasitaciones.Remove(desparasitacion);
        await _context.SaveChangesAsync();
    }
}

public class CirugiaRepository : ICirugiaRepository
{
    private readonly AdoPetsDbContext _context;

    public CirugiaRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Cirugia?> GetByIdAsync(Guid id)
    {
        return await _context.Cirugias
            .Include(c => c.Mascota)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Cirugia>> GetByMascotaAsync(Guid mascotaId)
    {
        return await _context.Cirugias
            .Include(c => c.Mascota)
            .Where(c => c.MascotaId == mascotaId)
            .OrderByDescending(c => c.PerformedAt)
            .ToListAsync();
    }

    public async Task<List<Cirugia>> GetByVeterinarioAsync(Guid veterinarioId)
    {
        return await _context.Cirugias
            .Include(c => c.Mascota)
            .Where(c => c.VeterinarioId == veterinarioId)
            .OrderByDescending(c => c.PerformedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Cirugia cirugia)
    {
        await _context.Cirugias.AddAsync(cirugia);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cirugia cirugia)
    {
        _context.Cirugias.Update(cirugia);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Cirugia cirugia)
    {
        _context.Cirugias.Remove(cirugia);
        await _context.SaveChangesAsync();
    }
}

public class ValoracionRepository : IValoracionRepository
{
    private readonly AdoPetsDbContext _context;

    public ValoracionRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<Valoracion?> GetByIdAsync(Guid id)
    {
        return await _context.Valoraciones
            .Include(v => v.Mascota)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Valoracion>> GetByMascotaAsync(Guid mascotaId)
    {
        return await _context.Valoraciones
            .Include(v => v.Mascota)
            .Where(v => v.MascotaId == mascotaId)
            .OrderByDescending(v => v.TakenAt)
            .ToListAsync();
    }

    public async Task<Valoracion?> GetLatestByMascotaAsync(Guid mascotaId)
    {
        return await _context.Valoraciones
            .Include(v => v.Mascota)
            .Where(v => v.MascotaId == mascotaId)
            .OrderByDescending(v => v.TakenAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Valoracion valoracion)
    {
        await _context.Valoraciones.AddAsync(valoracion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Valoracion valoracion)
    {
        _context.Valoraciones.Update(valoracion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Valoracion valoracion)
    {
        _context.Valoraciones.Remove(valoracion);
        await _context.SaveChangesAsync();
    }
}
