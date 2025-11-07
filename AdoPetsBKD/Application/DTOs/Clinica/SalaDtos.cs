using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.Clinica;

public class CreateSalaDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Range(1, 50, ErrorMessage = "La capacidad debe estar entre 1 y 50")]
    public int Capacidad { get; set; } = 1;
}

public class UpdateSalaDto
{
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string? Nombre { get; set; }
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Range(1, 50, ErrorMessage = "La capacidad debe estar entre 1 y 50")]
    public int? Capacidad { get; set; }
    
    public bool? Activa { get; set; }
}

public class SalaListDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public bool Activa { get; set; }
}

public class SalaDetailDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public bool Activa { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
