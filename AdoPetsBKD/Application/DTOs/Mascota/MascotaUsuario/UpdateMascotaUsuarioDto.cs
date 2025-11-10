using AdoPetsBKD.Domain.Entities.Mascotas;
using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Mascota.MascotaUsuario;

/// <summary>
/// DTO para actualizar una mascota de usuario
/// </summary>
public class UpdateMascotaUsuarioDto
{
    [Required(ErrorMessage = "El nombre de la mascota es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especie de la mascota es requerida")]
    [StringLength(50, ErrorMessage = "La especie no puede exceder 50 caracteres")]
    public string Especie { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "La raza no puede exceder 100 caracteres")]
    public string? Raza { get; set; }

    public DateTime? FechaNacimiento { get; set; }

    [Required(ErrorMessage = "El sexo de la mascota es requerido")]
    public SexoMascota Sexo { get; set; }

    [StringLength(500, ErrorMessage = "La personalidad no puede exceder 500 caracteres")]
    public string? Personalidad { get; set; }

    [StringLength(500, ErrorMessage = "El estado de salud no puede exceder 500 caracteres")]
    public string? EstadoSalud { get; set; }

    [StringLength(2000, ErrorMessage = "Las notas no pueden exceder 2000 caracteres")]
    public string? Notas { get; set; }
}
