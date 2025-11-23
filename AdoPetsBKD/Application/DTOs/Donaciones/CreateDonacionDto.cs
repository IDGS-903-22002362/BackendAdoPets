using System.ComponentModel.DataAnnotations;
using AdoPetsBKD.Domain.Entities.Donaciones;
namespace AdoPetsBKD.Application.DTOs.Donaciones
{
    public class CreateDonacionDto
    {
        public Guid? UsuarioId { get; set; } // Ahora nullable para donaciones anónimas
        
        [Required]
        public decimal Monto { get; set; }
        
        [Required]
        public string Moneda { get; set; } = "MXN";
        
        public int Status { get; set; } = (int)StatusDonacion.PENDING;
        public int Source { get; set; } = (int)SourceDonacion.Checkout;
        public string? Mensaje { get; set; }
        public bool Anonima { get; set; } = false;
    }
}
