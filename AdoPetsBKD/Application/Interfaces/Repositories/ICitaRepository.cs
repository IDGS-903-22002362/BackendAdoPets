using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface ICitaRepository
{
    Task<Cita?> GetByIdAsync(Guid id);
    Task<List<Cita>> GetAllAsync();
    Task<List<Cita>> GetByVeterinarioAsync(Guid veterinarioId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<Cita>> GetByMascotaAsync(Guid mascotaId);
    Task<List<Cita>> GetByPropietarioAsync(Guid propietarioId);
    Task<List<Cita>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<Cita>> GetByStatusAsync(StatusCita status);
    Task<List<Cita>> GetBySalaAsync(Guid salaId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<Cita>> GetUpcomingByVeterinarioAsync(Guid veterinarioId, int days);
    Task<List<Cita>> GetPendingRemindersAsync();
    Task AddAsync(Cita cita);
    Task UpdateAsync(Cita cita);
    Task DeleteAsync(Cita cita);
    Task<bool> HasOverlappingAppointmentAsync(Guid veterinarioId, DateTime startAt, DateTime endAt, Guid? excludeCitaId = null);
    Task<bool> HasSalaOverlappingAsync(Guid salaId, DateTime startAt, DateTime endAt, Guid? excludeCitaId = null);
}
