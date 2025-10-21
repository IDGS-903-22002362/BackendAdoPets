using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Mascotas;

/// <summary>
/// Foto de una mascota
/// </summary>
public class MascotaFoto : BaseEntity
{
    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public string StorageKey { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool EsPrincipal { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
