namespace AdoPetsBKD.Application.DTOs.Inventario;


public class CrearCompraDTO
{
    public Guid ProveedorId { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public string? Notas { get; set; }
    public List<CrearDetalleCompraDTO> Detalles { get; set; } = new();
}