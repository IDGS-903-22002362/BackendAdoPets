using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Domain.Entities.HistorialClinico;

/// <summary>
/// Adjunto médico (estudios, radiografías, etc.)
/// </summary>
public class AdjuntoMedico : BaseEntity
{
    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public TipoEntryMedico EntryType { get; set; }
    public Guid? EntryId { get; set; } // FK a Vacunacion, Desparasitacion, Cirugia, etc.

    public string StorageKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? Descripcion { get; set; }
    
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

public enum TipoEntryMedico
{
    Vacunacion = 1,
    Desparasitacion = 2,
    Cirugia = 3,
    Expediente = 4,
    Laboratorio = 5,
    Imagen = 6,
    General = 99
}
