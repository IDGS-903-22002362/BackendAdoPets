using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.HistorialClinico;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VacunacionesController : ControllerBase
{
    private readonly IVacunacionService _vacunacionService;
    private readonly ILogger<VacunacionesController> _logger;

    public VacunacionesController(IVacunacionService vacunacionService, ILogger<VacunacionesController> logger)
    {
        _vacunacionService = vacunacionService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtiene una vacunación por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<VacunacionDto>>> GetById(Guid id)
    {
        try
        {
            var vacunacion = await _vacunacionService.GetByIdAsync(id);
            if (vacunacion == null)
            {
                return NotFound(ApiResponse<VacunacionDto>.ErrorResponse("Vacunación no encontrada"));
            }

            return Ok(ApiResponse<VacunacionDto>.SuccessResponse(vacunacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener vacunación {VacunacionId}", id);
            return StatusCode(500, ApiResponse<VacunacionDto>.ErrorResponse("Error al obtener vacunación"));
        }
    }

    /// <summary>
    /// Obtiene vacunaciones por mascota
    /// </summary>
    [HttpGet("mascota/{mascotaId}")]
    public async Task<ActionResult<ApiResponse<List<VacunacionDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var vacunaciones = await _vacunacionService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<VacunacionDto>>.SuccessResponse(vacunaciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener vacunaciones de mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<VacunacionDto>>.ErrorResponse("Error al obtener vacunaciones"));
        }
    }

    /// <summary>
    /// Obtiene vacunaciones próximas a vencer
    /// </summary>
    [HttpGet("proximas")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<List<VacunacionDto>>>> GetUpcomingDue([FromQuery] int days = 30)
    {
        try
        {
            var vacunaciones = await _vacunacionService.GetUpcomingDueAsync(days);
            return Ok(ApiResponse<List<VacunacionDto>>.SuccessResponse(vacunaciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener vacunaciones próximas");
            return StatusCode(500, ApiResponse<List<VacunacionDto>>.ErrorResponse("Error al obtener vacunaciones"));
        }
    }

    /// <summary>
    /// Registra una nueva vacunación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<VacunacionDto>>> Create([FromBody] CreateVacunacionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<VacunacionDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var vacunacion = await _vacunacionService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = vacunacion.Id },
                ApiResponse<VacunacionDto>.SuccessResponse(vacunacion, "Vacunación registrada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar vacunación");
            return StatusCode(500, ApiResponse<VacunacionDto>.ErrorResponse("Error al registrar vacunación"));
        }
    }

    /// <summary>
    /// Elimina una vacunación
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _vacunacionService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Vacunación eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar vacunación {VacunacionId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar vacunación"));
        }
    }
}
