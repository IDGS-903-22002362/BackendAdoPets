namespace AdoPetsBKD.Application.DTOs.Donaciones
{
    public class DonacionDto
    {
        public Guid Id { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? NombreUsuario { get; set; }

        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public int Status { get; set; }
        public string StatusNombre { get; set; } = string.Empty;
        public int Source { get; set; }
        public string SourceNombre { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
        public bool Anonima { get; set; } = false;
        
        public string? PayPalOrderId { get; set; }
        public string? PayPalCaptureId { get; set; }
        public string? PayPalPayerEmail { get; set; }
        public string? PayPalPayerName { get; set; }
        
        public DateTime? CapturedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
