using System.ComponentModel.DataAnnotations;

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
    public string Token { get; set; } = string.Empty; // Token de PayPal que se usa en la URL
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

/// <summary>
/// DTO para mostrar información de pagos pendientes de citas
/// </summary>
public class PagoPendienteDto
{
    public Guid CitaId { get; set; }
    public string? NumeroCita { get; set; }
    public DateTime FechaCita { get; set; }
    public string? NombreMascota { get; set; }
    public string? NombrePropietario { get; set; }
    public Guid? PropietarioId { get; set; }
    public string? ServicioDescripcion { get; set; }

    // Información del pago anticipado (si existe)
    public Guid? PagoAnticipoId { get; set; }
    public decimal? MontoAnticipoPagado { get; set; }
    public DateTime? FechaPagoAnticipo { get; set; }

    // Información del monto pendiente
    public decimal MontoTotal { get; set; }
    public decimal MontoPendiente { get; set; }
    public decimal PorcentajePagado { get; set; }

    // Estado
    public bool TieneAnticipoPagado { get; set; }
    public string EstadoPago { get; set; } = string.Empty; // "Anticipo Pagado", "Pendiente Total", "Pagado Completo"
}

/// <summary>
/// DTO para completar el pago restante de una cita
/// </summary>
public class CompletarPagoRestanteDto
{
    [Required(ErrorMessage = "El ID de la cita es requerido")]
    public Guid CitaId { get; set; }

    public Guid? PagoAnticipoId { get; set; }

    [Required(ErrorMessage = "El método de pago es requerido")]
    [Range(2, 5, ErrorMessage = "El método de pago debe ser Efectivo (2), Tarjeta Débito (3), Tarjeta Crédito (4) o Transferencia (5)")]
    public int MetodoPago { get; set; } // 2=Efectivo, 3=TarjetaDebito, 4=TarjetaCredito, 5=Transferencia (NO PayPal=1)

    public string? Referencia { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para crear orden PayPal para pago restante
/// </summary>
public class CompletarPagoRestantePayPalDto
{
    [Required(ErrorMessage = "El ID de la cita es requerido")]
    public Guid CitaId { get; set; }

    public Guid? PagoAnticipoId { get; set; }

    [Required(ErrorMessage = "La URL de retorno es requerida")]
    public string ReturnUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "La URL de cancelación es requerida")]
    public string CancelUrl { get; set; } = string.Empty;
}

/// <summary>
/// DTO para crear un pago completo (100%) con PayPal para citas presenciales
/// </summary>
public class CrearPagoCompletoPayPalDto
{
    [Required(ErrorMessage = "El ID de la cita es requerido")]
    public Guid CitaId { get; set; }
    
    public Guid? UsuarioId { get; set; }
    
    [Required(ErrorMessage = "El monto es requerido")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
    public decimal Monto { get; set; }
    
    public string? Concepto { get; set; }
    
    [Required(ErrorMessage = "La URL de retorno es requerida")]
    public string ReturnUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La URL de cancelación es requerida")]
    public string CancelUrl { get; set; } = string.Empty;
}
