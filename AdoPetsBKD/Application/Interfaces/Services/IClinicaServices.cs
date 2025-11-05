using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface ICitaService
{
    Task<CitaDetailDto?> GetByIdAsync(Guid id);
    Task<List<CitaListDto>> GetAllAsync();
    Task<List<CitaListDto>> GetByVeterinarioAsync(Guid veterinarioId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CitaListDto>> GetByMascotaAsync(Guid mascotaId);
    Task<List<CitaListDto>> GetByPropietarioAsync(Guid propietarioId);
    Task<List<CitaListDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<CitaListDto>> GetByStatusAsync(StatusCita status);
    Task<CitaDetailDto> CreateAsync(CreateCitaDto dto, Guid userId);
    Task<CitaDetailDto> UpdateAsync(Guid id, UpdateCitaDto dto, Guid userId);
    Task<CitaDetailDto> CancelarAsync(Guid id, CancelarCitaDto dto, Guid userId);
    Task<CitaDetailDto> CompletarAsync(Guid id, CompletarCitaDto dto, Guid userId);
    Task DeleteAsync(Guid id);
    Task<DisponibilidadResponseDto> GetDisponibilidadAsync(DisponibilidadQueryDto query);
    Task<bool> HasOverlappingAppointmentAsync(Guid veterinarioId, DateTime startAt, DateTime endAt, Guid? excludeCitaId = null);
}

public interface ISalaService
{
    Task<SalaDetailDto?> GetByIdAsync(Guid id);
    Task<List<SalaListDto>> GetAllAsync();
    Task<List<SalaListDto>> GetActiveAsync();
    Task<SalaDetailDto> CreateAsync(CreateSalaDto dto, Guid userId);
    Task<SalaDetailDto> UpdateAsync(Guid id, UpdateSalaDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}
