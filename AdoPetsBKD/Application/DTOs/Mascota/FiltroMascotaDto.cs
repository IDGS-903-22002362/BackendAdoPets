using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class FiltroMascotaDto
    {
        public string? Especie { get; set; }
        public string? Raza { get; set; }
        public SexoMascota? Sexo { get; set; }
        public EstatusMascota? Estatus { get; set; }
        public int? EdadEnAnios { get; set; }
    }
}
