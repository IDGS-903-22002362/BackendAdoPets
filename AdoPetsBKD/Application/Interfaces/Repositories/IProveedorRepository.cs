using AdoPetsBKD.Domain.Entities.Inventario;
using System.Linq.Expressions;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

// Repositorio dedicado a las operaciones de persistencia de la entidad Proveedor.
public interface IProveedorRepository
{
    // === Mtodos de Persistencia ===

    /// <summary>
    /// Agrega una nueva entidad Proveedor al contexto de datos.
    /// </summary>
    Task CreateAsync(Proveedor proveedor);

    /// <summary>
    /// Marca una entidad Proveedor como modificada en el contexto de datos.
    /// </summary>
    Task UpdateAsync(Proveedor proveedor);

    /// <summary>
    /// Marca una entidad Proveedor para su eliminacin fsica del contexto. 
    /// En este sistema se recomienda usar borrado lgico (cambiar estatus).
    /// </summary>
    Task DeleteAsync(Proveedor proveedor);

    /// <summary>
    /// Guarda todos los cambios pendientes en la base de datos de forma asncrona.
    /// </summary>
    Task SaveChangesAsync();

    // === Mtodos de Consulta ===

    /// <summary>
    /// Obtiene un Proveedor por su identificador nico (Guid).
    /// </summary>
    Task<Proveedor?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtiene todos los Proveedores del sistema.
    /// </summary>
    Task<IEnumerable<Proveedor>> GetAllAsync();

    /// <summary>
    /// Permite buscar Proveedores aplicando una condicin de filtro.
    /// </summary>
    Task<IEnumerable<Proveedor>> FindAsync(Expression<Func<Proveedor, bool>> predicate);
}