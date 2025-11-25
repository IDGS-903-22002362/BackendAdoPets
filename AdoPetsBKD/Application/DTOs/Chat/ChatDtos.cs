namespace AdoPetsBKD.Application.DTOs.Chat;

/// <summary>
/// DTO para solicitar una respuesta del chatbot
/// </summary>
public class ChatAskRequestDto
{
    /// <summary>
    /// ID del usuario autenticado
    /// Por ahora se envía en el body, idealmente se obtendría del token JWT
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Mensaje del usuario
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// ID de la conversación existente (null para crear una nueva)
    /// </summary>
    public Guid? ConversationId { get; set; }
}

/// <summary>
/// DTO de respuesta del chatbot
/// </summary>
public class ChatAskResponseDto
{
    /// <summary>
    /// ID de la conversación (nueva o existente)
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Respuesta del asistente
    /// </summary>
    public string Answer { get; set; } = string.Empty;
}
