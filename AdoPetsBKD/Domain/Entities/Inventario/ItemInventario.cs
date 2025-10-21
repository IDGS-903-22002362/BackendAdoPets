using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Item de inventario
/// </summary>
public class ItemInventario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty; // pz, ml, mg, etc.
    public CategoriaInventario Categoria { get; set; }
    public decimal MinQty { get; set; } // Stock mínimo
    public bool Activo { get; set; } = true;
    public string? Notas { get; set; }
    public string? Descripcion { get; set; }

    // Navigation properties
    public ICollection<LoteInventario> Lotes { get; set; } = new List<LoteInventario>();
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    public ICollection<AlertaInventario> Alertas { get; set; } = new List<AlertaInventario>();

    public decimal StockTotal => Lotes.Sum(l => l.QtyDisponible);

    public bool TieneStockBajo()
    {
        return StockTotal < MinQty;
    }

    public bool TieneLotesProximosAVencer(int diasAnticipacion = 30)
    {
        var fechaLimite = DateTime.UtcNow.AddDays(diasAnticipacion);
        return Lotes.Any(l => l.ExpDate.HasValue && l.ExpDate.Value <= fechaLimite && l.QtyDisponible > 0);
    }
}

public enum CategoriaInventario
{
    Medicamento = 1,
    Vacuna = 2,
    Alimento = 3,
    MaterialCuracion = 4,
    Quirurgico = 5,
    Limpieza = 6,
    Otro = 99
}
