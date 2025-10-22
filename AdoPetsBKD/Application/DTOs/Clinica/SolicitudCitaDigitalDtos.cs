namespace AdoPetsBKD.Application.DTOs.Clinica;

public class CreateSolicitudCitaDigitalDto
{
    public Guid SolicitanteId { get; set; }
    public Guid? MascotaId { get; set; }
    public string NombreMascota { get; set; } = string.Empty;
    public string? EspecieMascota { get; set; }
    public string? RazaMascota { get; set; }
    public Guid? ServicioId { get; set; }
    public string DescripcionServicio { get; set; } = string.Empty;
    public string? MotivoConsulta { get; set; }
    public DateTime FechaHoraSolicitada { get; set; }
    public int DuracionEstimadaMin { get; set; } = 60;
    public Guid? VeterinarioPreferidoId { get; set; }
    public Guid? SalaPreferidaId { get; set; }
    public decimal CostoEstimado { get; set; }
}

public class SolicitudCitaDigitalDto
{
    public Guid Id { get; set; }
    public string NumeroSolicitud { get; set; } = string.Empty;
    public Guid SolicitanteId { get; set; }
    public string NombreSolicitante { get; set; } = string.Empty;
    public string EmailSolicitante { get; set; } = string.Empty;
    public Guid? MascotaId { get; set; }
    public string NombreMascota { get; set; } = string.Empty;
    public string? EspecieMascota { get; set; }
    public string? RazaMascota { get; set; }
    public Guid? ServicioId { get; set; }
    public string DescripcionServicio { get; set; } = string.Empty;
    public string? MotivoConsulta { get; set; }
    public DateTime FechaHoraSolicitada { get; set; }
    public int DuracionEstimadaMin { get; set; }
    public Guid? VeterinarioPreferidoId { get; set; }
    public string? NombreVeterinarioPreferido { get; set; }
    public decimal CostoEstimado { get; set; }
    public decimal MontoAnticipo { get; set; }
    public int Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRevision { get; set; }
    public DateTime? FechaConfirmacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public Guid? PagoAnticipoId { get; set; }
    public Guid? CitaId { get; set; }
    public bool DisponibilidadVerificada { get; set; }
}

public class ConfirmarSolicitudCitaDto
{
    public Guid SolicitudId { get; set; }
    public Guid ConfirmadoPorId { get; set; }
    public Guid VeterinarioId { get; set; }
    public Guid? SalaId { get; set; }
    public DateTime FechaHoraConfirmada { get; set; }
    public int DuracionMin { get; set; }
}

public class RechazarSolicitudCitaDto
{
    public Guid SolicitudId { get; set; }
    public Guid RechazadoPorId { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public class VerificarDisponibilidadDto
{
    public DateTime FechaHoraInicio { get; set; }
    public int DuracionMin { get; set; }
    public Guid? VeterinarioId { get; set; }
    public Guid? SalaId { get; set; }
}

public class DisponibilidadResponseDto
{
    public bool Disponible { get; set; }
    public string? Mensaje { get; set; }
    public List<ConflictoDto> Conflictos { get; set; } = new();
}

public class ConflictoDto
{
    public string Tipo { get; set; } = string.Empty; // "Veterinario", "Sala"
    public DateTime HoraInicio { get; set; }
    public DateTime HoraFin { get; set; }
    public string? Descripcion { get; set; }
}
