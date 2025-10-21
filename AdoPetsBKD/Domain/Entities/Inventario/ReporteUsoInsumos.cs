using AdoPetsBKD.Domain.Common;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Reporte post-procedimiento de uso de insumos con descuento FIFO
/// </summary>
public class ReporteUsoInsumos : BaseEntity
{
    public Guid CitaId { get; set; }
    public Cita Cita { get; set; } = null!;

    public Guid VetId { get; set; }
    public StatusReporte Status { get; set; } = StatusReporte.Registrado;
    
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    
    /// <summary>
    /// Para idempotencia - evita duplicados
    /// </summary>
    public string? ClientUsageId { get; set; }

    public DateTime? RevertedAt { get; set; }
    public Guid? RevertedBy { get; set; }
    public string? RevertReason { get; set; }

    // Navigation properties
    public ICollection<ReporteUsoInsumoDetalle> Detalles { get; set; } = new List<ReporteUsoInsumoDetalle>();

    public bool PuedeSerRevertido()
    {
        return Status == StatusReporte.Registrado;
    }

    public void Revertir(Guid userId, string? reason = null)
    {
        if (!PuedeSerRevertido())
            throw new InvalidOperationException("El reporte no puede ser revertido");

        Status = StatusReporte.Revertido;
        RevertedAt = DateTime.UtcNow;
        RevertedBy = userId;
        RevertReason = reason;
    }
}

public enum StatusReporte
{
    Registrado = 1,
    Revertido = 2
}
