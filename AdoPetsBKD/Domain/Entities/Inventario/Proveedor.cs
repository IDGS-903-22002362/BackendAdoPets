using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Inventario;

/// <summary>
/// Proveedor de insumos
/// </summary>
public class Proveedor : AuditableEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public EstatusProveedor Estatus { get; set; } = EstatusProveedor.Activo;
    public string? RFC { get; set; }
    public string? Contacto { get; set; }
    public string? Notas { get; set; }

    // Navigation properties
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();

    public void Activar()
    {
        Estatus = EstatusProveedor.Activo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Estatus = EstatusProveedor.Inactivo;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum EstatusProveedor
{
    Activo = 1,
    Inactivo = 2,
    Bloqueado = 3
}
