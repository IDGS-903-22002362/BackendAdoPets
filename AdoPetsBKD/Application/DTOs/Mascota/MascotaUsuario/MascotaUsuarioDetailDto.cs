using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota.MascotaUsuario;

/// <summary>
/// DTO de respuesta con detalles de una mascota de usuario
/// </summary>
public class MascotaUsuarioDetailDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public SexoMascota Sexo { get; set; }
    public string? Personalidad { get; set; }
    public string? EstadoSalud { get; set; }
    public string? Notas { get; set; }
    public Guid PropietarioId { get; set; }
    public int? EdadEnAnios { get; set; }
    public List<AddMascotaFotoDto> Fotos { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
