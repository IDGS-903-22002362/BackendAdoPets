namespace AdoPetsBKD.Application.DTOs.Clinica;

/// <summary>
/// DTO para la respuesta de captura de PayPal con información extraída manualmente
/// </summary>
public class PayPalCaptureResponseDto
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? CaptureId { get; set; }
    public string? CaptureStatus { get; set; }
    public string? PayerEmail { get; set; }
    public string? PayerName { get; set; }
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}
