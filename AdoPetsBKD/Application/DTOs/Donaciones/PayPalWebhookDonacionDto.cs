namespace AdoPetsBKD.Application.DTOs.Donaciones
{
    public class PayPalWebhookDonacionDto
    {
        public string EventType { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public object Resource { get; set; } = null!;
    }
}
