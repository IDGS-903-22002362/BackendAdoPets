namespace AdoPetsBKD.Application.DTOs.Clinica;

public class CreatePagoDto
{
    public Guid? UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "MXN";
    public int Tipo { get; set; } // TipoPago
    public int Metodo { get; set; } // MetodoPago
    public string? Concepto { get; set; }
    public string? Referencia { get; set; }
    public Guid? CitaId { get; set; }
    public Guid? TicketId { get; set; }
    public bool EsAnticipo { get; set; }
    public decimal? MontoTotal { get; set; }
}

public class CreatePagoPayPalDto
{
    public Guid? UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public Guid? SolicitudCitaId { get; set; }
    public Guid? CitaId { get; set; }
    public bool EsAnticipo { get; set; }
    public decimal? MontoTotal { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class PagoDto
{
    public Guid Id { get; set; }
    public string NumeroPago { get; set; } = string.Empty;
    public Guid? UsuarioId { get; set; }
    public string? NombreUsuario { get; set; }
    public decimal Monto { get; set; }
    public string Moneda { get; set; } = string.Empty;
    public int Tipo { get; set; }
    public string TipoNombre { get; set; } = string.Empty;
    public int Metodo { get; set; }
    public string MetodoNombre { get; set; } = string.Empty;
    public int Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public string? PayPalOrderId { get; set; }
    public string? PayPalCaptureId { get; set; }
    public string? PayPalPayerEmail { get; set; }
    public string? PayPalPayerName { get; set; }
    public DateTime? FechaPago { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string? Concepto { get; set; }
    public string? Referencia { get; set; }
    public Guid? CitaId { get; set; }
    public Guid? TicketId { get; set; }
    public bool EsAnticipo { get; set; }
    public decimal? MontoTotal { get; set; }
    public decimal? MontoRestante { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayPalOrderResponseDto
{
    public string OrderId { get; set; } = string.Empty;
    public string ApprovalUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class CapturePayPalPaymentDto
{
    public string OrderId { get; set; } = string.Empty;
}

public class PayPalWebhookDto
{
    public string EventType { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public object Resource { get; set; } = null!;
}
