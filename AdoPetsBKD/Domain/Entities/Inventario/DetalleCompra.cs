using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Detalle de compra - items específicos
/// </summary>
public class DetalleCompra : BaseEntity
{
    public Guid CompraId { get; set; }
    public Compra Compra { get; set; } = null!;

    public Guid ItemId { get; set; }
    public ItemInventario Item { get; set; } = null!;

    public string? Lote { get; set; }
    public DateTime? ExpDate { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;

    public string? Notas { get; set; }
}
