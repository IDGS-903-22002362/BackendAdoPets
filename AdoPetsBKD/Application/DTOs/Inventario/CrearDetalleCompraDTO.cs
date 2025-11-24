namespace AdoPetsBKD.Application.DTOs.Inventario;


public class CrearDetalleCompraDTO
{
    public Guid ItemId { get; set; }
    public string? Lote { get; set; }
    public DateTime? ExpDate { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string? Notas { get; set; }
}