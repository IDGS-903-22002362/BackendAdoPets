using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class CambiarEstadoSolicitudEnRevisionDto
    {
       public Guid Id { get; set; }

      public EstadoSolicitudAdopcion Estado { get; set; }

     public Guid RevisadoPor { get; set; }

    }
}
