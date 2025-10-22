using AdoPetsBKD.Application.DTOs.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface ISolicitudCitaDigitalService
{
    Task<SolicitudCitaDigitalDto> CreateSolicitudAsync(CreateSolicitudCitaDigitalDto dto, Guid createdBy);
    Task<SolicitudCitaDigitalDto?> GetSolicitudByIdAsync(Guid id);
    Task<List<SolicitudCitaDigitalDto>> GetSolicitudesByUsuarioAsync(Guid usuarioId);
    Task<List<SolicitudCitaDigitalDto>> GetSolicitudesPendientesAsync();
    Task<DisponibilidadResponseDto> VerificarDisponibilidadAsync(VerificarDisponibilidadDto dto);
    Task<SolicitudCitaDigitalDto> MarcarComoEnRevisionAsync(Guid solicitudId, Guid revisadoPorId);
    Task<SolicitudCitaDigitalDto> ConfirmarSolicitudAsync(ConfirmarSolicitudCitaDto dto);
    Task<SolicitudCitaDigitalDto> RechazarSolicitudAsync(RechazarSolicitudCitaDto dto);
    Task<SolicitudCitaDigitalDto> CancelarSolicitudAsync(Guid solicitudId, Guid usuarioId);
}
