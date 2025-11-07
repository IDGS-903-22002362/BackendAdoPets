using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class MascotaDetailDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string? Raza { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public SexoMascota Sexo { get; set; } 

        public EstatusMascota Estatus { get; set; }
        public string? Personalidad { get; set; }
        public string? EstadoSalud { get; set; }
        public string? RequisitoAdopcion { get; set; }
        public string? Origen { get; set; }
        public string? Notas { get; set; }
        public List<AddMascotaFotoDto>? Fotos { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public int? EdadEnAnio { get; set; }
    }
}
