using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Detalle de insumos usados en un reporte
/// </summary>
public class ReporteUsoInsumoDetalle : BaseEntity
{
    public Guid ReporteId { get; set; }
    public ReporteUsoInsumos Reporte { get; set; } = null!;

    public Guid ItemId { get; set; }
    public ItemInventario Item { get; set; } = null!;

    public decimal QtySolicitada { get; set; }
    public decimal QtyAplicada { get; set; }
    public string? Notas { get; set; }

    // Navigation properties - splits por lote FIFO
    public ICollection<ReporteUsoSplitLote> Splits { get; set; } = new List<ReporteUsoSplitLote>();

    public decimal TotalConsumido => Splits.Sum(s => s.QtyConsumida);
}
