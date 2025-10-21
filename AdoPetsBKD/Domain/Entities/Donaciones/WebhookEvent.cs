using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Donaciones;

/// <summary>
/// Evento de webhook (PayPal u otros servicios)
/// </summary>
public class WebhookEvent : BaseEntity
{
    public ProviderWebhook Provider { get; set; } = ProviderWebhook.PayPal;
    public string EventId { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public StatusWebhook Status { get; set; } = StatusWebhook.Pending;
    
    public int Retries { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastRetryAt { get; set; }

    public void MarcarComoProcesado()
    {
        Status = StatusWebhook.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarcarComoFallido(string errorMessage)
    {
        Status = StatusWebhook.Failed;
        ErrorMessage = errorMessage;
        LastRetryAt = DateTime.UtcNow;
        Retries++;
    }

    public void Reintentar()
    {
        LastRetryAt = DateTime.UtcNow;
        Retries++;
    }

    public bool PuedeReintentar(int maxRetries = 3)
    {
        return Retries < maxRetries && Status == StatusWebhook.Failed;
    }
}

public enum ProviderWebhook
{
    PayPal = 1,
    Stripe = 2,
    MercadoPago = 3
}

public enum StatusWebhook
{
    Pending = 1,
    Processed = 2,
    Failed = 3,
    Duplicate = 4
}
