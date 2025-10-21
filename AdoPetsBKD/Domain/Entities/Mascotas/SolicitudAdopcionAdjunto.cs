using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Mascotas;

/// <summary>
/// Adjunto a una solicitud de adopción
/// </summary>
public class SolicitudAdopcionAdjunto : BaseEntity
{
    public Guid SolicitudId { get; set; }
    public SolicitudAdopcion Solicitud { get; set; } = null!;

    public string StorageKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Guid? UploadedBy { get; set; }
}
