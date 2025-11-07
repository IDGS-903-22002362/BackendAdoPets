namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class AdopcionLogDto
    {
        public Guid Id { get; set; }
        public Guid SolicitudId { get; set; }
        public string FromEstado { get; set; } = string.Empty;
        public string ToEstado { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }

    }
}
