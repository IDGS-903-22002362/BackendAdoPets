using AdoPetsBKD.Domain.Entities.Mascotas;
using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    /// <summary>
    /// DTO para la creación de una nueva mascota en el refugio
    /// </summary>
    public class CreateMascotaDto
    {

        [Required(ErrorMessage = "El nombre de la mascota es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

   
        [Required(ErrorMessage = "La especie de la mascota es requerida")]
        [StringLength(100)]
        public string Especie { get; set; } = string.Empty;


        [StringLength(100)]
        public string? Raza { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El sexo de la mascota es requerido")]

        public SexoMascota Sexo { get; set; }

        public string? Personalidad { get; set; }

        [Required(ErrorMessage = "El estado de salud de la mascota es requerido")]
        [StringLength(500)]

        public string? EstadoSalud { get; set; }

        [Required(ErrorMessage = "Los Requisitos de Adopcíón son requeridos")]
        [StringLength(500)]
        public string? RequisitoAdopcion { get; set; }

        [Required(ErrorMessage = "El origen de la mascota es requerido")]
        [StringLength(8)]
        public string? Origen { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }



    }

    
}
