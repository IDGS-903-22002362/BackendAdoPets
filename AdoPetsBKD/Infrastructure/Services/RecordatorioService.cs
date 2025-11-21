using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Domain.Entities.Security;
using Microsoft.Extensions.Logging;

namespace AdoPetsBKD.Infrastructure.Services;

public class RecordatorioService : IRecordatorioService
{
    private readonly ICitaRepository _citaRepository;
    private readonly IEmailService _emailService;
    private readonly IPushNotificationService _pushService;
    private readonly INotificacionRepository _notificacionRepository;
    private readonly ILogger<RecordatorioService> _logger;

    public RecordatorioService(
        ICitaRepository citaRepository,
        IEmailService emailService,
        IPushNotificationService pushService,
        INotificacionRepository notificacionRepository,
        ILogger<RecordatorioService> logger)
    {
        _citaRepository = citaRepository;
        _emailService = emailService;
        _pushService = pushService;
        _notificacionRepository = notificacionRepository;
        _logger = logger;
    }

    public async Task EnviarRecordatoriosPendientesAsync()
    {
        try
        {
            _logger.LogInformation("?? Iniciando proceso de envío de recordatorios automáticos");
            
            var citasConRecordatorios = await _citaRepository.GetPendingRemindersAsync();
            
            if (!citasConRecordatorios.Any())
            {
                _logger.LogInformation("? No hay recordatorios pendientes");
                return;
            }

            _logger.LogInformation(
                "?? Procesando {Count} citas con recordatorios pendientes",
                citasConRecordatorios.Count
            );

            var recordatoriosEnviados = 0;

            foreach (var cita in citasConRecordatorios)
            {
                var enviados = await ProcesarRecordatoriosCitaAsync(cita);
                recordatoriosEnviados += enviados;
            }

            _logger.LogInformation(
                "? Proceso completado. {Count} recordatorios enviados",
                recordatoriosEnviados
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al procesar recordatorios automáticos");
            throw;
        }
    }

    public async Task ProgramarRecordatoriosParaCitaAsync(Guid citaId)
    {
        try
        {
            var cita = await _citaRepository.GetByIdAsync(citaId);
            if (cita == null)
            {
                _logger.LogWarning("Cita {CitaId} no encontrada", citaId);
                return;
            }

            // Crear recordatorios automáticos
            cita.Recordatorios = new List<CitaRecordatorio>
            {
                new() { Tipo = TipoRecordatorio.Horas24 },
                new() { Tipo = TipoRecordatorio.Horas2 },
                new() { Tipo = TipoRecordatorio.Hora1 }
            };

            await _citaRepository.UpdateAsync(cita);

            _logger.LogInformation(
                "? Recordatorios programados para cita {CitaId}",
                citaId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al programar recordatorios para cita {CitaId}", citaId);
            throw;
        }
    }

    private async Task<int> ProcesarRecordatoriosCitaAsync(Cita cita)
    {
        var now = DateTime.UtcNow;
        var enviados = 0;

        foreach (var recordatorio in cita.Recordatorios.Where(r => !r.WasSent))
        {
            var tiempoAntes = ObtenerTiempoAntes(recordatorio.Tipo);
            var tiempoRestante = cita.StartAt - now;

            // Verificar si es momento de enviar el recordatorio
            // Se envía cuando el tiempo restante es menor o igual al tiempo programado
            // y mayor a cero (aún no ha pasado la cita)
            if (tiempoRestante <= tiempoAntes && tiempoRestante > TimeSpan.Zero)
            {
                try
                {
                    await EnviarRecordatorioAsync(cita, recordatorio, tiempoRestante);
                    
                    // Marcar como enviado
                    recordatorio.SentAt = DateTime.UtcNow;
                    await _citaRepository.UpdateAsync(cita);

                    enviados++;

                    _logger.LogInformation(
                        "?? Recordatorio enviado: CitaId={CitaId}, Tipo={Tipo}, Usuario={UsuarioEmail}",
                        cita.Id,
                        recordatorio.Tipo,
                        cita.Propietario?.Email ?? "Sin propietario"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "? Error al enviar recordatorio: CitaId={CitaId}, Tipo={Tipo}",
                        cita.Id,
                        recordatorio.Tipo
                    );
                }
            }
        }

        return enviados;
    }

    private async Task EnviarRecordatorioAsync(
        Cita cita,
        CitaRecordatorio recordatorio,
        TimeSpan tiempoRestante)
    {
        var titulo = "?? Recordatorio de Cita Veterinaria";
        var mensaje = GenerarMensajeRecordatorio(cita, tiempoRestante);

        // 1. Enviar notificación in-app
        if (cita.PropietarioId.HasValue)
        {
            var notificacion = new Notificacion
            {
                UsuarioId = cita.PropietarioId.Value,
                Tipo = TipoNotificacion.AppointmentReminder,
                Titulo = titulo,
                Mensaje = mensaje,
                DataJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    CitaId = cita.Id,
                    FechaHora = cita.StartAt,
                    Veterinario = $"{cita.Veterinario.Nombre} {cita.Veterinario.ApellidoPaterno}",
                    Sala = cita.Sala?.Nombre,
                    TipoRecordatorio = recordatorio.Tipo.ToString()
                }),
                Fecha = DateTime.UtcNow
            };

            await _notificacionRepository.AddAsync(notificacion);
        }

        // 2. Enviar email
        if (!string.IsNullOrEmpty(cita.Propietario?.Email))
        {
            try
            {
                await _emailService.EnviarRecordatorioCitaAsync(
                    cita.Propietario.Email,
                    $"{cita.Propietario.Nombre} {cita.Propietario.ApellidoPaterno}",
                    cita,
                    tiempoRestante
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email para cita {CitaId}", cita.Id);
            }
        }

        // 3. Enviar push notification
        if (cita.PropietarioId.HasValue)
        {
            try
            {
                await _pushService.EnviarNotificacionAsync(
                    cita.PropietarioId.Value,
                    titulo,
                    mensaje,
                    new Dictionary<string, string>
                    {
                        ["tipo"] = "recordatorio_cita",
                        ["citaId"] = cita.Id.ToString(),
                        ["fechaHora"] = cita.StartAt.ToString("o")
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar push para cita {CitaId}", cita.Id);
            }
        }
    }

    private static TimeSpan ObtenerTiempoAntes(TipoRecordatorio tipo)
    {
        return tipo switch
        {
            TipoRecordatorio.Horas24 => TimeSpan.FromHours(24),
            TipoRecordatorio.Horas2 => TimeSpan.FromHours(2),
            TipoRecordatorio.Hora1 => TimeSpan.FromHours(1),
            _ => TimeSpan.Zero
        };
    }

    private static string GenerarMensajeRecordatorio(Cita cita, TimeSpan tiempoRestante)
    {
        var tiempo = FormatearTiempoRestante(tiempoRestante);
        var veterinario = $"{cita.Veterinario.Nombre} {cita.Veterinario.ApellidoPaterno}";
        var fecha = cita.StartAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        var mensaje = $"Tienes una cita {tiempo} con el Dr. {veterinario} el {fecha}.";
        
        if (cita.Mascota != null)
        {
            mensaje += $" Mascota: {cita.Mascota.Nombre}";
        }

        if (cita.Sala != null)
        {
            mensaje += $" - Sala: {cita.Sala.Nombre}";
        }

        return mensaje;
    }

    private static string FormatearTiempoRestante(TimeSpan tiempo)
    {
        if (tiempo.TotalHours >= 23)
            return "en 24 horas";
        if (tiempo.TotalHours >= 1.5)
            return "en 2 horas";
        if (tiempo.TotalHours >= 0.5)
            return "en 1 hora";
        
        return $"en {(int)tiempo.TotalMinutes} minutos";
    }
}
