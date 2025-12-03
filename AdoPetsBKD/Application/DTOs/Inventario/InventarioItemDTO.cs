namespace AdoPetsBKD.Application.DTOs.Inventario;

public class InventarioItemDTO
{
    public Guid ItemId { get; set; }
    public string? Nombre { get; set; }
    public decimal MinQty { get; set; }
    public string? Unidad { get; set; }
    public decimal TotalDisponible { get; set; }
    public decimal? PrecioUnitario { get; set; } // Precio del lote más próximo a vencer (FIFO)
    public LoteInventarioDTO? LoteMasProximo { get; set; }
}

public class LoteInventarioDTO
{
    public Guid LoteId { get; set; }
    public string? Lote { get; set; }
    public DateTime? ExpDate { get; set; }
    public decimal QtyDisponible { get; set; }
    public decimal PrecioUnitario { get; set; } // Precio unitario obtenido de DetalleCompra
}
