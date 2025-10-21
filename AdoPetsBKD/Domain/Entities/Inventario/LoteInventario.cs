using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Lote de inventario con control FIFO
/// </summary>
public class LoteInventario : BaseEntity
{
    public Guid ItemId { get; set; }
    public ItemInventario Item { get; set; } = null!;

    public string Lote { get; set; } = string.Empty;
    public DateTime? ExpDate { get; set; }
    public decimal QtyDisponible { get; set; }
    public decimal QtyInicial { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notas { get; set; }

    // Navigation properties
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    public ICollection<ReporteUsoSplitLote> SplitsUso { get; set; } = new List<ReporteUsoSplitLote>();

    public bool EstaVencido()
    {
        return ExpDate.HasValue && ExpDate.Value < DateTime.UtcNow;
    }

    public bool ProximoAVencer(int dias = 30)
    {
        if (!ExpDate.HasValue) return false;
        var fechaLimite = DateTime.UtcNow.AddDays(dias);
        return ExpDate.Value <= fechaLimite && ExpDate.Value >= DateTime.UtcNow;
    }

    public void DescontarStock(decimal cantidad)
    {
        if (cantidad > QtyDisponible)
            throw new InvalidOperationException($"No hay suficiente stock en el lote {Lote}");
        
        QtyDisponible -= cantidad;
    }

    public void AgregarStock(decimal cantidad)
    {
        QtyDisponible += cantidad;
    }
}
