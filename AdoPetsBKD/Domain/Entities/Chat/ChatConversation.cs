using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Chat;

/// <summary>
/// Representa una conversación de chat entre un usuario y el asistente virtual.
/// Diseñada específicamente para usuarios con rol Adoptante.
/// </summary>
public class ChatConversation : BaseEntity
{
    /// <summary>
    /// ID del usuario que participa en la conversación
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de creación de la conversación
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Mensajes de la conversación
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
