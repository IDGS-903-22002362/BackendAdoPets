using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface INotificacionRepository
{
    Task<Notificacion?> GetByIdAsync(Guid id);
    Task<List<Notificacion>> GetByUsuarioIdAsync(Guid usuarioId, int page = 1, int pageSize = 50);
    Task<int> GetUnreadCountAsync(Guid usuarioId);
    Task AddAsync(Notificacion notificacion);
    Task UpdateAsync(Notificacion notificacion);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync(Guid usuarioId);
}
