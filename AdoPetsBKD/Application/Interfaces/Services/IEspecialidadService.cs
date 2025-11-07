using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Especialidades; 

namespace AdoPetsBKD.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de especialidades
    /// </summary>
    public interface IEspecialidadService
    {
  
        Task<PagedResponse<EspecialidadListDto>> GetAllAsync(int pageNumber, int pageSize);

        Task<EspecialidadDetailDto?> GetByIdAsync(string codigo);

        Task<EspecialidadDetailDto> CreateAsync(CreateEspecialidadDto dto);
    }
}
