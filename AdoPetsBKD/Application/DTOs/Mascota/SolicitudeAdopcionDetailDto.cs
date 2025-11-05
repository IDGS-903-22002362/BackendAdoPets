using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class SolicitudeAdopcionDetailDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty; 
        public Guid MascotaId { get; set; }
        public string MascotaNombre { get; set; } = string.Empty; 
        public EstadoSolicitudAdopcion Estado { get; set; }
        public TipoVivienda Vivienda { get; set; }
        public int NumNinios { get; set; }
        public bool OtrasMascotas { get; set; }
        public int HorasDisponibilidad { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public decimal? IngresosMensuales { get; set; }
        public string? MotivoAdopcion { get; set; }
        public string? MotivoRechazo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRevision { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public Guid? RevisadoPor { get; set; }

       

            public List<AddMascotaFotoDto> MascotaFotos { get; set; } = new List<AddMascotaFotoDto>();
        

    }
}
