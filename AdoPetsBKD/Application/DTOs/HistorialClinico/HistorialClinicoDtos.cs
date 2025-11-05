using System.ComponentModel.DataAnnotations;

namespace AdoPetsBKD.Application.DTOs.HistorialClinico;

public enum CondicionCorporal
{
    MuyDelgado,
    Delgado,
    Ideal,
    Sobrepeso,
    Obeso
}

// Expediente DTOs
public class CreateExpedienteDto
{
    [Required(ErrorMessage = "La mascota es requerida")]
    public Guid MascotaId { get; set; }
    
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    public Guid? CitaId { get; set; }
    
    [StringLength(1000, ErrorMessage = "El motivo de consulta no puede exceder 1000 caracteres")]
    public string? MotivoConsulta { get; set; }
    
    [StringLength(2000, ErrorMessage = "La anamnesis no puede exceder 2000 caracteres")]
    public string? Anamnesis { get; set; }
    
    [Required(ErrorMessage = "El diagnóstico es requerido")]
    [StringLength(2000, ErrorMessage = "El diagnóstico no puede exceder 2000 caracteres")]
    public string Diagnostico { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "El tratamiento no puede exceder 2000 caracteres")]
    public string? Tratamiento { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notas { get; set; }
    
    [StringLength(500, ErrorMessage = "El pronóstico no puede exceder 500 caracteres")]
    public string? Pronostico { get; set; }
}

public class ExpedienteListDto
{
    public Guid Id { get; set; }
    public Guid MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public string? MotivoConsulta { get; set; }
    public string DiagnosticoResumido { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public class ExpedienteDetailDto
{
    public Guid Id { get; set; }
    public Guid MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public Guid? CitaId { get; set; }
    public string? MotivoConsulta { get; set; }
    public string? Anamnesis { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string? Tratamiento { get; set; }
    public string? Notas { get; set; }
    public string? Pronostico { get; set; }
    public DateTime Fecha { get; set; }
    public List<AdjuntoMedicoDto> Adjuntos { get; set; } = new();
}

// Adjunto Médico DTOs
public class CreateAdjuntoMedicoDto
{
    [Required(ErrorMessage = "El expediente es requerido")]
    public Guid ExpedienteId { get; set; }
    
    [Required(ErrorMessage = "El tipo de adjunto es requerido")]
    public string TipoAdjunto { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La URL es requerida")]
    [Url(ErrorMessage = "La URL no es válida")]
    public string Url { get; set; } = string.Empty;
    
    [StringLength(255, ErrorMessage = "El nombre del archivo no puede exceder 255 caracteres")]
    public string? FileName { get; set; }
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }
}

public class AdjuntoMedicoDto
{
    public Guid Id { get; set; }
    public Guid ExpedienteId { get; set; }
    public string TipoAdjunto { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
}

// Vacunación DTOs
public class CreateVacunacionDto
{
    [Required(ErrorMessage = "La mascota es requerida")]
    public Guid MascotaId { get; set; }
    
    [Required(ErrorMessage = "El nombre de la vacuna es requerido")]
    [StringLength(200, ErrorMessage = "El nombre de la vacuna no puede exceder 200 caracteres")]
    public string VaccineName { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "La dosis no puede exceder 100 caracteres")]
    public string? Dose { get; set; }
    
    [StringLength(50, ErrorMessage = "El lote no puede exceder 50 caracteres")]
    public string? Lot { get; set; }
    
    public DateTime? NextDueAt { get; set; }
    
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notes { get; set; }
    
    [StringLength(500, ErrorMessage = "La reacción adversa no puede exceder 500 caracteres")]
    public string? ReaccionAdversa { get; set; }
}

public class VacunacionDto
{
    public Guid Id { get; set; }
    public Guid MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public string VaccineName { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public string? Lot { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? NextDueAt { get; set; }
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? ReaccionAdversa { get; set; }
}

// Desparasitación DTOs
public class CreateDesparasitacionDto
{
    [Required(ErrorMessage = "La mascota es requerida")]
    public Guid MascotaId { get; set; }
    
    [Required(ErrorMessage = "El nombre del producto es requerido")]
    [StringLength(200, ErrorMessage = "El nombre del producto no puede exceder 200 caracteres")]
    public string ProductName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El tipo de parásito es requerido")]
    public string TipoParasito { get; set; } = string.Empty;
    
    public DateTime? NextDueAt { get; set; }
    
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    [Range(0.1, 200, ErrorMessage = "El peso debe estar entre 0.1 y 200 kg")]
    public decimal? Peso { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notes { get; set; }
}

public class DesparasitacionDto
{
    public Guid Id { get; set; }
    public Guid MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string TipoParasito { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? NextDueAt { get; set; }
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public decimal? Peso { get; set; }
    public string? Notes { get; set; }
}

// Cirugía DTOs
public class CreateCirugiaDto
{
    [Required(ErrorMessage = "La mascota es requerida")]
    public Guid MascotaId { get; set; }
    
    [Required(ErrorMessage = "El tipo de cirugía es requerido")]
    [StringLength(200, ErrorMessage = "El tipo no puede exceder 200 caracteres")]
    public string Tipo { get; set; } = string.Empty;
    
    [StringLength(2000, ErrorMessage = "La descripción no puede exceder 2000 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    [StringLength(500, ErrorMessage = "La anestesia no puede exceder 500 caracteres")]
    public string? Anesthesia { get; set; }
    
    [Range(1, 1440, ErrorMessage = "La duración debe estar entre 1 y 1440 minutos")]
    public int? DuracionMin { get; set; }
    
    public bool Complications { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notes { get; set; }
    
    [StringLength(1000, ErrorMessage = "La medicación no puede exceder 1000 caracteres")]
    public string? Medicacion { get; set; }
    
    [StringLength(1000, ErrorMessage = "Los cuidados no pueden exceder 1000 caracteres")]
    public string? CuidadosPostoperatorios { get; set; }
    
    public DateTime? FechaRevision { get; set; }
}

public class CirugiaDto
{
    public Guid Id { get; set; }
    public Guid MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime PerformedAt { get; set; }
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public string? Anesthesia { get; set; }
    public int? DuracionMin { get; set; }
    public bool Complications { get; set; }
    public string? Notes { get; set; }
    public string? Medicacion { get; set; }
    public string? CuidadosPostoperatorios { get; set; }
    public DateTime? FechaRevision { get; set; }
}

// Valoración DTOs
public class CreateValoracionDto
{
    [Required(ErrorMessage = "La mascota es requerida")]
    public Guid MascotaId { get; set; }
    
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    [Range(0.1, 200, ErrorMessage = "El peso debe estar entre 0.1 y 200 kg")]
    public decimal? Peso { get; set; }
    
    [Range(35, 42, ErrorMessage = "La temperatura debe estar entre 35 y 42 °C")]
    public decimal? Temperatura { get; set; }
    
    [Range(40, 220, ErrorMessage = "La frecuencia cardíaca debe estar entre 40 y 220 lpm")]
    public int? FrecuenciaCardiaca { get; set; }
    
    [Range(10, 60, ErrorMessage = "La frecuencia respiratoria debe estar entre 10 y 60 rpm")]
    public int? FrecuenciaRespiratoria { get; set; }
    
    public CondicionCorporal? CondicionCorporal { get; set; }
    
    [StringLength(1000, ErrorMessage = "Las notas no pueden exceder 1000 caracteres")]
    public string? Notas { get; set; }
}

public class ValoracionDto
{
    public Guid Id { get; set; }
    public Guid MascotaId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public decimal? Peso { get; set; }
    public decimal? Temperatura { get; set; }
    public int? FrecuenciaCardiaca { get; set; }
    public int? FrecuenciaRespiratoria { get; set; }
    public string? CondicionCorporal { get; set; }
    public DateTime Fecha { get; set; }
    public string? Notas { get; set; }
}
