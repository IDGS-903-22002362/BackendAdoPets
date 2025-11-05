using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;
    private readonly ILogger<CitasController> _logger;

    public CitasController(ICitaService citaService, ILogger<CitasController> logger)
    {
        _citaService = citaService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtiene todas las citas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetAll()
    {
        try
        {
            var citas = await _citaService.GetAllAsync();
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas, "Citas obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas");
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene una cita por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> GetById(Guid id)
    {
        try
        {
            var cita = await _citaService.GetByIdAsync(id);
            if (cita == null)
            {
                return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse("Cita no encontrada"));
            }

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al obtener cita"));
        }
    }

    /// <summary>
    /// Obtiene citas por veterinario
    /// </summary>
    [HttpGet("veterinario/{veterinarioId}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByVeterinario(
        Guid veterinarioId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var citas = await _citaService.GetByVeterinarioAsync(veterinarioId, startDate, endDate);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas del veterinario {VetId}", veterinarioId);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por mascota
    /// </summary>
    [HttpGet("mascota/{mascotaId}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var citas = await _citaService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas de la mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por propietario
    /// </summary>
    [HttpGet("propietario/{propietarioId}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByPropietario(Guid propietarioId)
    {
        try
        {
            var citas = await _citaService.GetByPropietarioAsync(propietarioId);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas del propietario {PropietarioId}", propietarioId);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por rango de fechas
    /// </summary>
    [HttpGet("rango")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var citas = await _citaService.GetByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas por rango de fechas");
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por estado
    /// </summary>
    [HttpGet("estado/{status}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByStatus(StatusCita status)
    {
        try
        {
            var citas = await _citaService.GetByStatusAsync(status);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas por estado {Status}", status);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Crea una nueva cita
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Create([FromBody] CreateCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = cita.Id },
                ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita creada exitosamente"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cita");
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al crear cita"));
        }
    }

    /// <summary>
    /// Actualiza una cita
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Update(Guid id, [FromBody] UpdateCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.UpdateAsync(id, dto, userId);

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita actualizada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al actualizar cita"));
        }
    }

    /// <summary>
    /// Cancela una cita
    /// </summary>
    [HttpPut("{id}/cancelar")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Cancelar(Guid id, [FromBody] CancelarCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.CancelarAsync(id, dto, userId);

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita cancelada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al cancelar cita"));
        }
    }

    /// <summary>
    /// Completa una cita
    /// </summary>
    [HttpPut("{id}/completar")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Completar(Guid id, [FromBody] CompletarCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.CompletarAsync(id, dto, userId);

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita completada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al completar cita"));
        }
    }

    /// <summary>
    /// Elimina una cita
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _citaService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cita eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar cita"));
        }
    }

    /// <summary>
    /// Obtiene disponibilidad de veterinario
    /// </summary>
    [HttpGet("disponibilidad")]
    public async Task<ActionResult<ApiResponse<DisponibilidadResponseDto>>> GetDisponibilidad(
        [FromQuery] DisponibilidadQueryDto query)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DisponibilidadResponseDto>.ErrorResponse("Datos inválidos", errors));
            }

            var disponibilidad = await _citaService.GetDisponibilidadAsync(query);
            return Ok(ApiResponse<DisponibilidadResponseDto>.SuccessResponse(disponibilidad));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener disponibilidad");
            return StatusCode(500, ApiResponse<DisponibilidadResponseDto>.ErrorResponse("Error al obtener disponibilidad"));
        }
    }
}
