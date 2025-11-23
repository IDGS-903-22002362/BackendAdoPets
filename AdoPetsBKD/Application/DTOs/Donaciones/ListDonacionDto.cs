namespace AdoPetsBKD.Application.DTOs.Donaciones
{
    public class ListDonacionDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }
        public decimal Monto { get; set; } = 0;
        public string Moneda { get; set; } = "MXN";
        public int Status { get; set; }
        public int Source { get; set; }
        public string? Mensaje { get; set; }
        public bool Anonima { get; set; } = false;
        public string? PayPalOrderId { get; set; }
        public string? PayPalCaptureId { get; set; }
        public string? PayPalPayerEmail { get; set; }
        public string? PayPalPayerName { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }
    }
}
