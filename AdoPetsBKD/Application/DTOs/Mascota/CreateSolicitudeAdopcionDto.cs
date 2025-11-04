using AdoPetsBKD.Domain.Entities.Mascotas;
using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class CreateSolicitudeAdopcionDto
    {
        [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
        public Guid UsuarioId { get; set; }

        [Required(ErrorMessage = "El ID de la mascota es obligatorio.")]
        public Guid MascotaId { get; set; }

        [Required(ErrorMessage = "El tipo de vivienda es obligatorio.")]
        [Range(1, 99, ErrorMessage = "El tipo de vivienda seleccionado no es válido.")]
        public TipoVivienda Vivienda { get; set; }

        [Range(0, 20, ErrorMessage = "El número de niños debe estar entre 0 y 20.")]
        public int NumNinios { get; set; }

        [Required(ErrorMessage = "Debe indicar si tiene otras mascotas.")]
        public bool OtrasMascotas { get; set; }

        [Range(1, 24, ErrorMessage = "Las horas de disponibilidad deben estar entre 1 y 24.")]
        public int HorasDisponibilidad { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [StringLength(255, ErrorMessage = "La dirección no puede superar los 255 caracteres.")]
        public string Direccion { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Los ingresos mensuales deben ser un valor positivo.")]
        public decimal? IngresosMensuales { get; set; }

        [StringLength(1000, ErrorMessage = "El motivo de adopción no puede superar los 1000 caracteres.")]
        public string? MotivoAdopcion { get; set; }

        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

        public EstadoSolicitudAdopcion Estado { get; set; } = EstadoSolicitudAdopcion.Pendiente;

        


    }
}
