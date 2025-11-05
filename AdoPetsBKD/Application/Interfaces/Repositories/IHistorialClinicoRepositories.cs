using AdoPetsBKD.Application.DTOs.HistorialClinico;
using AdoPetsBKD.Domain.Entities.HistorialClinico;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface IExpedienteRepository
{
    Task<Expediente?> GetByIdAsync(Guid id);
    Task<List<Expediente>> GetByMascotaAsync(Guid mascotaId);
    Task<List<Expediente>> GetByVeterinarioAsync(Guid veterinarioId);
    Task<Expediente?> GetByCitaAsync(Guid citaId);
    Task AddAsync(Expediente expediente);
    Task UpdateAsync(Expediente expediente);
    Task DeleteAsync(Expediente expediente);
}

public interface IAdjuntoMedicoRepository
{
    Task<AdjuntoMedico?> GetByIdAsync(Guid id);
    Task<List<AdjuntoMedico>> GetByExpedienteAsync(Guid expedienteId);
    Task AddAsync(AdjuntoMedico adjunto);
    Task DeleteAsync(AdjuntoMedico adjunto);
}

public interface IVacunacionRepository
{
    Task<Vacunacion?> GetByIdAsync(Guid id);
    Task<List<Vacunacion>> GetByMascotaAsync(Guid mascotaId);
    Task<List<Vacunacion>> GetUpcomingDueAsync(int days = 30);
    Task AddAsync(Vacunacion vacunacion);
    Task UpdateAsync(Vacunacion vacunacion);
    Task DeleteAsync(Vacunacion vacunacion);
}

public interface IDesparasitacionRepository
{
    Task<Desparasitacion?> GetByIdAsync(Guid id);
    Task<List<Desparasitacion>> GetByMascotaAsync(Guid mascotaId);
    Task<List<Desparasitacion>> GetUpcomingDueAsync(int days = 30);
    Task AddAsync(Desparasitacion desparasitacion);
    Task UpdateAsync(Desparasitacion desparasitacion);
    Task DeleteAsync(Desparasitacion desparasitacion);
}

public interface ICirugiaRepository
{
    Task<Cirugia?> GetByIdAsync(Guid id);
    Task<List<Cirugia>> GetByMascotaAsync(Guid mascotaId);
    Task<List<Cirugia>> GetByVeterinarioAsync(Guid veterinarioId);
    Task AddAsync(Cirugia cirugia);
    Task UpdateAsync(Cirugia cirugia);
    Task DeleteAsync(Cirugia cirugia);
}

public interface IValoracionRepository
{
    Task<Valoracion?> GetByIdAsync(Guid id);
    Task<List<Valoracion>> GetByMascotaAsync(Guid mascotaId);
    Task<Valoracion?> GetLatestByMascotaAsync(Guid mascotaId);
    Task AddAsync(Valoracion valoracion);
    Task UpdateAsync(Valoracion valoracion);
    Task DeleteAsync(Valoracion valoracion);
}
