using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface IDispositivoRepository
{
    Task<List<Dispositivo>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<Dispositivo?> GetByTokenAsync(string token);
    Task AddAsync(Dispositivo dispositivo);
    Task UpdateAsync(Dispositivo dispositivo);
    Task DeleteAsync(Guid id);
}
