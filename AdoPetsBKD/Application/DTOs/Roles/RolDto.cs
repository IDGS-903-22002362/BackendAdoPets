namespace AdoPetsBKD.Application.DTOs.Roles
{
    /// <summary>
    /// DTO simple para listar roles disponibles
    /// </summary>
    public class RolDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
