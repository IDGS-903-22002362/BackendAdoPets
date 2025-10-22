using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Sistema de pagos con integración a PayPal
/// Soporta pagos completos y anticipos (50% para citas)
/// </summary>
public class Pago : AuditableEntity
{
    public string NumeroPago { get; set; } = string.Empty; // Formato: PAY-YYYYMMDD-XXXX
    
    public Guid? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public decimal Monto { get; set; }
    public string Moneda { get; set; } = "MXN";
    public TipoPago Tipo { get; set; }
    public MetodoPago Metodo { get; set; }

    // Estado del pago
    public EstadoPago Estado { get; set; } = EstadoPago.Pendiente;

    // Información de PayPal
    public string? PayPalOrderId { get; set; }
    public string? PayPalCaptureId { get; set; }
    public string? PayPalPayerId { get; set; }
    public string? PayPalPayerEmail { get; set; }
    public string? PayPalPayerName { get; set; }

    // Timestamps
    public DateTime? FechaPago { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public DateTime? FechaCancelacion { get; set; }

    // Información adicional
    public string? Concepto { get; set; }
    public string? Referencia { get; set; }
    public string? Notas { get; set; }

    // Referencias
    public Guid? CitaId { get; set; }
    public Cita? Cita { get; set; }

    public Guid? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    // Para anticipos
    public bool EsAnticipo { get; set; }
    public decimal? MontoTotal { get; set; } // Monto total del servicio
    public decimal? MontoRestante { get; set; } // Monto que falta por pagar
    public Guid? PagoPrincipalId { get; set; } // Si es pago complementario
    public Pago? PagoPrincipal { get; set; }
    public ICollection<Pago> PagosComplementarios { get; set; } = new List<Pago>();

    public void MarcarComoPagado(string paypalCaptureId, string? payerEmail = null, string? payerName = null)
    {
        Estado = EstadoPago.Completado;
        PayPalCaptureId = paypalCaptureId;
        PayPalPayerEmail = payerEmail;
        PayPalPayerName = payerName;
        FechaPago = DateTime.UtcNow;
        FechaConfirmacion = DateTime.UtcNow;

        if (EsAnticipo && MontoTotal.HasValue)
        {
            MontoRestante = MontoTotal.Value - Monto;
        }
    }

    public void Cancelar(string? motivo = null)
    {
        Estado = EstadoPago.Cancelado;
        FechaCancelacion = DateTime.UtcNow;
        Notas = motivo ?? Notas;
    }

    public void Reembolsar()
    {
        Estado = EstadoPago.Reembolsado;
        FechaCancelacion = DateTime.UtcNow;
    }

    public string GenerarNumeroPago()
    {
        var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"PAY-{fecha}-{random}";
    }
}

public enum TipoPago
{
    PagoCompleto = 1,
    Anticipo = 2,
    PagoComplementario = 3,
    Reembolso = 4
}

public enum MetodoPago
{
    PayPal = 1,
    Efectivo = 2,
    TarjetaDebito = 3,
    TarjetaCredito = 4,
    Transferencia = 5
}

public enum EstadoPago
{
    Pendiente = 1,
    Procesando = 2,
    Completado = 3,
    Fallido = 4,
    Cancelado = 5,
    Reembolsado = 6
}
