namespace AdoPetsBKD.Application.DTOs.Proveedores;
using System.ComponentModel.DataAnnotations;

public class ProveedorDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public int Estatus { get; set; }  // 1 = Activo, 2 = Inactivo, 3 = Bloqueado
    public string? RFC { get; set; }
    public string? Contacto { get; set; }
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}