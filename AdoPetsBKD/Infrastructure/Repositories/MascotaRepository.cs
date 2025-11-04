using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories;

    // Implementacion del repositorio de mascotas
    public class MascotaRepository : IUMascotaRepositoty
    {
        private readonly AdoPetsDbContext _context;
        public MascotaRepository(AdoPetsDbContext context)
        {
            _context = context;
        }

    public async Task<Mascota?> GetByIdAsync(Guid id, bool includeFotos = true)
        {
            var query = _context.Mascotas.AsQueryable();
            if (includeFotos)
            {
                query = query
                    .Include(m => m.Fotos);
            }
            return await query.FirstOrDefaultAsync(m => m.Id == id);
    }


 public async Task<IEnumerable<Mascota>> GetAllAsync(bool includeFotos = true)
{
    IQueryable<Mascota> query = _context.Mascotas;

    if (includeFotos)
    {
        query = query.Include(m => m.Fotos);
    }

    return await query.ToListAsync();
}

    public async Task<Mascota> CreateAsync(Mascota mascota)
        {
            await _context.Mascotas.AddAsync(mascota);
            return mascota;
        }

    public async Task<Mascota> UpdateAsync(Mascota mascota)
        {
            mascota.UpdatedAt = DateTime.UtcNow;
            _context.Mascotas.Update(mascota);
            return mascota;
    }



    public async Task<Mascota> DeleteAsync(Guid id)
    {
        var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id);
        if (mascota == null)
            throw new InvalidOperationException("La mascota no existe");

        mascota.Estatus = EstatusMascota.NoAdoptable;
        mascota.UpdatedAt = DateTime.UtcNow;

        _context.Mascotas.Update(mascota);
        return mascota;
    }

    // Agregar foto a una mascota

    public async Task<IEnumerable<MascotaFoto>> AddPhotoAsync(IEnumerable<MascotaFoto> foto)
        {
            await _context.MascotasFotos.AddRangeAsync(foto);
        return foto;
    }
  


    public async Task<MascotaFoto> DeletePhotoAsync(Guid id)
    {
        var foto = await _context.MascotasFotos.FirstOrDefaultAsync(f => f.Id == id);
        if (foto == null)
            throw new InvalidOperationException("La foto no existe");

        _context.MascotasFotos.Remove(foto);
        return foto;
    }

    // Solicitud de adopcion
    public async Task<SolicitudAdopcion> CreateSolicitudAdopcionAsync(SolicitudAdopcion solicitud)
    {
        await _context.SolicitudesAdopcion.AddAsync(solicitud);
        return solicitud;
    }

    public async Task<SolicitudAdopcion?> GetSolicitudByIdAsync(Guid solicitudId)
    {
        return await _context.SolicitudesAdopcion
            .Include(s => s.Usuario)   
            .Include(s => s.Mascota)
            .ThenInclude(m => m.Fotos.OrderBy(f => f.Orden))
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == solicitudId);
    }

    public async Task<IEnumerable<SolicitudAdopcion>> GetAllSolicitudesAdopcionAsync()
    {
        return await _context.SolicitudesAdopcion
            .Include(s => s.Usuario)
            .Include(s => s.Mascota)
            .ThenInclude(m => m.Fotos.OrderBy(f => f.Orden))
            .Include(s => s.Logs.OrderByDescending(l => l.ChangedAt))
            .ToListAsync();
    }

    public async Task<SolicitudAdopcion> UpdateStatusSolicitudAdopcionAsync(SolicitudAdopcion solicitud)
    {
        solicitud.UpdatedAt = DateTime.UtcNow;
        _context.SolicitudesAdopcion.Update(solicitud);
        return solicitud;
    }
    
    public async Task<SolicitudAdopcion> UpdateSolicitudAdopcionAceptadaAsync(SolicitudAdopcion solicitud)
    {
        solicitud.UpdatedAt = DateTime.UtcNow;
        _context.SolicitudesAdopcion.Update(solicitud);
        return solicitud;
    }


    public async Task<SolicitudAdopcion> UpdateSolicitudAdopcionRechazadaAsync(SolicitudAdopcion solicitud)
    {
        solicitud.UpdatedAt = DateTime.UtcNow;
        _context.SolicitudesAdopcion.Update(solicitud);
        return solicitud;
    }

    public async Task<SolicitudAdopcion> UpdateSolicitudAdopcionCanceladaAsync(SolicitudAdopcion solicitud)
    {
        solicitud.UpdatedAt = DateTime.UtcNow;
        _context.SolicitudesAdopcion.Update(solicitud);
        return solicitud;
    }

    public async Task<IEnumerable<SolicitudAdopcion>> GetSolicitudbyUsuarioIdAsync(Guid usuarioId)
    {
        return await _context.SolicitudesAdopcion
            .Include(s => s.Usuario)
            .Include(s => s.Mascota)
            .Include(s => s.Logs)
            .Where(s => s.UsuarioId == usuarioId)
            .ToListAsync();
    }
    public async Task<AdopcionLog> AddAdopcionLogAsync(AdopcionLog log)
    {
        await _context.AdopcionLogs.AddAsync(log);
        return log;
    }

    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }


}

