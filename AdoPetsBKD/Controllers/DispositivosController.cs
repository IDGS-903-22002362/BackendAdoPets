using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DispositivosController : ControllerBase
{
    private readonly IDispositivoRepository _dispositivoRepository;
    private readonly ILogger<DispositivosController> _logger;

    public DispositivosController(
        IDispositivoRepository dispositivoRepository,
        ILogger<DispositivosController> logger)
    {
        _dispositivoRepository = dispositivoRepository;
        _logger = logger;
    }

    /// <summary>
    /// Registrar un nuevo dispositivo para recibir push notifications
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RegistrarDispositivo([FromBody] RegistrarDispositivoDto dto)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            // Verificar si el token ya existe
            var dispositivoExistente = await _dispositivoRepository.GetByTokenAsync(dto.Token);
            if (dispositivoExistente != null)
            {
                // Actualizar dispositivo existente
                dispositivoExistente.Enabled = true;
                dispositivoExistente.AppVersion = dto.AppVersion;
                dispositivoExistente.ActualizarUltimaVista();
                
                await _dispositivoRepository.UpdateAsync(dispositivoExistente);

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { id = dispositivoExistente.Id },
                    "Dispositivo actualizado"
                ));
            }

            // Crear nuevo dispositivo
            var dispositivo = new Dispositivo
            {
                UsuarioId = usuarioId,
                Token = dto.Token,
                Plataforma = dto.Plataforma,
                AppVersion = dto.AppVersion,
                Enabled = true,
                LastSeenAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _dispositivoRepository.AddAsync(dispositivo);

            _logger.LogInformation(
                "?? Dispositivo registrado: UsuarioId={UsuarioId}, Plataforma={Plataforma}",
                usuarioId,
                dto.Plataforma
            );

            return Ok(ApiResponse<object>.SuccessResponse(
                new { id = dispositivo.Id },
                "Dispositivo registrado exitosamente"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar dispositivo");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al registrar dispositivo"));
        }
    }

    /// <summary>
    /// Obtener dispositivos del usuario autenticado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObtenerDispositivos()
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var dispositivos = await _dispositivoRepository.GetByUsuarioIdAsync(usuarioId);

            var result = dispositivos.Select(d => new
            {
                d.Id,
                d.Plataforma,
                PlataformaNombre = d.Plataforma.ToString(),
                d.AppVersion,
                d.Enabled,
                d.LastSeenAt,
                d.CreatedAt
            });

            return Ok(ApiResponse<object>.SuccessResponse(
                result,
                "Dispositivos obtenidos"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dispositivos");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al obtener dispositivos"));
        }
    }

    /// <summary>
    /// Deshabilitar un dispositivo (dejar de enviar notificaciones)
    /// </summary>
    [HttpPut("{id}/deshabilitar")]
    public async Task<IActionResult> DeshabilitarDispositivo(Guid id)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var dispositivos = await _dispositivoRepository.GetByUsuarioIdAsync(usuarioId);
            var dispositivo = dispositivos.FirstOrDefault(d => d.Id == id);

            if (dispositivo == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo no encontrado"));
            }

            dispositivo.Deshabilitar();
            await _dispositivoRepository.UpdateAsync(dispositivo);

            _logger.LogInformation("Dispositivo deshabilitado: {DispositivoId}", id);

            return Ok(ApiResponse<string>.SuccessResponse(
                "Dispositivo deshabilitado",
                "Notificaciones deshabilitadas para este dispositivo"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al deshabilitar dispositivo");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al deshabilitar dispositivo"));
        }
    }

    /// <summary>
    /// Habilitar un dispositivo
    /// </summary>
    [HttpPut("{id}/habilitar")]
    public async Task<IActionResult> HabilitarDispositivo(Guid id)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var dispositivos = await _dispositivoRepository.GetByUsuarioIdAsync(usuarioId);
            var dispositivo = dispositivos.FirstOrDefault(d => d.Id == id);

            if (dispositivo == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo no encontrado"));
            }

            dispositivo.Enabled = true;
            await _dispositivoRepository.UpdateAsync(dispositivo);

            _logger.LogInformation("Dispositivo habilitado: {DispositivoId}", id);

            return Ok(ApiResponse<string>.SuccessResponse(
                "Dispositivo habilitado",
                "Notificaciones habilitadas para este dispositivo"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al habilitar dispositivo");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al habilitar dispositivo"));
        }
    }

    /// <summary>
    /// Eliminar un dispositivo
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarDispositivo(Guid id)
    {
        try
        {
            var usuarioId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var dispositivos = await _dispositivoRepository.GetByUsuarioIdAsync(usuarioId);
            var dispositivo = dispositivos.FirstOrDefault(d => d.Id == id);

            if (dispositivo == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("Dispositivo no encontrado"));
            }

            await _dispositivoRepository.DeleteAsync(id);

            _logger.LogInformation("Dispositivo eliminado: {DispositivoId}", id);

            return Ok(ApiResponse<string>.SuccessResponse(
                "Dispositivo eliminado",
                "El dispositivo ha sido eliminado exitosamente"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar dispositivo");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al eliminar dispositivo"));
        }
    }
}

public class RegistrarDispositivoDto
{
    public string Token { get; set; } = string.Empty;
    public PlataformaDispositivo Plataforma { get; set; }
    public string? AppVersion { get; set; }
}
