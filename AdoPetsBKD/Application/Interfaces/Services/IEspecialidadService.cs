using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Especialidades; 

namespace AdoPetsBKD.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de especialidades
    /// </summary>
    public interface IEspecialidadService
    {
  
        Task<List<EspecialidadListDto>> GetAllAsync();

        Task<EspecialidadDetailDto?> GetByIdAsync(string codigo);

        Task<EspecialidadDetailDto> CreateAsync(CreateEspecialidadDto dto);
    }
}
