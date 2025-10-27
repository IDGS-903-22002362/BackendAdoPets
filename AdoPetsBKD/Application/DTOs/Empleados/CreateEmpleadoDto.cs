using AdoPetsBKD.Application.DTOs.Usuarios;
using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Empleados
{
    /// <summary>
    /// DTO para crear un nuevo empleado
    /// </summary>
    public class CreateEmpleadoDto
    {
        [Required (ErrorMessage = "El empleado debe estar asociado a un usuario")]
        public CreateUsuarioDto Usuario { get; set; } = new();

        // Información laboral del empleado 
        [Required(ErrorMessage = "La cedula es requerida")]
        [StringLength(50)]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "La disponibilidad es requerida")]
        [StringLength(200)]
        public string Disponibilidad { get; set; } = string.Empty;

        [EmailAddress]
        [Required(ErrorMessage = "El email laboral es requerido")]
        [StringLength(80)]
        public string EmailLaboral { get; set; } = string.Empty;

        [Phone]
        [Required(ErrorMessage = "El telefono laboral es requerido")]
        [StringLength(20)]
        public string TelefonoLaboral { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de empleado es requerido")]
        public int Tipo { get; set; }

        [Required(ErrorMessage = "El sueldo es requerido")]
        public decimal Sueldo { get; set; }

    }
}
