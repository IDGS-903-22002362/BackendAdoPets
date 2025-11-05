using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalasController : ControllerBase
{
    private readonly ISalaService _salaService;
    private readonly ILogger<SalasController> _logger;

    public SalasController(ISalaService salaService, ILogger<SalasController> logger)
    {
        _salaService = salaService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtiene todas las salas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SalaListDto>>>> GetAll()
    {
        try
        {
            var salas = await _salaService.GetAllAsync();
            return Ok(ApiResponse<List<SalaListDto>>.SuccessResponse(salas, "Salas obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener salas");
            return StatusCode(500, ApiResponse<List<SalaListDto>>.ErrorResponse("Error al obtener salas"));
        }
    }

    /// <summary>
    /// Obtiene todas las salas activas
    /// </summary>
    [HttpGet("activas")]
    public async Task<ActionResult<ApiResponse<List<SalaListDto>>>> GetActive()
    {
        try
        {
            var salas = await _salaService.GetActiveAsync();
            return Ok(ApiResponse<List<SalaListDto>>.SuccessResponse(salas, "Salas activas obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener salas activas");
            return StatusCode(500, ApiResponse<List<SalaListDto>>.ErrorResponse("Error al obtener salas activas"));
        }
    }

    /// <summary>
    /// Obtiene una sala por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SalaDetailDto>>> GetById(Guid id)
    {
        try
        {
            var sala = await _salaService.GetByIdAsync(id);
            if (sala == null)
            {
                return NotFound(ApiResponse<SalaDetailDto>.ErrorResponse("Sala no encontrada"));
            }

            return Ok(ApiResponse<SalaDetailDto>.SuccessResponse(sala));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener sala {SalaId}", id);
            return StatusCode(500, ApiResponse<SalaDetailDto>.ErrorResponse("Error al obtener sala"));
        }
    }

    /// <summary>
    /// Crea una nueva sala
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SalaDetailDto>>> Create([FromBody] CreateSalaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SalaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var sala = await _salaService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = sala.Id },
                ApiResponse<SalaDetailDto>.SuccessResponse(sala, "Sala creada exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<SalaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear sala");
            return StatusCode(500, ApiResponse<SalaDetailDto>.ErrorResponse("Error al crear sala"));
        }
    }

    /// <summary>
    /// Actualiza una sala
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SalaDetailDto>>> Update(Guid id, [FromBody] UpdateSalaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<SalaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var sala = await _salaService.UpdateAsync(id, dto, userId);

            return Ok(ApiResponse<SalaDetailDto>.SuccessResponse(sala, "Sala actualizada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<SalaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<SalaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar sala {SalaId}", id);
            return StatusCode(500, ApiResponse<SalaDetailDto>.ErrorResponse("Error al actualizar sala"));
        }
    }

    /// <summary>
    /// Elimina una sala (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _salaService.DeleteAsync(id, userId);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Sala eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar sala {SalaId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar sala"));
        }
    }
}
