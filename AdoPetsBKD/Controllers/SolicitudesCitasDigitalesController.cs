using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitudesCitasDigitalesController : ControllerBase
{
    private readonly ISolicitudCitaDigitalService _solicitudService;

    public SolicitudesCitasDigitalesController(ISolicitudCitaDigitalService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SolicitudCitaDigitalDto>>> CreateSolicitud([FromBody] CreateSolicitudCitaDigitalDto dto)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var solicitud = await _solicitudService.CreateSolicitudAsync(dto, userId);
            return Ok(ApiResponse<SolicitudCitaDigitalDto>.SuccessResponse(solicitud, "Solicitud creada exitosamente"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SolicitudCitaDigitalDto>>> GetSolicitudById(Guid id)
    {
        try
        {
            var solicitud = await _solicitudService.GetSolicitudByIdAsync(id);
            if (solicitud == null)
                return NotFound(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse("Solicitud no encontrada"));

            return Ok(ApiResponse<SolicitudCitaDigitalDto>.SuccessResponse(solicitud));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("usuario/{usuarioId}")]
    public async Task<ActionResult<ApiResponse<List<SolicitudCitaDigitalDto>>>> GetSolicitudesByUsuario(Guid usuarioId)
    {
        try
        {
            var solicitudes = await _solicitudService.GetSolicitudesByUsuarioAsync(usuarioId);
            return Ok(ApiResponse<List<SolicitudCitaDigitalDto>>.SuccessResponse(solicitudes));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<SolicitudCitaDigitalDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult<ApiResponse<List<SolicitudCitaDigitalDto>>>> GetSolicitudesPendientes()
    {
        try
        {
            var solicitudes = await _solicitudService.GetSolicitudesPendientesAsync();
            return Ok(ApiResponse<List<SolicitudCitaDigitalDto>>.SuccessResponse(solicitudes));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<SolicitudCitaDigitalDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("verificar-disponibilidad")]
    public async Task<ActionResult<ApiResponse<DisponibilidadResponseDto>>> VerificarDisponibilidad([FromBody] VerificarDisponibilidadDto dto)
    {
        try
        {
            var disponibilidad = await _solicitudService.VerificarDisponibilidadAsync(dto);
            return Ok(ApiResponse<DisponibilidadResponseDto>.SuccessResponse(disponibilidad));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DisponibilidadResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/en-revision")]
    public async Task<ActionResult<ApiResponse<SolicitudCitaDigitalDto>>> MarcarEnRevision(Guid id)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var solicitud = await _solicitudService.MarcarComoEnRevisionAsync(id, userId);
            return Ok(ApiResponse<SolicitudCitaDigitalDto>.SuccessResponse(solicitud, "Solicitud en revisión"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("confirmar")]
    public async Task<ActionResult<ApiResponse<SolicitudCitaDigitalDto>>> ConfirmarSolicitud([FromBody] ConfirmarSolicitudCitaDto dto)
    {
        try
        {
            var solicitud = await _solicitudService.ConfirmarSolicitudAsync(dto);
            return Ok(ApiResponse<SolicitudCitaDigitalDto>.SuccessResponse(solicitud, "Solicitud confirmada y cita creada"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("rechazar")]
    public async Task<ActionResult<ApiResponse<SolicitudCitaDigitalDto>>> RechazarSolicitud([FromBody] RechazarSolicitudCitaDto dto)
    {
        try
        {
            var solicitud = await _solicitudService.RechazarSolicitudAsync(dto);
            return Ok(ApiResponse<SolicitudCitaDigitalDto>.SuccessResponse(solicitud, "Solicitud rechazada"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/cancelar")]
    public async Task<ActionResult<ApiResponse<SolicitudCitaDigitalDto>>> CancelarSolicitud(Guid id)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var solicitud = await _solicitudService.CancelarSolicitudAsync(id, userId);
            return Ok(ApiResponse<SolicitudCitaDigitalDto>.SuccessResponse(solicitud, "Solicitud cancelada"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SolicitudCitaDigitalDto>.ErrorResponse(ex.Message));
        }
    }
}
