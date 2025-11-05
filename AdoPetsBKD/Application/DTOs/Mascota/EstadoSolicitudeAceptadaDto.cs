using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class EstadoSolicitudeAceptadaDto
    {
        public Guid Id { get; set; }

        public EstadoSolicitudAdopcion Estado { get; set; }
    }
}
