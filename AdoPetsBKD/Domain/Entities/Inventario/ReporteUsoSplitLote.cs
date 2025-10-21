using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Split de consumo por lote - permite reversión exacta FIFO
/// </summary>
public class ReporteUsoSplitLote : BaseEntity
{
    public Guid ReporteId { get; set; }
    public ReporteUsoInsumos Reporte { get; set; } = null!;

    public Guid DetalleId { get; set; }
    public ReporteUsoInsumoDetalle Detalle { get; set; } = null!;

    public Guid BatchId { get; set; }
    public LoteInventario Batch { get; set; } = null!;

    public decimal QtyConsumida { get; set; }
    public DateTime ConsumedAt { get; set; } = DateTime.UtcNow;
}
