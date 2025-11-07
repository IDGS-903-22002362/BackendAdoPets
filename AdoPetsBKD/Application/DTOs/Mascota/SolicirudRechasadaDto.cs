using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class SolicirudRechasadaDto
    {
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }
        public EstadoSolicitudAdopcion Estado { get; set; }

        public string? MotivoRechazo { get; set; }
    }
}
