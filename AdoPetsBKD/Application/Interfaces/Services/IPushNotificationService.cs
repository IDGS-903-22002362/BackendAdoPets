namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IPushNotificationService
{
    /// <summary>
    /// Enviar notificación push a un usuario específico
    /// </summary>
    Task EnviarNotificacionAsync(Guid usuarioId, string titulo, string mensaje, Dictionary<string, string>? data = null);
    
    /// <summary>
    /// Enviar notificación push a múltiples usuarios
    /// </summary>
    Task EnviarNotificacionMulticastAsync(List<Guid> usuariosIds, string titulo, string mensaje, Dictionary<string, string>? data = null);
    
    /// <summary>
    /// Enviar notificación a un topic de Firebase (para notificaciones masivas)
    /// </summary>
    Task EnviarNotificacionATopicAsync(string topic, string titulo, string mensaje, Dictionary<string, string>? data = null);
    
    /// <summary>
    /// Suscribir tokens de dispositivos a un topic
    /// </summary>
    Task SuscribirATopicAsync(List<string> tokens, string topic);
}
