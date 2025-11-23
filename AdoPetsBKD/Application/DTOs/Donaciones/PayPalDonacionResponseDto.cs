namespace AdoPetsBKD.Application.DTOs.Donaciones
{
    public class PayPalDonacionResponseDto
    {
        public Guid DonacionId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
