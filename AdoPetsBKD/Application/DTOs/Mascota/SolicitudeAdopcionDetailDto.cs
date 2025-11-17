using AdoPetsBKD.Domain.Entities.Mascotas;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class SolicitudeAdopcionDetailDto
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty ;

        public string Telefono { get; set; } = string .Empty ;

        public Guid MascotaId { get; set; }
        public string MascotaNombre { get; set; } = string.Empty;

        public string Especie { get; set; } = string.Empty;
        public string Raza { get; set; } = string.Empty;
        public SexoMascota Sexo { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Edad { get; set; } = string.Empty;

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
    



        public List<AddMascotaFotoDto> MascotaFotos { get; set; } = new List<AddMascotaFotoDto>();
        

    }
}
