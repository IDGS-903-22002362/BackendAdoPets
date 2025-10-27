using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de empleados
    /// </summary>
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly AdoPetsDbContext _context;

        public EmpleadoRepository(AdoPetsDbContext context)
        {
            _context = context;
        }

        public async Task<Empleado> CreateAsync(Empleado empleado)
        {
            _context.Empleados.Add(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> UpdateAsync(Empleado empleado)
        {
            _context.Empleados.Update(empleado);
            await _context.SaveChangesAsync();
            return empleado;
        }

        public async Task<Empleado> DeleteAsync(Guid id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);
                await _context.SaveChangesAsync();
            }
            return empleado!;
        }

        public async Task<Empleado?> GetByIdAsync(Guid id)
        {
            return await _context.Empleados
                         .Include(e => e.Usuario)      
                         .ThenInclude(u => u.UsuarioRoles) 
                         .ThenInclude(ur => ur.Rol)
                         .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<Empleado>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false)
        {
            var query = _context.Empleados
                .Include(e => e.Usuario)
                .ThenInclude(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .AsQueryable();

            if (!includeInactive)
            {
                // por defecto excluimos empleados inactivos
                query = query.Where(e => e.Activo);
            }

            return await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync(bool includeInactive = false)
        {
            if (includeInactive)
                return await _context.Empleados.CountAsync();

            return await _context.Empleados.CountAsync(e => e.Activo);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Empleados.AnyAsync(e => e.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
