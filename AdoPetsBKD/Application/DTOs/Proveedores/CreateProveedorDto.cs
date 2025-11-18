namespace AdoPetsBKD.Application.DTOs.Proveedores;

public class CreateProveedorDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RFC { get; set; }
    public string? Contacto { get; set; }
    public string? Notas { get; set; }
}
