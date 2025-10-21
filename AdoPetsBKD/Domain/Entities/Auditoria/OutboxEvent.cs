using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Auditoria;

/// <summary>
/// Patrón Outbox para eventos internos (notificaciones, KPIs)
/// </summary>
public class OutboxEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int Attempts { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastAttemptAt { get; set; }

    public bool IsProcessed => ProcessedAt.HasValue;

    public void MarcarComoProcesado()
    {
        ProcessedAt = DateTime.UtcNow;
    }

    public void RegistrarIntento(string? error = null)
    {
        Attempts++;
        LastAttemptAt = DateTime.UtcNow;
        ErrorMessage = error;
    }

    public bool PuedeReintentar(int maxAttempts = 5)
    {
        return Attempts < maxAttempts && !IsProcessed;
    }
}
