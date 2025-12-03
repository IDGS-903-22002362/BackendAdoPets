using System.ComponentModel.DataAnnotations;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Application.DTOs.Clinica;

// Request DTOs
public class CreateCitaDto
{
    public Guid? SolicitudCitaDigitalId { get; set; }
    
    public Guid? MascotaId { get; set; }
    public Guid? PropietarioId { get; set; }
    
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    public Guid? SalaId { get; set; }
    
    [Required(ErrorMessage = "El tipo de cita es requerido")]
    public TipoCita Tipo { get; set; }
    
    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime StartAt { get; set; }
    
    [Required(ErrorMessage = "La duración es requerida")]
    [Range(15, 480, ErrorMessage = "La duración debe estar entre 15 minutos y 8 horas")]
    public int DuracionMin { get; set; }
    
    public string? Notas { get; set; }
    public string? MotivoConsulta { get; set; }
}

public class UpdateCitaDto
{
    public Guid? MascotaId { get; set; }
    public Guid? PropietarioId { get; set; }
    public Guid? VeterinarioId { get; set; }
    public Guid? SalaId { get; set; }
    public TipoCita? Tipo { get; set; }
    public DateTime? StartAt { get; set; }
    public int? DuracionMin { get; set; }
    public string? Notas { get; set; }
    public string? MotivoConsulta { get; set; }
}

public class CancelarCitaDto
{
    [Required(ErrorMessage = "El motivo de cancelación es requerido")]
    [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    public string MotivoRechazo { get; set; } = string.Empty;
}

public class CompletarCitaDto
{
    public string? Notas { get; set; }
}

// Response DTOs
public class CitaListDto
{
    public Guid Id { get; set; }
    public Guid? MascotaId { get; set; }
    public string? MascotaNombre { get; set; }
    public Guid? PropietarioId { get; set; }
    public string? PropietarioNombre { get; set; }
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public Guid? SalaId { get; set; }
    public string? SalaNombre { get; set; }
    public TipoCita Tipo { get; set; }
    public StatusCita Status { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int DuracionMin { get; set; }
}

public class CitaDetailDto
{
    public Guid Id { get; set; }
    public Guid? MascotaId { get; set; }
    public string? MascotaNombre { get; set; }
    public Guid? PropietarioId { get; set; }
    public string? PropietarioNombre { get; set; }
    public string? PropietarioEmail { get; set; }
    public string? PropietarioTelefono { get; set; }
    public Guid VeterinarioId { get; set; }
    public string VeterinarioNombre { get; set; } = string.Empty;
    public string? VeterinarioEmail { get; set; }
    public Guid? SalaId { get; set; }
    public string? SalaNombre { get; set; }
    public TipoCita Tipo { get; set; }
    public StatusCita Status { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int DuracionMin { get; set; }
    public string? Notas { get; set; }
    public string? MotivoConsulta { get; set; }
    public string? MotivoRechazo { get; set; }
    public Guid? PagoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CitaRecordatorioDto> Recordatorios { get; set; } = new();
    public List<CitaHistorialEstadoDto> Historial { get; set; } = new();
}

public class CitaRecordatorioDto
{
    public Guid Id { get; set; }
    public string TipoRecordatorio { get; set; } = string.Empty;
    public int MinutosAntes { get; set; }
    public bool Enviado { get; set; }
    public DateTime? EnviadoAt { get; set; }
    public string? Error { get; set; }
}

public class CitaHistorialEstadoDto
{
    public Guid Id { get; set; }
    public StatusCita FromStatus { get; set; }
    public StatusCita ToStatus { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByNombre { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? Notas { get; set; }
}

public class DisponibilidadQueryDto
{
    [Required(ErrorMessage = "El veterinario es requerido")]
    public Guid VeterinarioId { get; set; }
    
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Fecha { get; set; }
    
    public Guid? SalaId { get; set; }
}

public class DisponibilidadResponseDto
{
    public DateTime Fecha { get; set; }
    public List<HorarioDisponibleDto> HorariosDisponibles { get; set; } = new();
}

public class HorarioDisponibleDto
{
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public bool Disponible { get; set; }
    public string? Motivo { get; set; }
}
