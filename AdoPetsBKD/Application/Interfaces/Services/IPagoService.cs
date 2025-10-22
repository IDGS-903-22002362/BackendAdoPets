using AdoPetsBKD.Application.DTOs.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IPagoService
{
    Task<PagoDto> CreatePagoAsync(CreatePagoDto dto, Guid createdBy);
    Task<PayPalOrderResponseDto> CreatePayPalOrderAsync(CreatePagoPayPalDto dto, Guid createdBy);
    Task<PagoDto> CapturePayPalPaymentAsync(string orderId);
    Task<PagoDto?> GetPagoByIdAsync(Guid id);
    Task<PagoDto?> GetPagoByPayPalOrderIdAsync(string paypalOrderId);
    Task<List<PagoDto>> GetPagosByUsuarioAsync(Guid usuarioId);
    Task<PagoDto> CancelarPagoAsync(Guid pagoId, Guid canceladoPorId, string? motivo = null);
    Task ProcessWebhookAsync(PayPalWebhookDto webhook);
}
