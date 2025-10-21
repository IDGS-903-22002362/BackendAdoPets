using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Dispositivo registrado para notificaciones push (FCM)
/// </summary>
public class Dispositivo : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public PlataformaDispositivo Plataforma { get; set; }
    public string? AppVersion { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void ActualizarUltimaVista()
    {
        LastSeenAt = DateTime.UtcNow;
    }

    public void Deshabilitar()
    {
        Enabled = false;
    }
}

public enum PlataformaDispositivo
{
    Web = 1,
    Android = 2,
    iOS = 3
}
