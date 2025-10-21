using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Auditoria;

/// <summary>
/// Log de auditoría para cambios críticos
/// </summary>
public class AuditLog : BaseEntity
{
    public Guid? UsuarioId { get; set; }
    public string? Entidad { get; set; }
    public string? EntidadId { get; set; }
    public AccionAudit Accion { get; set; }
    
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public enum AccionAudit
{
    Create = 1,
    Update = 2,
    Delete = 3,
    StateChange = 4,
    Login = 5,
    Logout = 6,
    PasswordChange = 7,
    RoleAssignment = 8
}
