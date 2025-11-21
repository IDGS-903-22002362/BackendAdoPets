using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.DTOs.Horarios;
using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Domain.Entities.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers
{
    /// <summary>
    /// Controlador para gestionar horarios de empleados
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class HorarioController : ControllerBase
    {
        private readonly IHorarioService _horarioService;
        private readonly ILogger<HorarioController> _logger;

        public HorarioController(IHorarioService horarioService, ILogger<HorarioController> logger)
        {
            _horarioService = horarioService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los horarios con paginación y filtros opcionales por fechas y tipo
        /// </summary>
        /// <param name="tipo">Tipo de horario: 1=Turno, 2=Descanso, 3=Vacaciones, 4=Permiso, 5=Guardia (opcional)</param>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ListHorarioDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] TipoHorario? tipo = null)
        {
            try
            {
                var result = await _horarioService.GetAllAsync(pageNumber, pageSize, fechaInicio, fechaFin, tipo);
                
                var filtrosAplicados = new List<string>();
                if (fechaInicio.HasValue || fechaFin.HasValue)
                    filtrosAplicados.Add("fechas");
                if (tipo.HasValue)
                    filtrosAplicados.Add($"tipo: {tipo.Value}");

                var message = filtrosAplicados.Any()
                    ? $"Horarios obtenidos correctamente (filtrado por: {string.Join(", ", filtrosAplicados)})"
                    : "Horarios obtenidos correctamente";

                return Ok(new ApiResponse<PagedResponse<ListHorarioDto>>
                {
                    Success = true,
                    Message = message,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de horarios");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<PagedResponse<ListHorarioDto>>
                {
                    Success = false,
                    Message = "Error al obtener la lista de horarios: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener un horario por su ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<DetailHorarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var horario = await _horarioService.GetByIdAsync(id);
                
                if (horario == null)
                {
                    return NotFound(new ApiResponse<DetailHorarioDto>
                    {
                        Success = false,
                        Message = "Horario no encontrado"
                    });
                }

                return Ok(new ApiResponse<DetailHorarioDto>
                {
                    Success = true,
                    Message = "Horario obtenido correctamente",
                    Data = horario
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el horario con ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = "Error al obtener el horario: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Crear un nuevo horario para un empleado
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<DetailHorarioDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateHorarioDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DetailHorarioDto>
                    {
                        Success = false,
                        Message = "Datos inválidos",
                        Data = null
                    });
                }

                var result = await _horarioService.CreateAsync(dto);

                _logger.LogInformation("Horario creado correctamente para el empleado {EmpleadoId}", dto.EmpleadoId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    new ApiResponse<DetailHorarioDto>
                    {
                        Success = true,
                        Message = "Horario creado correctamente",
                        Data = result
                    }
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear horario");
                return BadRequest(new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear horario");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = "Error al crear el horario: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Actualizar un horario existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<DetailHorarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateHorarioDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<DetailHorarioDto>
                    {
                        Success = false,
                        Message = "Datos inválidos"
                    });
                }

                var result = await _horarioService.UpdateAsync(id, dto);

                _logger.LogInformation("Horario actualizado correctamente: {Id}", id);

                return Ok(new ApiResponse<DetailHorarioDto>
                {
                    Success = true,
                    Message = "Horario actualizado correctamente",
                    Data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Horario no encontrado: {Id}", id);
                return NotFound(new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar horario");
                return BadRequest(new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar horario");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = "Error al actualizar el horario: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Eliminar un horario
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
                await _horarioService.DeleteAsync(id);

                _logger.LogInformation("Horario eliminado correctamente: {Id}", id);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Horario eliminado correctamente"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Horario no encontrado: {Id}", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar horario");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error al eliminar el horario: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener horarios de un empleado específico con filtros opcionales por fechas y tipo
        /// </summary>
        /// <param name="tipo">Tipo de horario: 1=Turno, 2=Descanso, 3=Vacaciones, 4=Permiso, 5=Guardia (opcional)</param>
        [HttpGet("empleado/{empleadoId}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<PagedResponse<ListHorarioDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByEmpleado(
            Guid empleadoId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fechaInicio = null,
            [FromQuery] DateTime? fechaFin = null,
            [FromQuery] TipoHorario? tipo = null)
        {
            try
            {
                var result = await _horarioService.GetAllAsync(pageNumber, pageSize, fechaInicio, fechaFin, tipo);
                
                var filteredItems = result.Items.Where(h => h.EmpleadoId == empleadoId).ToList();
                
                var filteredResult = new PagedResponse<ListHorarioDto>
                {
                    Items = filteredItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = filteredItems.Count,
                    TotalPages = (int)Math.Ceiling(filteredItems.Count / (double)pageSize)
                };

                return Ok(new ApiResponse<PagedResponse<ListHorarioDto>>
                {
                    Success = true,
                    Message = $"Horarios del empleado obtenidos correctamente",
                    Data = filteredResult
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener horarios del empleado {EmpleadoId}", empleadoId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<PagedResponse<ListHorarioDto>>
                {
                    Success = false,
                    Message = "Error al obtener los horarios del empleado: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener el horario efectivo de un empleado para una fecha específica
        /// </summary>
        /// <param name="fecha">Fecha para consultar el horario (formato: YYYY-MM-DD)</param>
        [HttpGet("empleado/{empleadoId}/efectivo")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<DetailHorarioDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHorarioEfectivo(Guid empleadoId, [FromQuery] DateTime fecha)
        {
            try
            {
                var horario = await _horarioService.GetHorarioEfectivoAsync(empleadoId, fecha);

                if (horario == null)
                {
                    return NotFound(new ApiResponse<DetailHorarioDto>
                    {
                        Success = false,
                        Message = $"No se encontró un horario para el empleado en la fecha {fecha:dd/MM/yyyy}"
                    });
                }

                return Ok(new ApiResponse<DetailHorarioDto>
                {
                    Success = true,
                    Message = $"Horario efectivo obtenido para {fecha:dd/MM/yyyy}",
                    Data = horario
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener horario efectivo del empleado {EmpleadoId} para fecha {Fecha}", empleadoId, fecha);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<DetailHorarioDto>
                {
                    Success = false,
                    Message = "Error al obtener el horario efectivo: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener todos los horarios aplicables de un empleado para una fecha específica
        /// <param name="fecha">Fecha para consultar (formato: YYYY-MM-DD)</param>
        [HttpGet("empleado/{empleadoId}/aplicables")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<List<ListHorarioDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetHorariosAplicables(Guid empleadoId, [FromQuery] DateTime fecha)
        {
            try
            {
                var horarios = await _horarioService.GetHorariosAplicablesAsync(empleadoId, fecha);

                return Ok(new ApiResponse<List<ListHorarioDto>>
                {
                    Success = true,
                    Message = $"Se encontraron {horarios.Count} horarios aplicables para {fecha:dd/MM/yyyy} (ordenados por prioridad)",
                    Data = horarios
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener horarios aplicables del empleado {EmpleadoId} para fecha {Fecha}", empleadoId, fecha);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<ListHorarioDto>>
                {
                    Success = false,
                    Message = "Error al obtener los horarios aplicables: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener calendario general de todos los empleados
        /// Devuelve los horarios de todo el personal día por día
        /// <param name="incluirInactivos">Incluir empleados inactivos (default: false)</param>
        [HttpGet("calendario")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<List<CalendarioGeneralDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCalendarioGeneral(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin,
            [FromQuery] bool incluirInactivos = false)
        {
            try
            {
                var calendario = await _horarioService.GetCalendarioGeneralAsync(fechaInicio, fechaFin, incluirInactivos);

                var totalEmpleados = calendario.Count;
                var totalDias = (fechaFin.Date - fechaInicio.Date).Days + 1;
                var totalDiasConHorario = calendario.Sum(c => c.Estadisticas.DiasConHorario);
                var totalExcepciones = calendario.Sum(c => c.Estadisticas.TotalExcepciones);

                return Ok(new ApiResponse<List<CalendarioGeneralDto>>
                {
                    Success = true,
                    Message = $"Calendario general: {totalEmpleados} empleados, {totalDias} días ({totalDiasConHorario} días programados, {totalExcepciones} excepciones)",
                    Data = calendario
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al generar calendario general");
                return BadRequest(new ApiResponse<List<CalendarioGeneralDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar calendario general");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<CalendarioGeneralDto>>
                {
                    Success = false,
                    Message = "Error al generar el calendario general: " + ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener calendario expandido de horarios de un empleado
        /// Devuelve día por día con el horario efectivo (considerando prioridades)
        [HttpGet("empleado/{empleadoId}/calendario")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ApiResponse<List<CalendarioHorarioDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetCalendario(
            Guid empleadoId,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var calendario = await _horarioService.GetCalendarioAsync(empleadoId, fechaInicio, fechaFin);

                var diasConHorario = calendario.Count(c => c.TieneHorario);
                var diasSinHorario = calendario.Count(c => !c.TieneHorario);
                var excepciones = calendario.Count(c => c.EsExcepcion);

                return Ok(new ApiResponse<List<CalendarioHorarioDto>>
                {
                    Success = true,
                    Message = $"Calendario generado: {calendario.Count} días ({diasConHorario} con horario, {diasSinHorario} sin horario, {excepciones} excepciones)",
                    Data = calendario
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación al generar calendario");
                return BadRequest(new ApiResponse<List<CalendarioHorarioDto>>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar calendario del empleado {EmpleadoId}", empleadoId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<List<CalendarioHorarioDto>>
                {
                    Success = false,
                    Message = "Error al generar el calendario: " + ex.Message
                });
            }
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
