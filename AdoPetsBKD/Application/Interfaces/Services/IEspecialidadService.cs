using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Especialidades; 

namespace AdoPetsBKD.Application.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de especialidades
    /// </summary>
    public interface IEspecialidadService
    {
        /// <summary>
        /// Obtiene todas las especialidades de forma paginada
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de especialidades</returns>
        Task<PagedResponse<EspecialidadListDto>> GetAllAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Obtiene una especialidad por su código
        /// </summary>
        /// <param name="codigo">Código de la especialidad</param>
        /// <returns>Detalle de la especialidad</returns>
        Task<EspecialidadDetailDto?> GetByIdAsync(string codigo);

        /// <summary>
        /// Crea una nueva especialidad
        /// </summary>
        /// <param name="dto">Datos de la especialidad a crear</param>
        /// <returns>Detalle de la especialidad creada</returns>
        Task<EspecialidadDetailDto> CreateAsync(CreateEspecialidadDto dto);
    }
}
