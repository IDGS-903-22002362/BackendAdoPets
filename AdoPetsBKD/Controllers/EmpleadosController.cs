using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.DTOs.Empleados;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Controllers
{
    /// <summary>
    /// Controlador para gestionar empleados
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmpleadosController : ControllerBase
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly ILogger<EmpleadosController> _logger;

        public EmpleadosController(IEmpleadoService empleadoService, ILogger<EmpleadosController> logger)
        {
            _empleadoService = empleadoService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los empleados con paginación
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<EmpleadoListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var result = await _empleadoService.GetAllAsync(pageNumber, pageSize, includeInactive);
                return Ok(new ApiResponse<PagedResponse<EmpleadoListDto>>
                {
                    Success = true,
                    Message = "Empleados obtenidos correctamente",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de empleados");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<PagedResponse<EmpleadoListDto>>
                {
                    Success = false,
                    Message = "Error al obtener la lista de empleados: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Crear un nuevo empleado
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EmpleadoDetailDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateEmpleadoDto dto)
        {
            try
            {
                var createdBy = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.Empty.ToString());

                var result = await _empleadoService.CreateAsync(dto, createdBy);

                _logger.LogInformation("Empleado creado correctamente: {Cedula}", dto.Cedula);

                return CreatedAtAction(
                    nameof(GetById),   
                    new { id = result.Id },
                    new ApiResponse<EmpleadoDetailDto>
                    {
                        Success = true,
                        Message = "Empleado creado correctamente",
                        Data = result
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear empleado");
                return BadRequest(new ApiResponse<EmpleadoDetailDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empleado");
                return BadRequest(new ApiResponse<EmpleadoDetailDto>
                {
                    Success = false,
                    Message = "Error al crear el empleado: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar un empleado existente    
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EmpleadoDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] EmpleadoUpdateDto dto)
        {
            try
            {
                var currentEmpleadorId = GetCurrentUserId();
                var empleado = await _empleadoService.UpdateAsync(id, dto, currentEmpleadorId); 

                _logger.LogInformation("Empleado actualizado {Id}", empleado.Id);
                return Ok(ApiResponse<EmpleadoDetailDto>.SuccessResponse(empleado, "Empleado actualizado exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar empleado");
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar los datos del empleado");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error al actualizar los datos del empleado: " + ex.Message));
            }
        }

        /// <summary>
        /// Eliminar un empleado 
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _empleadoService.DeleteAsync(id, currentUserId);

                _logger.LogInformation("Empleado eliminado: {Id}", id);
                return Ok(ApiResponse<object>.SuccessResponse(null, "Empleado eliminado exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el empleado");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error al eliminar el empleado: " + ex.Message));
            }
        }

        /// <summary>
        /// Activar a un empleado
        /// </summary>
        [HttpPatch("{id}/activate")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EmpleadoDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Activate(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var empleado = await _empleadoService.ReactivarAsync(id, currentUserId);

                if (empleado == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Empleado no encontrado"));

                _logger.LogInformation("Empleado reactivado: {Id}", id);
                return Ok(ApiResponse<EmpleadoDetailDto>.SuccessResponse(empleado, "Empleado reactivado exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar empleado");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error al reactivar empleado: " + ex.Message));
            }
        }

        /// <summary>
        /// Desactivar un empleado 
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EmpleadoDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var empleado = await _empleadoService.DarDeBajaAsync(id, currentUserId);

                if (empleado == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("Empleado no encontrado"));

                _logger.LogInformation("Empleado dado de baja: {Id}", id);
                return Ok(ApiResponse<EmpleadoDetailDto>.SuccessResponse(empleado, "Empleado dado de baja exitosamente"));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al dar de baja al empleado");
                return BadRequest(ApiResponse<object>.ErrorResponse("Error al dar de baja al empleado: " + ex.Message));
            }
        }

        /// <summary>
        /// Consultar el registro de un empleado
        /// </summary>
        /// <param name="id">ID del empleado</param>
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<EmpleadoDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var empleado = await _empleadoService.GetByIdAsync(id);
            if (empleado == null)
                return NotFound(new ApiResponse<EmpleadoDetailDto> { Success = false, Message = "Empleado no encontrado" });

            return Ok(new ApiResponse<EmpleadoDetailDto> { Success = true, Data = empleado, Message = "Empleado encontrado" });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado");
            }
            return userId;
        }
    }
}
