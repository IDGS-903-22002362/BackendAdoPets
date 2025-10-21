using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Domain.Entities.Donaciones;

/// <summary>
/// Donación realizada vía PayPal
/// </summary>
public class Donacion : AuditableEntity
{
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "MXN";
    public StatusDonacion Status { get; set; } = StatusDonacion.PENDING;

    public string PaypalOrderId { get; set; } = string.Empty;
    public string? PaypalCaptureId { get; set; }
    public string? PayerEmail { get; set; }
    public string? PayerName { get; set; }
    public string? PaypalPayerId { get; set; }

    public SourceDonacion Source { get; set; } = SourceDonacion.Checkout;
    public string? Mensaje { get; set; }
    public bool Anonima { get; set; }

    public DateTime? CapturedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public void Capturar(string captureId, string? payerEmail = null, string? payerName = null)
    {
        Status = StatusDonacion.CAPTURED;
        PaypalCaptureId = captureId;
        PayerEmail = payerEmail;
        PayerName = payerName;
        CapturedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancelar(string? reason = null)
    {
        Status = StatusDonacion.CANCELLED;
        CancellationReason = reason;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fallar(string? reason = null)
    {
        Status = StatusDonacion.FAILED;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum StatusDonacion
{
    PENDING = 1,
    CAPTURED = 2,
    CANCELLED = 3,
    FAILED = 4,
    REFUNDED = 5
}

public enum SourceDonacion
{
    Checkout = 1,
    Webhook = 2,
    Manual = 3
}
