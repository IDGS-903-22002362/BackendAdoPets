using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Ticket que se entrega al cliente después de un procedimiento
/// Contiene detalles del procedimiento, costos y total
/// </summary>
public class Ticket : AuditableEntity
{
    public string NumeroTicket { get; set; } = string.Empty; // Formato: TK-YYYYMMDD-XXXX
    public Guid CitaId { get; set; }
    public Cita Cita { get; set; } = null!;

    public Guid? MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public Guid ClienteId { get; set; }
    public Usuario Cliente { get; set; } = null!;

    public Guid VeterinarioId { get; set; }
    public Usuario Veterinario { get; set; } = null!;

    public DateTime FechaProcedimiento { get; set; }
    public string NombreProcedimiento { get; set; } = string.Empty;
    public string? DescripcionProcedimiento { get; set; }

    // Desglose de costos
    public decimal CostoProcedimiento { get; set; }
    public decimal CostoInsumos { get; set; }
    public decimal CostoAdicional { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal IVA { get; set; }
    public decimal Total { get; set; }

    // Información adicional
    public string? Observaciones { get; set; }
    public string? Diagnostico { get; set; }
    public string? Tratamiento { get; set; }
    public string? MedicacionPrescrita { get; set; }

    // Estado del ticket
    public EstadoTicket Estado { get; set; } = EstadoTicket.Generado;
    public DateTime? FechaEntrega { get; set; }
    public Guid? EntregadoPorId { get; set; }
    public Usuario? EntregadoPor { get; set; }

    // Relación con pago
    public Guid? PagoId { get; set; }
    public Pago? Pago { get; set; }

    // Detalles del ticket
    public ICollection<TicketDetalle> Detalles { get; set; } = new List<TicketDetalle>();

    public void CalcularTotal()
    {
        Subtotal = CostoProcedimiento + CostoInsumos + CostoAdicional;
        var subtotalConDescuento = Subtotal - Descuento;
        IVA = subtotalConDescuento * 0.16m; // 16% IVA México
        Total = subtotalConDescuento + IVA;
    }

    public void MarcarComoEntregado(Guid entregadoPorId)
    {
        Estado = EstadoTicket.Entregado;
        FechaEntrega = DateTime.UtcNow;
        EntregadoPorId = entregadoPorId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = entregadoPorId;
    }

    public string GenerarNumeroTicket()
    {
        var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"TK-{fecha}-{random}";
    }
}

public enum EstadoTicket
{
    Generado = 1,
    Entregado = 2,
    Cancelado = 3,
    Reimpreso = 4
}
