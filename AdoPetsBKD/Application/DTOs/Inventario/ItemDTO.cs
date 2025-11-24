using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Application.DTOs.Inventario;

public class ItemDTO
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
    public CategoriaInventario Categoria { get; set; }
    public decimal MinQty { get; set; }
    public bool Activo { get; set; }
    public decimal StockTotal { get; set; }
}
