using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Mascotas;

/// <summary>
/// Representa una mascota del refugio
/// </summary>
public class Mascota : SoftDeletableEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public SexoMascota Sexo { get; set; }
    public EstatusMascota Estatus { get; set; } = EstatusMascota.Disponible;
    public string? Personalidad { get; set; }
    public string? EstadoSalud { get; set; }
    public string? RequisitoAdopcion { get; set; }
    public string? Origen { get; set; }
    public string? Notas { get; set; }

    // Navigation properties
    public ICollection<MascotaFoto> Fotos { get; set; } = new List<MascotaFoto>();
    public ICollection<SolicitudAdopcion> SolicitudesAdopcion { get; set; } = new List<SolicitudAdopcion>();

    public int? EdadEnMeses
    {
        get
        {
            if (!FechaNacimiento.HasValue) return null;
            var edad = DateTime.UtcNow - FechaNacimiento.Value;
            return (int)(edad.TotalDays / 30.44);
        }
    }

    public bool EstaDisponibleParaAdopcion()
    {
        return Estatus == EstatusMascota.Disponible && !IsDeleted;
    }

    public void CambiarEstatus(EstatusMascota nuevoEstatus, Guid usuarioId)
    {
        Estatus = nuevoEstatus;
        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum SexoMascota
{
    Macho = 1,
    Hembra = 2,
    Desconocido = 3
}

public enum EstatusMascota
{
    Disponible = 1,
    Reservada = 2,
    Adoptada = 3,
    NoAdoptable = 4,
    EnTratamiento = 5
}
