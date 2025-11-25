using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Chat;

/// <summary>
/// Representa un mensaje individual dentro de una conversación de chat.
/// Puede ser del usuario ("user"), del asistente ("assistant") o del sistema ("system").
/// </summary>
public class ChatMessage : BaseEntity
{
    /// <summary>
    /// ID de la conversación a la que pertenece este mensaje
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Rol del emisor: "user", "assistant", "system"
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Contenido del mensaje
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del mensaje
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navegación a la conversación padre
    /// </summary>
    public ChatConversation Conversation { get; set; } = null!;
}
