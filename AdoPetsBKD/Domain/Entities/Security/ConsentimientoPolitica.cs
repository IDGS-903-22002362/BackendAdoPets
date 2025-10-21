using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Registro de consentimiento de pol�ticas por usuario
/// </summary>
public class ConsentimientoPolitica : BaseEntity
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string Version { get; set; } = string.Empty;
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
}
