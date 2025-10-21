using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Movimiento de inventario (entrada, salida, ajuste)
/// </summary>
public class MovimientoInventario : BaseEntity
{
    public Guid ItemId { get; set; }
    public ItemInventario Item { get; set; } = null!;

    public Guid? BatchId { get; set; }
    public LoteInventario? Batch { get; set; }

    public TipoMovimiento Tipo { get; set; }
    public decimal Qty { get; set; }
    public string Reason { get; set; } = string.Empty;
    
    public Guid PerformedBy { get; set; }
    public Guid? RelatedAppointmentId { get; set; }
    public Cita? RelatedAppointment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Observaciones { get; set; }
}

public enum TipoMovimiento
{
    Entrada = 1,
    Salida = 2,
    Ajuste = 3,
    Merma = 4,
    Devolucion = 5
}
