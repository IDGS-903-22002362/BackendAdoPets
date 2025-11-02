using AdoPetsBKD.Domain.Entities.Mascotas;
using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class UpdateMascotaDto
    {
        [Required(ErrorMessage = "El nombre de la mascota es requerido")]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;


        [StringLength(100)]
        public string Especie { get; set; } = string.Empty;


        [StringLength(100)]
        public string? Raza { get; set; }

        public DateTime? FechaNacimiento { get; set; }


        public SexoMascota Sexo { get; set; }

        public EstatusMascota Estatus { get; set; }

        public string? Personalidad { get; set; }

        [StringLength(500)]

        public string? EstadoSalud { get; set; }

        [StringLength(500)]
        public string? RequisitoAdopcion { get; set; }

        [StringLength(8)]
        public string? Origen { get; set; }

        [StringLength(500)]
        public string? Notas { get; set; }
    }
}
