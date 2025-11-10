using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.DTOs.Mascota.MascotaUsuario;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

/// <summary>
/// Controlador para que los usuarios gestionen sus propias mascotas (para citas veterinarias)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Solo usuarios autenticados
public class MisMascotasController : ControllerBase
{
    private readonly IMascotaUsuarioService _mascotaUsuarioService;
    private readonly ILogger<MisMascotasController> _logger;

    public MisMascotasController(
        IMascotaUsuarioService mascotaUsuarioService,
        ILogger<MisMascotasController> logger)
    {
        _mascotaUsuarioService = mascotaUsuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las mascotas del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<MascotaUsuarioDetailDto>>>> GetMisMascotas()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var mascotas = await _mascotaUsuarioService.GetMascotasByUsuarioAsync(userId);
            return Ok(ApiResponse<IEnumerable<MascotaUsuarioDetailDto>>.SuccessResponse(mascotas));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<IEnumerable<MascotaUsuarioDetailDto>>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<IEnumerable<MascotaUsuarioDetailDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Obtiene una mascota específica del usuario autenticado
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MascotaUsuarioDetailDto>>> GetById(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var mascota = await _mascotaUsuarioService.GetByIdAsync(id, userId);
            if (mascota == null)
                return NotFound(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse("Mascota no encontrada o no pertenece al usuario"));

            return Ok(ApiResponse<MascotaUsuarioDetailDto>.SuccessResponse(mascota));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Registra una nueva mascota para el usuario autenticado
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MascotaUsuarioDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MascotaUsuarioDetailDto>>> Create([FromBody] CreateMascotaUsuarioDto dto)
    {
        // ============ LOGGING TEMPORAL - DEBUGGING 401 ============
        _logger.LogWarning("?? === INICIO DEBUG DE CLAIMS ===");
        _logger.LogWarning($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        _logger.LogWarning($"User.Identity.Name: {User.Identity?.Name}");
        _logger.LogWarning($"Total Claims: {User.Claims.Count()}");
        
        foreach (var claim in User.Claims)
        {
            _logger.LogWarning($"  Claim: {claim.Type} = {claim.Value}");
        }
        
        _logger.LogWarning("?? === FIN DEBUG DE CLAIMS ===");
        // ========================================================

        try
        {
            // DEBUG: Verificar headers
            var authHeader = Request.Headers["Authorization"].ToString();
            _logger.LogInformation("Authorization Header: {AuthHeader}", 
                string.IsNullOrEmpty(authHeader) ? "NO PRESENTE" : authHeader.Substring(0, Math.Min(50, authHeader.Length)) + "...");
            
            // DEBUG: Verificar claims del usuario
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            _logger.LogInformation("User Claims: {Claims}", string.Join(", ", claims));
            
            var userId = GetCurrentUserId();
            _logger.LogInformation("User ID extraído: {UserId}", userId);
            
            var mascota = await _mascotaUsuarioService.CreateAsync(dto, userId);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = mascota.Id }, 
                ApiResponse<MascotaUsuarioDetailDto>.SuccessResponse(mascota, "Mascota registrada exitosamente")
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Usuario no autenticado: {Message}", ex.Message);
            return Unauthorized(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse("Usuario no autenticado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear mascota");
            return BadRequest(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza una mascota del usuario autenticado
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<MascotaUsuarioDetailDto>>> Update(Guid id, [FromBody] UpdateMascotaUsuarioDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var mascota = await _mascotaUsuarioService.UpdateAsync(id, dto, userId);
            return Ok(ApiResponse<MascotaUsuarioDetailDto>.SuccessResponse(mascota, "Mascota actualizada exitosamente"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Elimina una mascota del usuario autenticado
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var result = await _mascotaUsuarioService.DeleteAsync(id, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Mascota eliminada exitosamente"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Agrega fotos a una mascota del usuario autenticado
    /// </summary>
    [HttpPost("{id}/fotos")]
    public async Task<ActionResult<ApiResponse<MascotaUsuarioDetailDto>>> AddPhotos(Guid id, [FromBody] IEnumerable<CreatePhotoDto> fotos)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var mascota = await _mascotaUsuarioService.AddPhotosAsync(id, fotos, userId);
            return Ok(ApiResponse<MascotaUsuarioDetailDto>.SuccessResponse(mascota, "Fotos agregadas exitosamente"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<MascotaUsuarioDetailDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Elimina una foto de una mascota del usuario autenticado
    /// </summary>
    [HttpDelete("fotos/{fotoId}")]
    public async Task<ActionResult<ApiResponse<string>>> DeletePhoto(Guid fotoId)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            var result = await _mascotaUsuarioService.DeletePhotoAsync(fotoId, userId);
            return Ok(ApiResponse<string>.SuccessResponse(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
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
