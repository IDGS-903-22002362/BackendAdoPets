using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.HistorialClinico;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DesparasitacionesController : ControllerBase
{
    private readonly IDesparasitacionService _desparasitacionService;
    private readonly ILogger<DesparasitacionesController> _logger;

    public DesparasitacionesController(IDesparasitacionService desparasitacionService, ILogger<DesparasitacionesController> logger)
    {
        _desparasitacionService = desparasitacionService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DesparasitacionDto>>> GetById(Guid id)
    {
        try
        {
            var desparasitacion = await _desparasitacionService.GetByIdAsync(id);
            if (desparasitacion == null)
            {
                return NotFound(ApiResponse<DesparasitacionDto>.ErrorResponse("Desparasitación no encontrada"));
            }

            return Ok(ApiResponse<DesparasitacionDto>.SuccessResponse(desparasitacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener desparasitación {DesparasitacionId}", id);
            return StatusCode(500, ApiResponse<DesparasitacionDto>.ErrorResponse("Error al obtener desparasitación"));
        }
    }

    [HttpGet("mascota/{mascotaId}")]
    public async Task<ActionResult<ApiResponse<List<DesparasitacionDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var desparasitaciones = await _desparasitacionService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<DesparasitacionDto>>.SuccessResponse(desparasitaciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener desparasitaciones de mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<DesparasitacionDto>>.ErrorResponse("Error al obtener desparasitaciones"));
        }
    }

    [HttpGet("proximas")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<List<DesparasitacionDto>>>> GetUpcomingDue([FromQuery] int days = 30)
    {
        try
        {
            var desparasitaciones = await _desparasitacionService.GetUpcomingDueAsync(days);
            return Ok(ApiResponse<List<DesparasitacionDto>>.SuccessResponse(desparasitaciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener desparasitaciones próximas");
            return StatusCode(500, ApiResponse<List<DesparasitacionDto>>.ErrorResponse("Error al obtener desparasitaciones"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<DesparasitacionDto>>> Create([FromBody] CreateDesparasitacionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DesparasitacionDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var desparasitacion = await _desparasitacionService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = desparasitacion.Id },
                ApiResponse<DesparasitacionDto>.SuccessResponse(desparasitacion, "Desparasitación registrada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar desparasitación");
            return StatusCode(500, ApiResponse<DesparasitacionDto>.ErrorResponse("Error al registrar desparasitación"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _desparasitacionService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Desparasitación eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar desparasitación {DesparasitacionId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar desparasitación"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CirugiasController : ControllerBase
{
    private readonly ICirugiaService _cirugiaService;
    private readonly ILogger<CirugiasController> _logger;

    public CirugiasController(ICirugiaService cirugiaService, ILogger<CirugiasController> logger)
    {
        _cirugiaService = cirugiaService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CirugiaDto>>> GetById(Guid id)
    {
        try
        {
            var cirugia = await _cirugiaService.GetByIdAsync(id);
            if (cirugia == null)
            {
                return NotFound(ApiResponse<CirugiaDto>.ErrorResponse("Cirugía no encontrada"));
            }

            return Ok(ApiResponse<CirugiaDto>.SuccessResponse(cirugia));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cirugía {CirugiaId}", id);
            return StatusCode(500, ApiResponse<CirugiaDto>.ErrorResponse("Error al obtener cirugía"));
        }
    }

    [HttpGet("mascota/{mascotaId}")]
    public async Task<ActionResult<ApiResponse<List<CirugiaDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var cirugias = await _cirugiaService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<CirugiaDto>>.SuccessResponse(cirugias));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cirugías de mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<CirugiaDto>>.ErrorResponse("Error al obtener cirugías"));
        }
    }

    [HttpGet("veterinario/{veterinarioId}")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<List<CirugiaDto>>>> GetByVeterinario(Guid veterinarioId)
    {
        try
        {
            var cirugias = await _cirugiaService.GetByVeterinarioAsync(veterinarioId);
            return Ok(ApiResponse<List<CirugiaDto>>.SuccessResponse(cirugias));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cirugías del veterinario {VetId}", veterinarioId);
            return StatusCode(500, ApiResponse<List<CirugiaDto>>.ErrorResponse("Error al obtener cirugías"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<CirugiaDto>>> Create([FromBody] CreateCirugiaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CirugiaDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cirugia = await _cirugiaService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = cirugia.Id },
                ApiResponse<CirugiaDto>.SuccessResponse(cirugia, "Cirugía registrada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar cirugía");
            return StatusCode(500, ApiResponse<CirugiaDto>.ErrorResponse("Error al registrar cirugía"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _cirugiaService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cirugía eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cirugía {CirugiaId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar cirugía"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ValoracionesController : ControllerBase
{
    private readonly IValoracionService _valoracionService;
    private readonly ILogger<ValoracionesController> _logger;

    public ValoracionesController(IValoracionService valoracionService, ILogger<ValoracionesController> logger)
    {
        _valoracionService = valoracionService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ValoracionDto>>> GetById(Guid id)
    {
        try
        {
            var valoracion = await _valoracionService.GetByIdAsync(id);
            if (valoracion == null)
            {
                return NotFound(ApiResponse<ValoracionDto>.ErrorResponse("Valoración no encontrada"));
            }

            return Ok(ApiResponse<ValoracionDto>.SuccessResponse(valoracion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valoración {ValoracionId}", id);
            return StatusCode(500, ApiResponse<ValoracionDto>.ErrorResponse("Error al obtener valoración"));
        }
    }

    [HttpGet("mascota/{mascotaId}")]
    public async Task<ActionResult<ApiResponse<List<ValoracionDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var valoraciones = await _valoracionService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<ValoracionDto>>.SuccessResponse(valoraciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener valoraciones de mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<ValoracionDto>>.ErrorResponse("Error al obtener valoraciones"));
        }
    }

    [HttpGet("mascota/{mascotaId}/ultima")]
    public async Task<ActionResult<ApiResponse<ValoracionDto>>> GetLatestByMascota(Guid mascotaId)
    {
        try
        {
            var valoracion = await _valoracionService.GetLatestByMascotaAsync(mascotaId);
            if (valoracion == null)
            {
                return NotFound(ApiResponse<ValoracionDto>.ErrorResponse("No se encontraron valoraciones para esta mascota"));
            }

            return Ok(ApiResponse<ValoracionDto>.SuccessResponse(valoracion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener última valoración de mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<ValoracionDto>.ErrorResponse("Error al obtener valoración"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<ValoracionDto>>> Create([FromBody] CreateValoracionDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<ValoracionDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var valoracion = await _valoracionService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = valoracion.Id },
                ApiResponse<ValoracionDto>.SuccessResponse(valoracion, "Valoración registrada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar valoración");
            return StatusCode(500, ApiResponse<ValoracionDto>.ErrorResponse("Error al registrar valoración"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _valoracionService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Valoración eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar valoración {ValoracionId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar valoración"));
        }
    }
}
