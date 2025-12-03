using AdoPetsBKD.Application.DTOs.Mascota;

namespace AdoPetsBKD.Application.DTOs.Usuarios;

/// <summary>
/// DTO que representa un adoptante con todas sus mascotas
/// </summary>
public class AdoptanteConMascotasDto
{
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime? UltimoAccesoAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Total de mascotas (adoptadas + registradas)
    /// </summary>
    public int TotalMascotas { get; set; }
    
    /// <summary>
    /// Cantidad de mascotas adoptadas del refugio
    /// </summary>
    public int MascotasAdoptadas { get; set; }
    
    /// <summary>
    /// Cantidad de mascotas registradas por el usuario
    /// </summary>
    public int MascotasRegistradas { get; set; }
    
    /// <summary>
    /// Lista de todas las mascotas del adoptante
    /// </summary>
    public List<MascotaAdoptanteDto> Mascotas { get; set; } = new();
}

/// <summary>
/// DTO que representa una mascota de un adoptante con información de origen
/// </summary>
public class MascotaAdoptanteDto
{
    public Guid MascotaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public int Sexo { get; set; }
    public string SexoNombre => Sexo == 1 ? "Macho" : "Hembra";
    public DateTime? FechaNacimiento { get; set; }
    public int? EdadEnAnios { get; set; }
    public string? Personalidad { get; set; }
    public string? EstadoSalud { get; set; }
    public int Estatus { get; set; }
    public string EstatusNombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo de mascota: 1 = Del Refugio, 2 = De Usuario
    /// </summary>
    public int Tipo { get; set; }
    
    /// <summary>
    /// Indica si la mascota fue adoptada del refugio o registrada por el usuario
    /// </summary>
    public string OrigenMascota => Tipo == 1 ? "Adoptada del Refugio" : "Registrada por Usuario";
    
    /// <summary>
    /// Fecha en que se adoptó (si fue adoptada) o se registró (si fue registrada)
    /// </summary>
    public DateTime FechaAdquisicion { get; set; }
    
    /// <summary>
    /// Para mascotas adoptadas: fecha de la solicitud de adopción aprobada
    /// </summary>
    public DateTime? FechaSolicitudAdopcion { get; set; }
    
    /// <summary>
    /// Para mascotas adoptadas: fecha de aprobación
    /// </summary>
    public DateTime? FechaAprobacionAdopcion { get; set; }
    
    /// <summary>
    /// Fotos de la mascota
    /// </summary>
    public List<AddMascotaFotoDto> Fotos { get; set; } = new();
}
