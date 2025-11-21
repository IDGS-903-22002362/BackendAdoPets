using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdoPetsBKD.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly IDispositivoRepository _dispositivoRepository;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly bool _firebaseEnabled;

    public PushNotificationService(
        IDispositivoRepository dispositivoRepository,
        ILogger<PushNotificationService> logger,
        IConfiguration configuration)
    {
        _dispositivoRepository = dispositivoRepository;
        _logger = logger;

        // Inicializar Firebase Admin (solo una vez)
        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialsPath = configuration["Firebase:CredentialsPath"] ?? "firebase-adminsdk.json";
                
                if (File.Exists(credentialsPath))
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialsPath)
                    });
                    
                    _firebaseEnabled = true;
                    _logger.LogInformation("? Firebase Admin SDK inicializado correctamente");
                }
                else
                {
                    _firebaseEnabled = false;
                    _logger.LogWarning("?? Archivo de credenciales Firebase no encontrado en: {Path}. Push notifications deshabilitadas.", credentialsPath);
                }
            }
            else
            {
                _firebaseEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _firebaseEnabled = false;
            _logger.LogError(ex, "? Error al inicializar Firebase Admin SDK. Push notifications deshabilitadas.");
        }
    }

    public async Task EnviarNotificacionAsync(
        Guid usuarioId,
        string titulo,
        string mensaje,
        Dictionary<string, string>? data = null)
    {
        if (!_firebaseEnabled)
        {
            _logger.LogWarning("Push notifications deshabilitadas. Mensaje no enviado a usuario {UsuarioId}", usuarioId);
            return;
        }

        try
        {
            var dispositivos = await _dispositivoRepository.GetByUsuarioIdAsync(usuarioId);
            var dispositivosActivos = dispositivos.Where(d => d.Enabled).ToList();

            if (!dispositivosActivos.Any())
            {
                _logger.LogWarning("No hay dispositivos activos para el usuario {UsuarioId}", usuarioId);
                return;
            }

            var tokens = dispositivosActivos.Select(d => d.Token).ToList();

            _logger.LogInformation(
                "?? Enviando push notification a {Count} dispositivos del usuario {UsuarioId}",
                tokens.Count,
                usuarioId
            );

            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje,
                    ImageUrl = null // Puedes agregar una URL de imagen aquí
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ChannelId = "citas_recordatorios",
                        Color = "#4CAF50", // Verde de AdoPets
                        Icon = "ic_notification",
                        Tag = "recordatorio_cita",
                        ClickAction = "FLUTTER_NOTIFICATION_CLICK"
                    },
                    TimeToLive = TimeSpan.FromHours(24)
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1,
                        MutableContent = true,
                        ContentAvailable = true,
                        Category = "RECORDATORIO_CITA"
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["apns-priority"] = "10",
                        ["apns-expiration"] = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds().ToString()
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            _logger.LogInformation(
                "? Push notifications enviadas: {SuccessCount}/{TotalCount} exitosas",
                response.SuccessCount,
                tokens.Count
            );

            // Deshabilitar tokens que fallaron
            if (response.FailureCount > 0)
            {
                await DeshabilitarTokensInvalidosAsync(response, dispositivosActivos);
            }

            // Actualizar última vista de dispositivos exitosos
            foreach (var dispositivo in dispositivosActivos.Where((d, i) => response.Responses[i].IsSuccess))
            {
                dispositivo.ActualizarUltimaVista();
                await _dispositivoRepository.UpdateAsync(dispositivo);
            }
        }
        catch (FirebaseMessagingException fmEx)
        {
            _logger.LogError(
                fmEx,
                "? Error de Firebase Messaging al enviar push al usuario {UsuarioId}: {ErrorCode}",
                usuarioId,
                fmEx.MessagingErrorCode
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al enviar push notification al usuario {UsuarioId}", usuarioId);
        }
    }

    public async Task EnviarNotificacionMulticastAsync(
        List<Guid> usuariosIds,
        string titulo,
        string mensaje,
        Dictionary<string, string>? data = null)
    {
        _logger.LogInformation(
            "?? Enviando push notification masiva a {Count} usuarios",
            usuariosIds.Count
        );

        var tasks = usuariosIds.Select(usuarioId => 
            EnviarNotificacionAsync(usuarioId, titulo, mensaje, data)
        );

        await Task.WhenAll(tasks);
    }

    private async Task DeshabilitarTokensInvalidosAsync(
        BatchResponse response,
        List<Domain.Entities.Security.Dispositivo> dispositivos)
    {
        for (int i = 0; i < response.Responses.Count; i++)
        {
            if (!response.Responses[i].IsSuccess)
            {
                var errorCode = response.Responses[i].Exception?.MessagingErrorCode;
                
                // Tokens inválidos o no registrados deben ser deshabilitados
                if (errorCode == MessagingErrorCode.InvalidArgument ||
                    errorCode == MessagingErrorCode.Unregistered ||
                    errorCode == MessagingErrorCode.SenderIdMismatch)
                {
                    var dispositivo = dispositivos[i];
                    dispositivo.Deshabilitar();
                    await _dispositivoRepository.UpdateAsync(dispositivo);
                    
                    _logger.LogWarning(
                        "?? Token FCM deshabilitado: DispositivoId={DispositivoId}, Error={Error}",
                        dispositivo.Id,
                        errorCode
                    );
                }
                else if (errorCode == MessagingErrorCode.QuotaExceeded)
                {
                    _logger.LogError("? Cuota de Firebase excedida. No se enviaron más notificaciones.");
                    break;
                }
                else
                {
                    _logger.LogWarning(
                        "?? Error al enviar a dispositivo {DispositivoId}: {Error}",
                        dispositivos[i].Id,
                        errorCode
                    );
                }
            }
        }
    }

    /// <summary>
    /// Enviar notificación a un topic específico (para notificaciones masivas)
    /// </summary>
    public async Task EnviarNotificacionATopicAsync(
        string topic,
        string titulo,
        string mensaje,
        Dictionary<string, string>? data = null)
    {
        if (!_firebaseEnabled)
        {
            _logger.LogWarning("Push notifications deshabilitadas. Mensaje no enviado al topic {Topic}", topic);
            return;
        }

        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = mensaje
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        Sound = "default",
                        ChannelId = "general",
                        Color = "#4CAF50"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        Badge = 1
                    }
                }
            };

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            
            _logger.LogInformation(
                "? Notificación enviada al topic {Topic}. MessageId: {MessageId}",
                topic,
                messageId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al enviar notificación al topic {Topic}", topic);
        }
    }

    /// <summary>
    /// Suscribir dispositivos a un topic
    /// </summary>
    public async Task SuscribirATopicAsync(List<string> tokens, string topic)
    {
        if (!_firebaseEnabled)
        {
            _logger.LogWarning("Push notifications deshabilitadas. No se puede suscribir al topic {Topic}", topic);
            return;
        }

        try
        {
            var response = await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(tokens, topic);
            
            _logger.LogInformation(
                "? Dispositivos suscritos al topic {Topic}: {SuccessCount}/{TotalCount}",
                topic,
                response.SuccessCount,
                tokens.Count
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al suscribir dispositivos al topic {Topic}", topic);
        }
    }
}
