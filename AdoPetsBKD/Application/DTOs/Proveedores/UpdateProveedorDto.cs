namespace AdoPetsBKD.Application.DTOs.Proveedores;

public class UpdateProveedorDto
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? RFC { get; set; }
    public string? Contacto { get; set; }
    public string? Notas { get; set; }
    public int? Estatus { get; set; }
}
