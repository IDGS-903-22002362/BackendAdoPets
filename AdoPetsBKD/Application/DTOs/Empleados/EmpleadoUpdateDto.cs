using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Empleados
{
    /// <summary>
    /// DTO para actualizar un empleado existente
    /// </summary>
    public class EmpleadoUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Cedula { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required]
        [StringLength(80)]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Disponibilidad { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        [StringLength(80)]
        public string EmailLaboral { get; set; } = string.Empty;
        
        [Phone]
        [Required]
        [StringLength(20)]
        public string TelefonoLaboral { get; set; } = string.Empty;
        
        [Required]
        public int Tipo { get; set; }
        
        [Required]
        public decimal Sueldo { get; set; }

        /// <summary>
        /// Lista de especialidades del empleado (opcional)
        /// </summary>
        public List<EspecialidadAsignacionDto>? Especialidades { get; set; }
    }
}
