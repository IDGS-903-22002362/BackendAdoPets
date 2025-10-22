using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Detalle de items incluidos en el ticket
/// </summary>
public class TicketDetalle : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public string Descripcion { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public string? Unidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    // Referencia opcional a item de inventario
    public Guid? ItemInventarioId { get; set; }
    public ItemInventario? ItemInventario { get; set; }

    public TipoDetalleTicket Tipo { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = Cantidad * PrecioUnitario;
    }
}

public enum TipoDetalleTicket
{
    Procedimiento = 1,
    Insumo = 2,
    Medicamento = 3,
    Consulta = 4,
    Otro = 5
}
