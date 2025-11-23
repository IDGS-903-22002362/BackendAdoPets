using System.Data;

namespace AdoPetsBKD.Application.DTOs.Donaciones
{
    public class CreatePayPalDonacionDto
    {
        public Guid? UsuarioId { get; set; }
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string Concepto { get; set; } = "Donación para AdoPets";
        public string? Mensaje { get; set; }
        public bool Anonima { get; set; } = false;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
    }
}
