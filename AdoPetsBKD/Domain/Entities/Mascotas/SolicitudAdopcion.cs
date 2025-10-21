using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Domain.Entities.Mascotas;

/// <summary>
/// Solicitud de adopción de una mascota
/// </summary>
public class SolicitudAdopcion : AuditableEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public Guid MascotaId { get; set; }
    public Mascota Mascota { get; set; } = null!;

    public EstadoSolicitudAdopcion Estado { get; set; } = EstadoSolicitudAdopcion.Pendiente;
    
    // Información del solicitante
    public TipoVivienda Vivienda { get; set; }
    public int NumNinios { get; set; }
    public bool OtrasMascotas { get; set; }
    public int HorasDisponibilidad { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public decimal? IngresosMensuales { get; set; }
    public string? MotivoAdopcion { get; set; }
    public string? MotivoRechazo { get; set; }

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRevision { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public Guid? RevisadoPor { get; set; }

    // Navigation properties
    public ICollection<SolicitudAdopcionAdjunto> Adjuntos { get; set; } = new List<SolicitudAdopcionAdjunto>();
    public ICollection<AdopcionLog> Logs { get; set; } = new List<AdopcionLog>();

    public void CambiarEstado(EstadoSolicitudAdopcion nuevoEstado, Guid usuarioId, string? motivo = null)
    {
        var estadoAnterior = Estado;
        Estado = nuevoEstado;
        
        if (nuevoEstado == EstadoSolicitudAdopcion.Rechazada)
        {
            MotivoRechazo = motivo;
        }

        if (nuevoEstado == EstadoSolicitudAdopcion.EnRevision)
        {
            FechaRevision = DateTime.UtcNow;
            RevisadoPor = usuarioId;
        }

        if (nuevoEstado == EstadoSolicitudAdopcion.Aprobada)
        {
            FechaAprobacion = DateTime.UtcNow;
        }

        // Agregar log
        Logs.Add(new AdopcionLog
        {
            SolicitudId = Id,
            FromEstado = estadoAnterior,
            ToEstado = nuevoEstado,
            Reason = motivo,
            ChangedBy = usuarioId,
            ChangedAt = DateTime.UtcNow
        });

        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool PuedeSerAprobada()
    {
        return Estado == EstadoSolicitudAdopcion.EnRevision 
            && Mascota.EstaDisponibleParaAdopcion();
    }
}

public enum EstadoSolicitudAdopcion
{
    Pendiente = 1,
    EnRevision = 2,
    Aprobada = 3,
    Rechazada = 4,
    Cancelada = 5
}

public enum TipoVivienda
{
    Casa = 1,
    Departamento = 2,
    Quinta = 3,
    Otro = 99
}
