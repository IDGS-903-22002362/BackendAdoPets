using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

public class SolicitudCitaDigitalRepository : ISolicitudCitaDigitalRepository
{
    private readonly AdoPetsDbContext _context;

    public SolicitudCitaDigitalRepository(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitudCitaDigital?> GetByIdAsync(Guid id)
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Mascota)
            .Include(s => s.Servicio)
            .Include(s => s.VeterinarioPreferido)
            .Include(s => s.SalaPreferida)
            .Include(s => s.RevisadoPor)
            .Include(s => s.PagoAnticipo)
            .Include(s => s.Cita)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<SolicitudCitaDigital>> GetByUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Mascota)
            .Include(s => s.Servicio)
            .Include(s => s.PagoAnticipo)
            .Include(s => s.Cita)
            .Where(s => s.SolicitanteId == usuarioId)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();
    }

    public async Task<List<SolicitudCitaDigital>> GetPendientesAsync()
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Mascota)
            .Include(s => s.Servicio)
            .Include(s => s.VeterinarioPreferido)
            .Include(s => s.PagoAnticipo)
            .Where(s => s.Estado == EstadoSolicitudCita.PagadaPendienteConfirmacion 
                     || s.Estado == EstadoSolicitudCita.PendientePago
                     || s.Estado == EstadoSolicitudCita.EnRevision)
            .OrderBy(s => s.FechaSolicitud)
            .ToListAsync();
    }

    public async Task<SolicitudCitaDigital?> GetByNumeroSolicitudAsync(string numeroSolicitud)
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Mascota)
            .Include(s => s.Servicio)
            .FirstOrDefaultAsync(s => s.NumeroSolicitud == numeroSolicitud);
    }

    public async Task AddAsync(SolicitudCitaDigital solicitud)
    {
        await _context.SolicitudesCitasDigitales.AddAsync(solicitud);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SolicitudCitaDigital solicitud)
    {
        _context.SolicitudesCitasDigitales.Update(solicitud);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.SolicitudesCitasDigitales.AnyAsync(s => s.Id == id);
    }
}
