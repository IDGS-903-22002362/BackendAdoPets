namespace AdoPetsBKD.Application.DTOs.Mascota
{

    //Dto Para subir foto asociada a la mascota
    public class AddMascotaFotoDto
    {
        public string StorageKey { get; set; } 
        public string MimeType { get; set; } = string.Empty;
        public int Orden { get; set; }
        public bool EsPrincipal { get; set; }
    }
}
