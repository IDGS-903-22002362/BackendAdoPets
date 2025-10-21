using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Security;

/// <summary>
/// Representa un rol del sistema
/// </summary>
public class Rol : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    // Navigation properties
    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}

/// <summary>
/// Catálogo de roles del sistema
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Veterinario = "Veterinario";
    public const string Recepcionista = "Recepcionista";
    public const string Asistente = "Asistente";
    public const string Adoptante = "Adoptante";
}
