using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AdoPetsBKD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdopcionController : ControllerBase
    {
        private readonly IUMascotaService _mascotaService;
        private readonly ILogger<MascotaController> _logger;

        public AdopcionController(IUMascotaService mascotaService, ILogger<MascotaController> logger)
        {
            _mascotaService = mascotaService;
            _logger = logger;
        }

        [HttpPost("crear-solicitud")]
        public async Task<IActionResult> CrearSolicitud([FromBody] CreateSolicitudeAdopcionDto dto)
        {
            try
            {
                var result = await _mascotaService.CrearSolicitudAdopcionAsync(dto);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear solicitud de adopción.");
                return StatusCode(500, new { message = "Error interno del servidor al crear la solicitud." });
            }
        }

        [HttpGet("{solicitudId}")]
        public async Task<IActionResult> GetSolicitudById(Guid solicitudId)
        {
            var solicitud = await _mascotaService.GetSolicitudAdopcionByIdAsync(solicitudId);
            if (solicitud == null)
                return NotFound(new { message = "Solicitud no encontrada" });

            // Aplicar la misma conversión de URLs
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            if (solicitud.MascotaFotos != null)
            {
                foreach (var foto in solicitud.MascotaFotos)
                {
                    if (!string.IsNullOrEmpty(foto.StorageKey))
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

            return Ok(solicitud);
        }

        [HttpGet("solicitud")]
        public async Task<IActionResult> GetAllSolicitudes()
        {
            var solicitudes = await _mascotaService.GetAllSolicitudesAdopcionAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            foreach (var solicitud in solicitudes)
            {
                if (solicitud.MascotaFotos != null)
                {
                    foreach (var foto in solicitud.MascotaFotos)
                    {
                        if (!string.IsNullOrEmpty(foto.StorageKey))
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

            return Ok(solicitudes);
        }
        // Para actualizar el estado de una adopción a EnRevisión 
        [HttpPut("solicitudes/EnRevision")]
        public async Task<ActionResult<CambiarEstadoSolicitudEnRevisionDto>> CambiarEstadoSolicitudR(
            
            [FromBody] CambiarEstadoSolicitudEnRevisionDto dto)
        {

            var resultado = await _mascotaService.UpdateStatusSolicitudAdopcionAsync(dto);
            return Ok(new {message = "La solicitud de adopción está en revisión." });
        }


        [HttpPut("solicitudes/Aceptada")]
        public async Task<ActionResult<EstadoSolicitudeAceptadaDto>> CambiarEstadoSolicitudA([FromBody] EstadoSolicitudeAceptadaDto dtoA)
        {

            var resultado = await _mascotaService.UpdateStatusSolicitudAprobadaAsync(dtoA);
            return Ok(new {message = "La solicitud de adopción se aprobó." });
        }



        [HttpPut("solicitudes/Rechazada")]
        public async Task<ActionResult<SolicirudRechasadaDto>> CambiarEstadoSolicitudR( [FromBody] SolicirudRechasadaDto dtoA)
        {

            var resultado = await _mascotaService.UpdateStatusSolicitudRechazadaAsync(dtoA);
            return Ok(new { message = "La solicitud de adopción se Rechazo." });
        }

        [HttpPut("solicitudes/Cancelada")]
        public async Task<ActionResult<CancelarSolicitudDto>> CambiarEstadoSolicitudC([FromBody] CancelarSolicitudDto dtoC)
        {
            var resultado = await _mascotaService.UpdateStatusSolicitudCanceladaAsync(dtoC);
            return Ok(new { message = "La solicitud de adopción se Cancelo." });
        }


        [HttpGet("Solicitud/{usuarioId}")]
        public async Task<IActionResult> GetSolicitudbyUsuarioId(Guid usuarioId)
        {
            var solicitud = await _mascotaService.GetSolicitudbyUsuarioIdAsync(usuarioId);
            if (solicitud == null)
                return NotFound(new { message = "Solicitud no encontrada para el usuario especificado" });
            return Ok(solicitud);
        }
    }
}