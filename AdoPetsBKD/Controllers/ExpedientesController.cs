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
public class ExpedientesController : ControllerBase
{
    private readonly IExpedienteService _expedienteService;
    private readonly IAdjuntoMedicoService _adjuntoService;
    private readonly ILogger<ExpedientesController> _logger;

    public ExpedientesController(
        IExpedienteService expedienteService,
        IAdjuntoMedicoService adjuntoService,
        ILogger<ExpedientesController> logger)
    {
        _expedienteService = expedienteService;
        _adjuntoService = adjuntoService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtiene un expediente por ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<ExpedienteDetailDto>>> GetById(Guid id)
    {
        try
        {
            var expediente = await _expedienteService.GetByIdAsync(id);
            if (expediente == null)
            {
                return NotFound(ApiResponse<ExpedienteDetailDto>.ErrorResponse("Expediente no encontrado"));
            }

            return Ok(ApiResponse<ExpedienteDetailDto>.SuccessResponse(expediente));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener expediente {ExpedienteId}", id);
            return StatusCode(500, ApiResponse<ExpedienteDetailDto>.ErrorResponse("Error al obtener expediente"));
        }
    }

    /// <summary>
    /// Obtiene expedientes por mascota
    /// </summary>
    [HttpGet("mascota/{mascotaId}")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<List<ExpedienteListDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var expedientes = await _expedienteService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<ExpedienteListDto>>.SuccessResponse(expedientes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener expedientes de mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<ExpedienteListDto>>.ErrorResponse("Error al obtener expedientes"));
        }
    }

    /// <summary>
    /// Obtiene expedientes por veterinario
    /// </summary>
    [HttpGet("veterinario/{veterinarioId}")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<List<ExpedienteListDto>>>> GetByVeterinario(Guid veterinarioId)
    {
        try
        {
            var expedientes = await _expedienteService.GetByVeterinarioAsync(veterinarioId);
            return Ok(ApiResponse<List<ExpedienteListDto>>.SuccessResponse(expedientes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener expedientes del veterinario {VetId}", veterinarioId);
            return StatusCode(500, ApiResponse<List<ExpedienteListDto>>.ErrorResponse("Error al obtener expedientes"));
        }
    }

    /// <summary>
    /// Crea un nuevo expediente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<ExpedienteDetailDto>>> Create([FromBody] CreateExpedienteDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ExpedienteDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var expediente = await _expedienteService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = expediente.Id },
                ApiResponse<ExpedienteDetailDto>.SuccessResponse(expediente, "Expediente creado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear expediente");
            return StatusCode(500, ApiResponse<ExpedienteDetailDto>.ErrorResponse("Error al crear expediente"));
        }
    }

    /// <summary>
    /// Elimina un expediente
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _expedienteService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Expediente eliminado exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar expediente {ExpedienteId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar expediente"));
        }
    }

    /// <summary>
    /// Agrega un adjunto médico a un expediente
    /// </summary>
    [HttpPost("{id}/adjuntos")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<AdjuntoMedicoDto>>> AddAdjunto(Guid id, [FromBody] CreateAdjuntoMedicoDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<AdjuntoMedicoDto>.ErrorResponse("Datos inválidos", errors));
            }

            // Validar que el expediente existe
            var expediente = await _expedienteService.GetByIdAsync(id);
            if (expediente == null)
            {
                return NotFound(ApiResponse<AdjuntoMedicoDto>.ErrorResponse("Expediente no encontrado"));
            }

            dto.ExpedienteId = id;
            var userId = GetUserId();
            var adjunto = await _adjuntoService.CreateAsync(dto, userId);

            return Ok(ApiResponse<AdjuntoMedicoDto>.SuccessResponse(adjunto, "Adjunto agregado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar adjunto al expediente {ExpedienteId}", id);
            return StatusCode(500, ApiResponse<AdjuntoMedicoDto>.ErrorResponse("Error al agregar adjunto"));
        }
    }

    /// <summary>
    /// Elimina un adjunto médico
    /// </summary>
    [HttpDelete("adjuntos/{adjuntoId}")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAdjunto(Guid adjuntoId)
    {
        try
        {
            await _adjuntoService.DeleteAsync(adjuntoId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Adjunto eliminado exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar adjunto {AdjuntoId}", adjuntoId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar adjunto"));
        }
    }
}
