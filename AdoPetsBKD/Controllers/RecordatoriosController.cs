using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RecordatoriosController : ControllerBase
{
    private readonly IRecordatorioService _recordatorioService;
    private readonly ILogger<RecordatoriosController> _logger;

    public RecordatoriosController(
        IRecordatorioService recordatorioService,
        ILogger<RecordatoriosController> logger)
    {
        _recordatorioService = recordatorioService;
        _logger = logger;
    }

    /// <summary>
    /// Ejecutar manualmente el job de recordatorios (solo para testing)
    /// </summary>
    [HttpPost("ejecutar-ahora")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<IActionResult> EjecutarRecordatoriosAhora()
    {
        try
        {
            // TODO: Descomentar cuando Hangfire esté instalado
            // BackgroundJob.Enqueue<IRecordatorioService>(
            //     service => service.EnviarRecordatoriosPendientesAsync()
            // );

            // Por ahora ejecutar directamente
            await _recordatorioService.EnviarRecordatoriosPendientesAsync();

            return Ok(ApiResponse<string>.SuccessResponse(
                "El proceso se ha completado",
                "Job de recordatorios ejecutado exitosamente"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar job de recordatorios");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al ejecutar job de recordatorios"));
        }
    }

    /// <summary>
    /// Programar recordatorios para una cita específica
    /// </summary>
    [HttpPost("programar/{citaId}")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<IActionResult> ProgramarRecordatoriosParaCita(Guid citaId)
    {
        try
        {
            await _recordatorioService.ProgramarRecordatoriosParaCitaAsync(citaId);

            return Ok(ApiResponse<string>.SuccessResponse(
                $"Se han programado recordatorios para la cita {citaId}",
                "Recordatorios programados exitosamente"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al programar recordatorios para cita {CitaId}", citaId);
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error al programar recordatorios"));
        }
    }

    /// <summary>
    /// Obtener información del job de recordatorios
    /// </summary>
    [HttpGet("info")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetJobInfo()
    {
        var jobInfo = new
        {
            JobId = "enviar-recordatorios-citas",
            Frecuencia = "Cada 15 minutos",
            CronExpression = "*/15 * * * *",
            TimeZone = "Central Standard Time (Mexico)",
            DashboardUrl = "/hangfire",
            Status = "Configurado (requiere instalación de Hangfire)"
        };

        return Ok(ApiResponse<object>.SuccessResponse(
            jobInfo,
            "Información del job de recordatorios"
        ));
    }
}
