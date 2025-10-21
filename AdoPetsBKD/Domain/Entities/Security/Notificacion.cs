using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Notificación para un usuario
/// </summary>
public class Notificacion : BaseEntity
{
    public TipoNotificacion Tipo { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? DataJson { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public bool IsRead => ReadAt.HasValue;

    public void MarcarComoLeida()
    {
        ReadAt = DateTime.UtcNow;
    }

    public void MarcarComoEntregada()
    {
        DeliveredAt = DateTime.UtcNow;
    }
}

public enum TipoNotificacion
{
    AdoptionStatusChange = 1,
    AppointmentReminder = 2,
    InventoryLow = 3,
    InventoryExpiring = 4,
    DonationReceived = 5,
    AppointmentCancelled = 6,
    AppointmentCreated = 7,
    System = 99
}
