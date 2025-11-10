using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Servicios;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

/// <summary>
/// Controlador para gestionar servicios veterinarios
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly IServicioService _servicioService;
    private readonly ILogger<ServiciosController> _logger;

    public ServiciosController(IServicioService servicioService, ILogger<ServiciosController> logger)
    {
        _servicioService = servicioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los servicios activos (para usuarios)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<ServicioDto>>>> GetServicios()
    {
        try
        {
            var servicios = await _servicioService.GetActivosAsync();
            return Ok(ApiResponse<List<ServicioDto>>.SuccessResponse(servicios));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener servicios");
            return BadRequest(ApiResponse<List<ServicioDto>>.ErrorResponse("Error al obtener servicios"));
        }
    }

    /// <summary>
    /// Obtiene todos los servicios incluyendo inactivos (solo admin)
    /// </summary>
    [HttpGet("todos")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<List<ServicioDto>>>> GetTodosLosServicios([FromQuery] bool incluirInactivos = false)
    {
        try
        {
            var servicios = await _servicioService.GetAllAsync(incluirInactivos);
            return Ok(ApiResponse<List<ServicioDto>>.SuccessResponse(servicios));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todos los servicios");
            return BadRequest(ApiResponse<List<ServicioDto>>.ErrorResponse("Error al obtener servicios"));
        }
    }

    /// <summary>
    /// Obtiene un servicio por ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ServicioDto>>> GetServicioById(Guid id)
    {
        try
        {
            var servicio = await _servicioService.GetByIdAsync(id);
            if (servicio == null)
            {
                return NotFound(ApiResponse<ServicioDto>.ErrorResponse("Servicio no encontrado"));
            }

            return Ok(ApiResponse<ServicioDto>.SuccessResponse(servicio));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener servicio {Id}", id);
            return BadRequest(ApiResponse<ServicioDto>.ErrorResponse("Error al obtener servicio"));
        }
    }

    /// <summary>
    /// Crea un nuevo servicio (solo admin)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ServicioDto>>> CreateServicio([FromBody] CreateServicioDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var servicio = await _servicioService.CreateAsync(dto, userId);

            _logger.LogInformation("Servicio creado: {Descripcion}", dto.Descripcion);

            return CreatedAtAction(
                nameof(GetServicioById),
                new { id = servicio.Id },
                ApiResponse<ServicioDto>.SuccessResponse(servicio, "Servicio creado exitosamente")
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<ServicioDto>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear servicio");
            return BadRequest(ApiResponse<ServicioDto>.ErrorResponse("Error al crear servicio"));
        }
    }

    /// <summary>
    /// Actualiza un servicio existente (solo admin)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<ServicioDto>>> UpdateServicio(Guid id, [FromBody] UpdateServicioDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var servicio = await _servicioService.UpdateAsync(id, dto, userId);

            _logger.LogInformation("Servicio actualizado: {Id}", id);

            return Ok(ApiResponse<ServicioDto>.SuccessResponse(servicio, "Servicio actualizado exitosamente"));
        }
        catch (InvalidOperationException)
        {
            return NotFound(ApiResponse<ServicioDto>.ErrorResponse("Servicio no encontrado"));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<ServicioDto>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar servicio");
            return BadRequest(ApiResponse<ServicioDto>.ErrorResponse("Error al actualizar servicio"));
        }
    }

    /// <summary>
    /// Desactiva un servicio (solo admin)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteServicio(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _servicioService.DeleteAsync(id, userId);

            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Servicio no encontrado"));
            }

            _logger.LogInformation("Servicio desactivado: {Id}", id);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Servicio desactivado exitosamente"));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar servicio");
            return BadRequest(ApiResponse<bool>.ErrorResponse("Error al desactivar servicio"));
        }
    }

    /// <summary>
    /// Activa un servicio desactivado (solo admin)
    /// </summary>
    [HttpPatch("{id}/activar")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> ActivarServicio(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _servicioService.ActivarAsync(id, userId);

            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Servicio no encontrado"));
            }

            _logger.LogInformation("Servicio activado: {Id}", id);

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Servicio activado exitosamente"));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar servicio");
            return BadRequest(ApiResponse<bool>.ErrorResponse("Error al activar servicio"));
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el token JWT
    /// </summary>
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
