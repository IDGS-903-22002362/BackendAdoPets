using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Domain.Entities.Clinica;

/// <summary>
/// Cita veterinaria
/// </summary>
public class Cita : AuditableEntity
{
    public Guid? MascotaId { get; set; }
    public Mascota? Mascota { get; set; }

    public Guid? PropietarioId { get; set; }
    public Usuario? Propietario { get; set; }

    public Guid VeterinarioId { get; set; }
    public Usuario Veterinario { get; set; } = null!;

    public Guid? SalaId { get; set; }
    public Sala? Sala { get; set; }

    public TipoCita Tipo { get; set; }
    public StatusCita Status { get; set; } = StatusCita.Programada;
    
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int DuracionMin { get; set; }
    
    public string? Notas { get; set; }
    public string? MotivoConsulta { get; set; }
    public string? MotivoRechazo { get; set; }
    
    public Guid? PagoId { get; set; } // Reservado para futuro

    // Navigation properties
    public ICollection<CitaRecordatorio> Recordatorios { get; set; } = new List<CitaRecordatorio>();
    public ICollection<CitaHistorialEstado> HistorialEstados { get; set; } = new List<CitaHistorialEstado>();
    public ReporteUsoInsumos? ReporteInsumos { get; set; }

    public bool TieneSolapamiento(DateTime inicio, DateTime fin)
    {
        return StartAt < fin && EndAt > inicio;
    }

    public void Cancelar(Guid usuarioId, string? motivo = null)
    {
        var estadoAnterior = Status;
        Status = StatusCita.Cancelada;
        MotivoRechazo = motivo;
        
        HistorialEstados.Add(new CitaHistorialEstado
        {
            CitaId = Id,
            FromStatus = estadoAnterior,
            ToStatus = StatusCita.Cancelada,
            ChangedBy = usuarioId,
            ChangedAt = DateTime.UtcNow
        });

        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Completar(Guid usuarioId)
    {
        var estadoAnterior = Status;
        Status = StatusCita.Completada;
        
        HistorialEstados.Add(new CitaHistorialEstado
        {
            CitaId = Id,
            FromStatus = estadoAnterior,
            ToStatus = StatusCita.Completada,
            ChangedBy = usuarioId,
            ChangedAt = DateTime.UtcNow
        });

        UpdatedBy = usuarioId;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool PuedeSerCancelada()
    {
        return Status == StatusCita.Programada && StartAt > DateTime.UtcNow;
    }
}

public enum TipoCita
{
    Consulta = 1,
    Cirugia = 2,
    Baño = 3,
    Vacuna = 4,
    Procedimiento = 5,
    Urgencia = 6,
    Seguimiento = 7
}

public enum StatusCita
{
    Programada = 1,
    Completada = 2,
    Cancelada = 3,
    NoAsistio = 4,
    EnProceso = 5
}
