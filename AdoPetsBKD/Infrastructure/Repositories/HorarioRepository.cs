using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de horarios
    /// </summary>
    public class HorarioRepository : IHorarioRepositoy
    {
        private readonly AdoPetsDbContext _context;

        public HorarioRepository(AdoPetsDbContext context)
        {
            _context = context;
        }

        public async Task<Horario> CreateAsync(Horario horario)
        {
            _context.Horarios.Add(horario);
            await _context.SaveChangesAsync();
            return horario;
        }

        public async Task<Horario> UpdateAsync(Horario horario)
        {
            _context.Horarios.Update(horario);
            await _context.SaveChangesAsync();
            return horario;
        }

        public async Task<Horario> DeleteAsync(Guid id)
        {
            var horario = await _context.Horarios.FindAsync(id);
            if (horario != null)
            {
                _context.Horarios.Remove(horario);
                await _context.SaveChangesAsync();
            }
            return horario!;
        }

        public async Task<Horario?> GetByIdAsync(Guid id)
        {
            return await _context.Horarios
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Usuario)
                        .ThenInclude(u => u.UsuarioRoles)
                            .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<List<Horario>> GetAllAsync(int pageNumber = 1, int pageSize = 10, DateTime? fechaInicio = null, DateTime? fechaFin = null, TipoHorario? tipo = null)
        {
            var query = _context.Horarios
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Usuario)
                        .ThenInclude(u => u.UsuarioRoles)
                            .ThenInclude(ur => ur.Rol)
                .AsQueryable();

            // Filtrar por tipo de horario si se proporciona
            if (tipo.HasValue)
            {
                query = query.Where(h => h.Tipo == tipo.Value);
            }

            // Filtrar por fechas si se proporcionan
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                query = query.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date >= fechaInicio.Value.Date && h.Fecha.Value.Date <= fechaFin.Value.Date) ||
                    (h.RangoInicio.HasValue && h.RangoFin.HasValue &&
                     ((h.RangoInicio.Value.Date <= fechaFin.Value.Date && h.RangoFin.Value.Date >= fechaInicio.Value.Date)))
                );
            }
            else if (fechaInicio.HasValue)
            {
                query = query.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date >= fechaInicio.Value.Date) ||
                    (h.RangoFin.HasValue && h.RangoFin.Value.Date >= fechaInicio.Value.Date)
                );
            }
            else if (fechaFin.HasValue)
            {
                query = query.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date <= fechaFin.Value.Date) ||
                    (h.RangoInicio.HasValue && h.RangoInicio.Value.Date <= fechaFin.Value.Date)
                );
            }

            return await query
                .OrderByDescending(h => h.Fecha ?? h.RangoInicio ?? DateTime.MinValue)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(DateTime? fechaInicio = null, DateTime? fechaFin = null, TipoHorario? tipo = null)
        {
            var query = _context.Horarios.AsQueryable();

            if (tipo.HasValue)
            {
                query = query.Where(h => h.Tipo == tipo.Value);
            }

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                query = query.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date >= fechaInicio.Value.Date && h.Fecha.Value.Date <= fechaFin.Value.Date) ||
                    (h.RangoInicio.HasValue && h.RangoFin.HasValue &&
                     ((h.RangoInicio.Value.Date <= fechaFin.Value.Date && h.RangoFin.Value.Date >= fechaInicio.Value.Date)))
                );
            }
            else if (fechaInicio.HasValue)
            {
                query = query.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date >= fechaInicio.Value.Date) ||
                    (h.RangoFin.HasValue && h.RangoFin.Value.Date >= fechaInicio.Value.Date)
                );
            }
            else if (fechaFin.HasValue)
            {
                query = query.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date <= fechaFin.Value.Date) ||
                    (h.RangoInicio.HasValue && h.RangoInicio.Value.Date <= fechaFin.Value.Date)
                );
            }

            return await query.CountAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Horarios.AnyAsync(h => h.Id == id);
        }

        public async Task<Horario?> GetHorarioEfectivoAsync(Guid empleadoId, DateTime fecha)
        {
            var horariosAplicables = await GetHorariosAplicablesAsync(empleadoId, fecha);

            if (!horariosAplicables.Any())
                return null;

            return horariosAplicables
                .OrderByDescending(h => h.ObtenerPrioridad())
                .ThenByDescending(h => h.Fecha.HasValue) 
                .FirstOrDefault();
        }

        public async Task<List<Horario>> GetHorariosAplicablesAsync(Guid empleadoId, DateTime fecha)
        {
            var fechaBuscada = fecha.Date;
            var diaSemana = fecha.DayOfWeek;

            return await _context.Horarios
                .Include(h => h.Empleado)
                    .ThenInclude(e => e.Usuario)
                        .ThenInclude(u => u.UsuarioRoles)
                            .ThenInclude(ur => ur.Rol)
                .Where(h => h.EmpleadoId == empleadoId &&
                    (
                        (h.Fecha.HasValue && h.Fecha.Value.Date == fechaBuscada) ||
                        
                        (h.RangoInicio.HasValue && h.RangoFin.HasValue && 
                         h.DiaSemana.HasValue && h.DiaSemana.Value == diaSemana &&
                         fechaBuscada >= h.RangoInicio.Value.Date && 
                         fechaBuscada <= h.RangoFin.Value.Date)
                    ))
                .ToListAsync();
        }

        public async Task<List<Horario>> GetConflictosAsync(
            Guid empleadoId, 
            DateTime? fecha, 
            DateTime? rangoInicio, 
            DateTime? rangoFin, 
            DayOfWeek? diaSemana,
            Guid? horarioIdExcluir = null)
        {
            var query = _context.Horarios.Where(h => h.EmpleadoId == empleadoId);

            if (horarioIdExcluir.HasValue)
            {
                query = query.Where(h => h.Id != horarioIdExcluir.Value);
            }

            var todosLosHorarios = await query.ToListAsync();

            var conflictos = new List<Horario>();

            if (fecha.HasValue)
            {
                var fechaBuscada = fecha.Value.Date;
                var diaSemanaFecha = fecha.Value.DayOfWeek;

                conflictos = todosLosHorarios.Where(h =>
                    (h.Fecha.HasValue && h.Fecha.Value.Date == fechaBuscada) ||
                    (h.RangoInicio.HasValue && h.RangoFin.HasValue &&
                     h.DiaSemana.HasValue && h.DiaSemana.Value == diaSemanaFecha &&
                     fechaBuscada >= h.RangoInicio.Value.Date &&
                     fechaBuscada <= h.RangoFin.Value.Date)
                ).ToList();
            }
            else if (rangoInicio.HasValue && rangoFin.HasValue && diaSemana.HasValue)
            {
                conflictos = todosLosHorarios.Where(h =>
                    (h.Fecha.HasValue && 
                     h.Fecha.Value.DayOfWeek == diaSemana.Value &&
                     h.Fecha.Value.Date >= rangoInicio.Value.Date &&
                     h.Fecha.Value.Date <= rangoFin.Value.Date) ||
                    (h.RangoInicio.HasValue && h.RangoFin.HasValue &&
                     h.DiaSemana.HasValue && h.DiaSemana.Value == diaSemana.Value &&
                     ((h.RangoInicio.Value.Date <= rangoFin.Value.Date && 
                       h.RangoFin.Value.Date >= rangoInicio.Value.Date)))
                ).ToList();
            }

            return conflictos;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
