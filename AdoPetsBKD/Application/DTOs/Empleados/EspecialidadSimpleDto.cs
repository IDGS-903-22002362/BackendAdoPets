namespace AdoPetsBKD.Application.DTOs.Empleados
{
    /// <summary>
    /// DTO simplificado para mostrar especialidad en listados
    /// </summary>
    public class EspecialidadSimpleDto
    {
        public Guid Id { get; set; }
        public string? Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
