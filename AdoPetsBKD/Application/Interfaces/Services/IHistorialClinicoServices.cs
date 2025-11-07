using AdoPetsBKD.Application.DTOs.HistorialClinico;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IExpedienteService
{
    Task<ExpedienteDetailDto?> GetByIdAsync(Guid id);
    Task<List<ExpedienteListDto>> GetByMascotaAsync(Guid mascotaId);
    Task<List<ExpedienteListDto>> GetByVeterinarioAsync(Guid veterinarioId);
    Task<ExpedienteDetailDto> CreateAsync(CreateExpedienteDto dto, Guid userId);
    Task DeleteAsync(Guid id);
}

public interface IAdjuntoMedicoService
{
    Task<AdjuntoMedicoDto?> GetByIdAsync(Guid id);
    Task<List<AdjuntoMedicoDto>> GetByExpedienteAsync(Guid expedienteId);
    Task<AdjuntoMedicoDto> CreateAsync(CreateAdjuntoMedicoDto dto, Guid userId);
    Task DeleteAsync(Guid id);
}

public interface IVacunacionService
{
    Task<VacunacionDto?> GetByIdAsync(Guid id);
    Task<List<VacunacionDto>> GetByMascotaAsync(Guid mascotaId);
    Task<List<VacunacionDto>> GetUpcomingDueAsync(int days = 30);
    Task<VacunacionDto> CreateAsync(CreateVacunacionDto dto, Guid userId);
    Task DeleteAsync(Guid id);
}

public interface IDesparasitacionService
{
    Task<DesparasitacionDto?> GetByIdAsync(Guid id);
    Task<List<DesparasitacionDto>> GetByMascotaAsync(Guid mascotaId);
    Task<List<DesparasitacionDto>> GetUpcomingDueAsync(int days = 30);
    Task<DesparasitacionDto> CreateAsync(CreateDesparasitacionDto dto, Guid userId);
    Task DeleteAsync(Guid id);
}

public interface ICirugiaService
{
    Task<CirugiaDto?> GetByIdAsync(Guid id);
    Task<List<CirugiaDto>> GetByMascotaAsync(Guid mascotaId);
    Task<List<CirugiaDto>> GetByVeterinarioAsync(Guid veterinarioId);
    Task<CirugiaDto> CreateAsync(CreateCirugiaDto dto, Guid userId);
    Task DeleteAsync(Guid id);
}

public interface IValoracionService
{
    Task<ValoracionDto?> GetByIdAsync(Guid id);
    Task<List<ValoracionDto>> GetByMascotaAsync(Guid mascotaId);
    Task<ValoracionDto?> GetLatestByMascotaAsync(Guid mascotaId);
    Task<ValoracionDto> CreateAsync(CreateValoracionDto dto, Guid userId);
    Task DeleteAsync(Guid id);
}
