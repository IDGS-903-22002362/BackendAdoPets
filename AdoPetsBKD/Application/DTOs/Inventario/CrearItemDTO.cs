using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Application.DTOs.Inventario;

public class CrearItemDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty; // pz, ml, etc.
    public CategoriaInventario Categoria { get; set; }
    public decimal MinQty { get; set; }
    public string? Notas { get; set; }
    public string? Descripcion { get; set; }
}
