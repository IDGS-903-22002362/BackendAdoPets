using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Solicitud de cita digital realizada por un adoptante
/// Requiere confirmación del personal y pago del 50% por PayPal
/// </summary>
public class SolicitudCitaDigital : AuditableEntity
{
    public string NumeroSolicitud { get; set; } = string.Empty; // Formato: SC-YYYYMMDD-XXXX

    // Solicitante
    public Guid SolicitanteId { get; set; }
    public Usuario Solicitante { get; set; } = null!;

    // Información de la mascota
    public Guid? MascotaId { get; set; }
    public Mascotas.Mascota? Mascota { get; set; }
    public string NombreMascota { get; set; } = string.Empty;
    public string? EspecieMascota { get; set; }
    public string? RazaMascota { get; set; }

    // Detalles de la solicitud
    public Guid? ServicioId { get; set; }
    public Servicios.Servicio? Servicio { get; set; }
    public string DescripcionServicio { get; set; } = string.Empty;
    public string? MotivoConsulta { get; set; }

    // Fecha y hora solicitadas
    public DateTime FechaHoraSolicitada { get; set; }
    public int DuracionEstimadaMin { get; set; } = 60;

    // Preferencias
    public Guid? VeterinarioPreferidoId { get; set; }
    public Usuario? VeterinarioPreferido { get; set; }
    public Guid? SalaPreferidaId { get; set; }
    public Sala? SalaPreferida { get; set; }

    // Estado de la solicitud
    public EstadoSolicitudCita Estado { get; set; } = EstadoSolicitudCita.Pendiente;
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRevision { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public DateTime? FechaRechazo { get; set; }

    // Revisión y confirmación
    public Guid? RevisadoPorId { get; set; }
    public Usuario? RevisadoPor { get; set; }
    public string? MotivoRechazo { get; set; }
    public string? NotasInternas { get; set; }

    // Costos
    public decimal CostoEstimado { get; set; }
    public decimal MontoAnticipo { get; set; } // 50% del costo estimado

    // Pago del anticipo
    public Guid? PagoAnticipoId { get; set; }
    public Pago? PagoAnticipo { get; set; }

    // Cita generada
    public Guid? CitaId { get; set; }
    public Cita? Cita { get; set; }

    // Verificación de disponibilidad
    public bool DisponibilidadVerificada { get; set; }
    public DateTime? FechaVerificacionDisponibilidad { get; set; }

    public void CalcularMontoAnticipo()
    {
        MontoAnticipo = CostoEstimado * 0.5m; // 50% de anticipo
    }

    public void MarcarComoEnRevision(Guid revisadoPorId)
    {
        Estado = EstadoSolicitudCita.EnRevision;
        FechaRevision = DateTime.UtcNow;
        RevisadoPorId = revisadoPorId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = revisadoPorId;
    }

    public void Confirmar(Guid confiradoPorId, Guid citaId)
    {
        Estado = EstadoSolicitudCita.Confirmada;
        FechaConfirmacion = DateTime.UtcNow;
        CitaId = citaId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = confiradoPorId;
    }

    public void Rechazar(Guid rechazadoPorId, string motivo)
    {
        Estado = EstadoSolicitudCita.Rechazada;
        FechaRechazo = DateTime.UtcNow;
        MotivoRechazo = motivo;
        RevisadoPorId = rechazadoPorId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = rechazadoPorId;
    }

    public void MarcarPagoRecibido(Guid pagoId)
    {
        PagoAnticipoId = pagoId;
        if (Estado == EstadoSolicitudCita.PendientePago)
        {
            Estado = EstadoSolicitudCita.PagadaPendienteConfirmacion;
        }
    }

    public string GenerarNumeroSolicitud()
    {
        var fecha = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"SC-{fecha}-{random}";
    }
}

public enum EstadoSolicitudCita
{
    Pendiente = 1,              // Solicitud creada, esperando revisión
    EnRevision = 2,             // Personal está revisando la solicitud
    PendientePago = 3,          // Aprobada, esperando pago del anticipo
    PagadaPendienteConfirmacion = 4, // Pago recibido, esperando confirmación final
    Confirmada = 5,             // Cita confirmada y creada
    Rechazada = 6,              // Solicitud rechazada
    Cancelada = 7,              // Cancelada por el solicitante
    Expirada = 8                // Venció el tiempo para pago
}
