using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AdoPetsBKD.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio para la entidad Proveedor.
    /// Gestiona las operaciones CRUD y consultas personalizadas en base de datos.
    /// </summary>
    public class ProveedorRepository : IProveedorRepository
    {
        private readonly AdoPetsDbContext _context;

        public ProveedorRepository(AdoPetsDbContext context)
        {
            _context = context;
        }

        // 🟢 Crear nuevo proveedor
        public async Task CreateAsync(Proveedor proveedor)
        {
            await _context.Proveedores.AddAsync(proveedor);
        }

        // 🟠 Actualizar proveedor existente
        public async Task UpdateAsync(Proveedor proveedor)
        {
            proveedor.UpdatedAt = DateTime.UtcNow;
            _context.Proveedores.Update(proveedor);
            await Task.CompletedTask; // para cumplir con la firma async
        }

        // 🔴 Eliminar proveedor (físicamente, aunque normalmente se usa borrado lógico)
        public async Task DeleteAsync(Proveedor proveedor)
        {
            _context.Proveedores.Remove(proveedor);
            await Task.CompletedTask;
        }

        // 💾 Guardar cambios en la base de datos
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // 🔍 Obtener proveedor por ID
        public async Task<Proveedor?> GetByIdAsync(Guid id)
        {
            return await _context.Proveedores.FirstOrDefaultAsync(p => p.Id == id);
        }

        // 📋 Obtener todos los proveedores
        public async Task<IEnumerable<Proveedor>> GetAllAsync()
        {
            return await _context.Proveedores
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        // 🔎 Buscar proveedores por condición
        public async Task<IEnumerable<Proveedor>> FindAsync(Expression<Func<Proveedor, bool>> predicate)
        {
            return await _context.Proveedores
                .Where(predicate)
                .ToListAsync();
        }
    }
}
