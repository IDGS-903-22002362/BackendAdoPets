using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Compra de insumos a un proveedor
/// </summary>
public class Compra : AuditableEntity
{
    public Guid ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public string? NumeroFactura { get; set; }
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public StatusCompra Status { get; set; } = StatusCompra.Pendiente;
    public string? Notas { get; set; }
    public DateTime? FechaRecepcion { get; set; }
    public Guid? RecibidoPor { get; set; }

    // Navigation properties
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();

    public void Confirmar(Guid userId)
    {
        Status = StatusCompra.Confirmada;
        FechaRecepcion = DateTime.UtcNow;
        RecibidoPor = userId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void Cancelar(Guid userId)
    {
        Status = StatusCompra.Cancelada;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public decimal CalcularTotal()
    {
        return Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
    }
}

public enum StatusCompra
{
    Pendiente = 1,
    Confirmada = 2,
    Recibida = 3,
    Cancelada = 4,
    Parcial = 5
}
