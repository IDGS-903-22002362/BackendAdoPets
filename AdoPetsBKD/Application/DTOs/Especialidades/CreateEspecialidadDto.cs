using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Especialidades
{
    /// <summary>
    /// DTO para crear una nueva especialidad
    /// </summary>
    public class CreateEspecialidadDto
    {
        [Required(ErrorMessage = "El código de la especialidad es obligatorio")]
        [StringLength(100)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción de la especialidad es obligatoria")]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;
    }
}
