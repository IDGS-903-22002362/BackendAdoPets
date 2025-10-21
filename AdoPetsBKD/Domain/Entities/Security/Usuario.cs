using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Representa un usuario del sistema
/// </summary>
public class Usuario : AuditableEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public EstatusUsuario Estatus { get; set; } = EstatusUsuario.Activo;
    public DateTime? UltimoAccesoAt { get; set; }
    public string? AceptoPoliticasVersion { get; set; }
    public DateTime? AceptoPoliticasAt { get; set; }

    // Navigation properties
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<Dispositivo> Dispositivos { get; set; } = new List<Dispositivo>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    public ICollection<ConsentimientoPolitica> Consentimientos { get; set; } = new List<ConsentimientoPolitica>();

    public string NombreCompleto => $"{Nombre} {ApellidoPaterno} {ApellidoMaterno}".Trim();

    public bool TienePoliticasAceptadas(string versionActual)
    {
        return AceptoPoliticasVersion == versionActual && AceptoPoliticasAt.HasValue;
    }

    public void AceptarPoliticas(string version)
    {
        AceptoPoliticasVersion = version;
        AceptoPoliticasAt = DateTime.UtcNow;
    }

    public void RegistrarAcceso()
    {
        UltimoAccesoAt = DateTime.UtcNow;
    }
}

public enum EstatusUsuario
{
    Activo = 1,
    Inactivo = 2,
    Bloqueado = 3
}
