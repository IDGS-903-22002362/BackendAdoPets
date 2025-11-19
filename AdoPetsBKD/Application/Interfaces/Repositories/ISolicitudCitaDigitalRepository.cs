using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Repositories;

public interface ISolicitudCitaDigitalRepository
{
    Task<SolicitudCitaDigital?> GetByIdAsync(Guid id);
    Task<List<SolicitudCitaDigital>> GetByUsuarioIdAsync(Guid usuarioId);
    Task<List<SolicitudCitaDigital>> GetPendientesAsync();
    Task<SolicitudCitaDigital?> GetByNumeroSolicitudAsync(string numeroSolicitud);
    Task AddAsync(SolicitudCitaDigital solicitud);
    Task UpdateAsync(SolicitudCitaDigital solicitud);
    Task<bool> ExistsAsync(Guid id);
}
