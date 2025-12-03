using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Usuarios;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

/// <summary>
/// Controlador de gestión de usuarios
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener lista paginada de usuarios
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UsuarioListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _usuarioService.GetAllAsync(pageNumber, pageSize);
            return Ok(ApiResponse<PagedResponse<UsuarioListDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de usuarios");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener usuarios", ex.Message));
        }
    }

    /// <summary>
    /// Obtener usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var usuario = await _usuarioService.GetByIdAsync(id);

            if (usuario == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Usuario no encontrado", $"No se encontró usuario con ID {id}"));
            }

            return Ok(ApiResponse<UsuarioDetailDto>.SuccessResponse(usuario));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {UserId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener usuario", ex.Message));
        }
    }

    /// <summary>
    /// Crear nuevo usuario
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var usuario = await _usuarioService.CreateAsync(dto, currentUserId);
            
            _logger.LogInformation("Usuario creado exitosamente: {Email} por {CreatedBy}", dto.Email, currentUserId);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = usuario.Id }, 
                ApiResponse<UsuarioDetailDto>.SuccessResponse(usuario, "Usuario creado exitosamente")
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al crear usuario", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al crear usuario", ex.Message));
        }
    }

    /// <summary>
    /// Actualizar usuario existente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUsuarioDto dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var usuario = await _usuarioService.UpdateAsync(id, dto, currentUserId);
            
            _logger.LogInformation("Usuario actualizado: {UserId} por {UpdatedBy}", id, currentUserId);
            
            return Ok(ApiResponse<UsuarioDetailDto>.SuccessResponse(usuario, "Usuario actualizado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Usuario no encontrado", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {UserId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al actualizar usuario", ex.Message));
        }
    }

    /// <summary>
    /// Eliminar usuario
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _usuarioService.DeleteAsync(id, currentUserId);
            
            _logger.LogInformation("Usuario eliminado: {UserId} por {DeletedBy}", id, currentUserId);
            
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Usuario eliminado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Usuario no encontrado", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario {UserId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al eliminar usuario", ex.Message));
        }
    }

    /// <summary>
    /// Activar usuario
    /// </summary>
    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            var result = await _usuarioService.ActivateAsync(id);
            
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Usuario no encontrado", $"No se encontró usuario con ID {id}"));
            }

            _logger.LogInformation("Usuario activado: {UserId}", id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Usuario activado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar usuario {UserId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al activar usuario", ex.Message));
        }
    }

    /// <summary>
    /// Desactivar usuario
    /// </summary>
    [HttpPatch("{id}/deactivate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            var result = await _usuarioService.DeactivateAsync(id);
            
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Usuario no encontrado", $"No se encontró usuario con ID {id}"));
            }

            _logger.LogInformation("Usuario desactivado: {UserId}", id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Usuario desactivado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario {UserId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al desactivar usuario", ex.Message));
        }
    }

    /// <summary>
    /// Asignar roles a usuario
    /// </summary>
    [HttpPost("{id}/roles")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] List<Guid> rolesIds)
    {
        try
        {
            var result = await _usuarioService.AssignRolesAsync(id, rolesIds);
            
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Usuario no encontrado", $"No se encontró usuario con ID {id}"));
            }

            _logger.LogInformation("Roles asignados a usuario: {UserId}", id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Roles asignados exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al asignar roles", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar roles a usuario {UserId}", id);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al asignar roles", ex.Message));
        }
    }

    /// <summary>
    /// Obtener lista de todos los adoptantes con sus mascotas (adoptadas y registradas)
    /// </summary>
    /// <remarks>
    /// Retorna una lista completa de adoptantes incluyendo:
    /// - Mascotas adoptadas del refugio
    /// - Mascotas registradas directamente por el usuario
    /// </remarks>
    [HttpGet("adoptantes/mascotas")]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AdoptanteConMascotasDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdoptantesConMascotas()
    {
        try
        {
            _logger.LogInformation("Obteniendo lista de adoptantes con sus mascotas");
            
            var adoptantes = await _usuarioService.GetAdoptantesConMascotasAsync();
            
            // Convertir las URLs de fotos a URLs completas
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            foreach (var adoptante in adoptantes)
            {
                foreach (var mascota in adoptante.Mascotas)
                {
                    foreach (var foto in mascota.Fotos)
                    {
                        if (!string.IsNullOrEmpty(foto.StorageKey) && !foto.StorageKey.StartsWith("http"))
                        {
                            if (foto.StorageKey.StartsWith("/"))
                            {
                                foto.StorageKey = $"{baseUrl}{foto.StorageKey}";
                            }
                            else if (foto.StorageKey.StartsWith("uploads/"))
                            {
                                foto.StorageKey = $"{baseUrl}/{foto.StorageKey}";
                            }
                        }
                    }
                }
            }

            _logger.LogInformation("Se encontraron {Count} adoptantes con mascotas", adoptantes.Count());
            
            return Ok(ApiResponse<IEnumerable<AdoptanteConMascotasDto>>.SuccessResponse(
                adoptantes, 
                $"Se encontraron {adoptantes.Count()} adoptantes"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de adoptantes con mascotas");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener adoptantes", ex.Message));
        }
    }

    /// <summary>
    /// Obtener un adoptante específico con todas sus mascotas
    /// </summary>
    /// <param name="usuarioId">ID del usuario adoptante</param>
    /// <remarks>
    /// Retorna información detallada del adoptante incluyendo:
    /// - Datos personales del adoptante
    /// - Mascotas adoptadas del refugio con fecha de adopción
    /// - Mascotas registradas directamente con fecha de registro
    /// - Estadísticas: total de mascotas, adoptadas y registradas
    /// </remarks>
    [HttpGet("adoptantes/{usuarioId}/mascotas")]
    [Authorize(Policy = "StaffOnly")]
    [ProducesResponseType(typeof(ApiResponse<AdoptanteConMascotasDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdoptanteConMascotasById(Guid usuarioId)
    {
        try
        {
            _logger.LogInformation("Obteniendo adoptante {UsuarioId} con sus mascotas", usuarioId);
            
            var adoptante = await _usuarioService.GetAdoptanteConMascotasByIdAsync(usuarioId);

            if (adoptante == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(
                    "Adoptante no encontrado", 
                    $"No se encontró adoptante con ID {usuarioId}"
                ));
            }

            // Convertir las URLs de fotos a URLs completas
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            foreach (var mascota in adoptante.Mascotas)
            {
                foreach (var foto in mascota.Fotos)
                {
                    if (!string.IsNullOrEmpty(foto.StorageKey) && !foto.StorageKey.StartsWith("http"))
                    {
                        if (foto.StorageKey.StartsWith("/"))
                        {
                            foto.StorageKey = $"{baseUrl}{foto.StorageKey}";
                        }
                        else if (foto.StorageKey.StartsWith("uploads/"))
                        {
                            foto.StorageKey = $"{baseUrl}/{foto.StorageKey}";
                        }
                    }
                }
            }

            _logger.LogInformation(
                "Adoptante {UsuarioId} tiene {TotalMascotas} mascotas ({Adoptadas} adoptadas, {Registradas} registradas)", 
                usuarioId, 
                adoptante.TotalMascotas, 
                adoptante.MascotasAdoptadas, 
                adoptante.MascotasRegistradas
            );
            
            return Ok(ApiResponse<AdoptanteConMascotasDto>.SuccessResponse(
                adoptante, 
                $"Adoptante encontrado con {adoptante.TotalMascotas} mascotas"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener adoptante {UsuarioId} con mascotas", usuarioId);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener adoptante", ex.Message));
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        return userId;
    }

    #endregion
}
