using AdoPetsBKD.Application.DTOs.Donaciones;

namespace AdoPetsBKD.Application.Interfaces.Services
{
    public interface IDonacionesService
    {
        Task<DonacionDto> CreateDonacionAsync(CreateDonacionDto dto, Guid createdBy);
        Task<PayPalDonacionResponseDto> CreatePayPayDonacionAsync(CreateDonacionDto dto, Guid createdBy);
        Task<DonacionDto> CapturePayPalDonacionAsync(string orderId);
        Task<DonacionDto?> GetDonacionByIdAsync(Guid id);
        Task<DonacionDto?> GetDonacionByPayPalOrderIdAsync(string paypalOrderId);
        Task<List<DonacionDto>> GetDonacionesByUsuarioAsync(Guid usuarioId);
        Task<List<DonacionDto>> GetDonacionesAsync(int pageNumber = 1, int pageSize = 10, FiltroDonacionAnonima filtro = FiltroDonacionAnonima.SoloPublicas);
        Task<DonacionDto> CancelarDonacionAsync(Guid donacionId, Guid canceladoPorId, string? motivo = null);
        Task ProcessWebhookAsync(PayPalWebhookDonacionDto webhook);
    }

    public enum FiltroDonacionAnonima
    {
        Todas = 0,
        SoloPublicas = 1,
        SoloAnonimas = 2
    }
}
