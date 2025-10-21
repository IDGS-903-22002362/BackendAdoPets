using AdoPetsBKD.Domain.Common;

namespace AdoPetsBKD.Domain.Entities.Servicios;

/// <summary>
/// Catálogo de servicios veterinarios
/// </summary>
public class Servicio : BaseEntity
{
    public string Descripcion { get; set; } = string.Empty;
    public int DuracionMinDefault { get; set; }
    public decimal? PrecioSugerido { get; set; }
    public bool Activo { get; set; } = true;
    public string? Notas { get; set; }
    public CategoriaServicio Categoria { get; set; }

    public void Activar()
    {
        Activo = true;
    }

    public void Desactivar()
    {
        Activo = false;
    }
}

public enum CategoriaServicio
{
    Consulta = 1,
    Cirugia = 2,
    Diagnostico = 3,
    Estetica = 4,
    Vacunacion = 5,
    Emergencia = 6,
    Otro = 99
}
