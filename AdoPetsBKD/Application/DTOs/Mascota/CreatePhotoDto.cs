namespace AdoPetsBKD.Application.DTOs.Mascota
{
    public class CreatePhotoDto
    {
        public string StorageKey { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool EsPrincipal { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
