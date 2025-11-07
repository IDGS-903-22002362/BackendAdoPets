using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de empleados
    /// </summary>
    public interface IEmpleadoRepository
    {
        Task<Empleado?> GetByIdAsync(Guid id);
        Task<Empleado?> GetByIdWithEspecialidadesAsync(Guid id);
        Task<Empleado> CreateAsync(Empleado empleado);
        Task<Empleado> UpdateAsync(Empleado empleado);
        Task<Empleado> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);

        Task<int> GetTotalCountAsync(bool includeInactive = false);
        Task<List<Empleado>> GetAllAsync(int pageNumber = 1, int pageSize = 10, bool includeInactive = false);

        Task SaveChangesAsync();
    }
}
