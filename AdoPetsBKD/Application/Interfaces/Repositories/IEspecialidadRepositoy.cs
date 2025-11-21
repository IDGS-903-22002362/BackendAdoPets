using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Application.Interfaces.Repositories
{
    public interface IEspecialidadRepositoy
    {
        Task<Especialidad?> CreateAsync (Especialidad especialidad);
        Task<Especialidad> GetByIdAsync(string codigo);
        Task<List<Especialidad>> GetAllAsync();
        Task SaveChangesAsync();
    }
}
