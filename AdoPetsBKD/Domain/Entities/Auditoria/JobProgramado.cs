using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Auditoria;

/// <summary>
/// Jobs programados (recordatorios, alertas, etc.)
/// </summary>
public class JobProgramado : BaseEntity
{
    public TipoJob Tipo { get; set; }
    public string? RelatedEntityId { get; set; }
    public string? PayloadJson { get; set; }
    
    public DateTime ScheduledFor { get; set; }
    public StatusJob Status { get; set; } = StatusJob.Pending;
    
    public DateTime? LastRunAt { get; set; }
    public int Attempts { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }

    public void MarcarComoCompletado()
    {
        Status = StatusJob.Done;
        CompletedAt = DateTime.UtcNow;
        LastRunAt = DateTime.UtcNow;
    }

    public void MarcarComoFallido(string error)
    {
        Status = StatusJob.Failed;
        ErrorMessage = error;
        LastRunAt = DateTime.UtcNow;
        Attempts++;
    }

    public void Reintentar()
    {
        Status = StatusJob.Pending;
        LastRunAt = DateTime.UtcNow;
        Attempts++;
    }

    public bool PuedeEjecutarse()
    {
        return Status == StatusJob.Pending && ScheduledFor <= DateTime.UtcNow;
    }

    public bool PuedeReintentar(int maxAttempts = 3)
    {
        return Attempts < maxAttempts && Status == StatusJob.Failed;
    }
}

public enum TipoJob
{
    RecordatorioCita = 1,
    AlertaInventarioExpiring = 2,
    AlertaInventarioLow = 3,
    LimpiezaTokensExpirados = 4,
    ProcesamientoOutbox = 5,
    GeneracionReportes = 6
}

public enum StatusJob
{
    Pending = 1,
    Running = 2,
    Done = 3,
    Failed = 4,
    Cancelled = 5
}
