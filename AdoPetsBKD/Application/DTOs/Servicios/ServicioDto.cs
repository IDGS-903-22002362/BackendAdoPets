using AdoPetsBKD.Domain.Entities.Servicios;

namespace AdoPetsBKD.Application.DTOs.Servicios;

/// <summary>
/// DTO para listar servicios
/// </summary>
public class ServicioDto
{
    public Guid Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public CategoriaServicio Categoria { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public int DuracionMinDefault { get; set; }
    public decimal PrecioSugerido { get; set; }
    public string? Notas { get; set; }
    public bool Activo { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO para crear un servicio
/// </summary>
public class CreateServicioDto
{
    public string Descripcion { get; set; } = string.Empty;
    public CategoriaServicio Categoria { get; set; }
    public int DuracionMinDefault { get; set; } = 30;
    public decimal PrecioSugerido { get; set; }
    public string? Notas { get; set; }
}

/// <summary>
/// DTO para actualizar un servicio
/// </summary>
public class UpdateServicioDto
{
    public string Descripcion { get; set; } = string.Empty;
    public CategoriaServicio Categoria { get; set; }
    public int DuracionMinDefault { get; set; }
    public decimal PrecioSugerido { get; set; }
    public string? Notas { get; set; }
    public bool Activo { get; set; }
}
