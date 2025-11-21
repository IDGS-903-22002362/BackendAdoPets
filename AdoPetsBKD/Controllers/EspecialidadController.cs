using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.DTOs.Especialidades;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Controllers
{
    /// <summary>
    /// Controlador para gestionar especialidades
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class EspecialidadController : ControllerBase
    {
        private readonly IEspecialidadService _especialidadService;
        private readonly ILogger<EspecialidadController> _logger;

        public EspecialidadController(IEspecialidadService especialidadService, ILogger<EspecialidadController> logger)
        {
            _especialidadService = especialidadService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todas las especialidades
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<List<EspecialidadListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _especialidadService.GetAllAsync();
                return Ok(new ApiResponse<List<EspecialidadListDto>>
                {
                    Success = true,
                    Message = "Especialidades obtenidas correctamente",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de especialidades");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<EspecialidadListDto>>
                {
                    Success = false,
                    Message = "Error al obtener la lista de especialidades: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener una especialidad por su código
        /// </summary>
        [HttpGet("{codigo}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EspecialidadDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(string codigo)
        {
            try
            {
                var especialidad = await _especialidadService.GetByIdAsync(codigo);
                
                if (especialidad == null)
                {
                    return NotFound(new ApiResponse<EspecialidadDetailDto>
                    {
                        Success = false,
                        Message = "Especialidad no encontrada"
                    });
                }

                return Ok(new ApiResponse<EspecialidadDetailDto>
                {
                    Success = true,
                    Message = "Especialidad encontrada",
                    Data = especialidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la especialidad {Codigo}", codigo);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<EspecialidadDetailDto>
                {
                    Success = false,
                    Message = "Error al obtener la especialidad: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Crear una nueva especialidad
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EspecialidadDetailDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateEspecialidadDto dto)
        {
            try
            {
                var result = await _especialidadService.CreateAsync(dto);

                _logger.LogInformation("Especialidad creada correctamente: {Codigo}", dto.Codigo);

                return CreatedAtAction(
                    nameof(GetById),
                    new { codigo = result.Codigo },
                    new ApiResponse<EspecialidadDetailDto>
                    {
                        Success = true,
                        Message = "Especialidad creada correctamente",
                        Data = result
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear especialidad");
                return BadRequest(new ApiResponse<EspecialidadDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear especialidad");
                return BadRequest(new ApiResponse<EspecialidadDetailDto>
                {
                    Success = false,
                    Message = "Error al crear la especialidad: " + ex.Message
                });
            }
        }
    }
}
