using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Alerta de inventario (stock bajo, productos por vencer)
/// </summary>
public class AlertaInventario : BaseEntity
{
    public Guid ItemId { get; set; }
    public ItemInventario Item { get; set; } = null!;

    public TipoAlerta Tipo { get; set; }
    public string? PayloadJson { get; set; }
    public StatusAlerta Status { get; set; } = StatusAlerta.Open;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public Guid? AcknowledgedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public string? ResolutionNotes { get; set; }

    public void Acknowledge(Guid userId)
    {
        Status = StatusAlerta.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedBy = userId;
    }

    public void Close(Guid userId, string? notes = null)
    {
        Status = StatusAlerta.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedBy = userId;
        ResolutionNotes = notes;
    }
}

public enum TipoAlerta
{
    LowStock = 1,
    Expiring = 2,
    Expired = 3,
    OutOfStock = 4
}

public enum StatusAlerta
{
    Open = 1,
    Acknowledged = 2,
    Closed = 3
}
