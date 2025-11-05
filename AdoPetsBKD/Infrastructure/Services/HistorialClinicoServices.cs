using AdoPetsBKD.Application.DTOs.HistorialClinico;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.HistorialClinico;

namespace AdoPetsBKD.Infrastructure.Services;

public class ExpedienteService : IExpedienteService
{
    private readonly IExpedienteRepository _expedienteRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public ExpedienteService(IExpedienteRepository expedienteRepository, IUsuarioRepository usuarioRepository)
    {
        _expedienteRepository = expedienteRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<ExpedienteDetailDto?> GetByIdAsync(Guid id)
    {
        var expediente = await _expedienteRepository.GetByIdAsync(id);
        if (expediente == null) return null;

        var veterinario = await _usuarioRepository.GetByIdAsync(expediente.VeterinarioId);
        return MapToDetailDto(expediente, veterinario);
    }

    public async Task<List<ExpedienteListDto>> GetByMascotaAsync(Guid mascotaId)
    {
        var expedientes = await _expedienteRepository.GetByMascotaAsync(mascotaId);
        var result = new List<ExpedienteListDto>();
        
        foreach (var e in expedientes)
        {
            var vet = await _usuarioRepository.GetByIdAsync(e.VeterinarioId);
            result.Add(MapToListDto(e, vet));
        }
        
        return result;
    }

    public async Task<List<ExpedienteListDto>> GetByVeterinarioAsync(Guid veterinarioId)
    {
        var expedientes = await _expedienteRepository.GetByVeterinarioAsync(veterinarioId);
        var vet = await _usuarioRepository.GetByIdAsync(veterinarioId);
        return expedientes.Select(e => MapToListDto(e, vet)).ToList();
    }

    public async Task<ExpedienteDetailDto> CreateAsync(CreateExpedienteDto dto, Guid userId)
    {
        var expediente = new Expediente
        {
            MascotaId = dto.MascotaId,
            VeterinarioId = dto.VeterinarioId,
            CitaId = dto.CitaId,
            MotivoConsulta = dto.MotivoConsulta,
            Anamnesis = dto.Anamnesis,
            Diagnostico = dto.Diagnostico,
            Tratamiento = dto.Tratamiento,
            Notas = dto.Notas,
            Pronostico = dto.Pronostico,
            Fecha = DateTime.UtcNow
        };

        await _expedienteRepository.AddAsync(expediente);

        var created = await _expedienteRepository.GetByIdAsync(expediente.Id);
        var veterinario = await _usuarioRepository.GetByIdAsync(dto.VeterinarioId);
        return MapToDetailDto(created!, veterinario);
    }

    public async Task DeleteAsync(Guid id)
    {
        var expediente = await _expedienteRepository.GetByIdAsync(id);
        if (expediente == null)
        {
            throw new KeyNotFoundException("Expediente no encontrado");
        }

        await _expedienteRepository.DeleteAsync(expediente);
    }

    private static ExpedienteListDto MapToListDto(Expediente e, Domain.Entities.Security.Usuario? vet)
    {
        return new ExpedienteListDto
        {
            Id = e.Id,
            MascotaId = e.MascotaId,
            MascotaNombre = e.Mascota?.Nombre ?? string.Empty,
            VeterinarioId = e.VeterinarioId,
            VeterinarioNombre = vet != null
                ? $"{vet.Nombre} {vet.ApellidoPaterno}"
                : "Veterinario",
            MotivoConsulta = e.MotivoConsulta,
            DiagnosticoResumido = e.Diagnostico.Length > 100
                ? e.Diagnostico.Substring(0, 100) + "..."
                : e.Diagnostico,
            Fecha = e.Fecha
        };
    }

    private static ExpedienteDetailDto MapToDetailDto(Expediente e, Domain.Entities.Security.Usuario? vet)
    {
        return new ExpedienteDetailDto
        {
            Id = e.Id,
            MascotaId = e.MascotaId,
            MascotaNombre = e.Mascota?.Nombre ?? string.Empty,
            VeterinarioId = e.VeterinarioId,
            VeterinarioNombre = vet != null
                ? $"{vet.Nombre} {vet.ApellidoPaterno}"
                : "Veterinario",
            CitaId = e.CitaId,
            MotivoConsulta = e.MotivoConsulta,
            Anamnesis = e.Anamnesis,
            Diagnostico = e.Diagnostico,
            Tratamiento = e.Tratamiento,
            Notas = e.Notas,
            Pronostico = e.Pronostico,
            Fecha = e.Fecha,
            Adjuntos = e.Adjuntos.Select(a => new AdjuntoMedicoDto
            {
                Id = a.Id,
                ExpedienteId = e.Id, // El adjunto no tiene ExpedienteId, usa EntryId
                TipoAdjunto = a.EntryType.ToString(),
                Url = a.StorageKey,
                FileName = a.FileName,
                Description = a.Descripcion,
                UploadedAt = a.UploadedAt
            }).ToList()
        };
    }
}

public class AdjuntoMedicoService : IAdjuntoMedicoService
{
    private readonly IAdjuntoMedicoRepository _adjuntoRepository;

    public AdjuntoMedicoService(IAdjuntoMedicoRepository adjuntoRepository)
    {
        _adjuntoRepository = adjuntoRepository;
    }

    public async Task<AdjuntoMedicoDto?> GetByIdAsync(Guid id)
    {
        var adjunto = await _adjuntoRepository.GetByIdAsync(id);
        return adjunto == null ? null : MapToDto(adjunto);
    }

    public async Task<List<AdjuntoMedicoDto>> GetByExpedienteAsync(Guid expedienteId)
    {
        var adjuntos = await _adjuntoRepository.GetByExpedienteAsync(expedienteId);
        return adjuntos.Select(MapToDto).ToList();
    }

    public async Task<AdjuntoMedicoDto> CreateAsync(CreateAdjuntoMedicoDto dto, Guid userId)
    {
        var adjunto = new AdjuntoMedico
        {
            MascotaId = Guid.Empty, // Esto debería venir del DTO o del Expediente
            EntryType = TipoEntryMedico.Expediente,
            EntryId = dto.ExpedienteId,
            StorageKey = dto.Url,
            FileName = dto.FileName ?? "archivo",
            MimeType = "application/octet-stream",
            Size = 0,
            Descripcion = dto.Description,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };

        await _adjuntoRepository.AddAsync(adjunto);

        var created = await _adjuntoRepository.GetByIdAsync(adjunto.Id);
        return MapToDto(created!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var adjunto = await _adjuntoRepository.GetByIdAsync(id);
        if (adjunto == null)
        {
            throw new KeyNotFoundException("Adjunto no encontrado");
        }

        await _adjuntoRepository.DeleteAsync(adjunto);
    }

    private static AdjuntoMedicoDto MapToDto(AdjuntoMedico a)
    {
        return new AdjuntoMedicoDto
        {
            Id = a.Id,
            ExpedienteId = a.EntryId ?? Guid.Empty,
            TipoAdjunto = a.EntryType.ToString(),
            Url = a.StorageKey,
            FileName = a.FileName,
            Description = a.Descripcion,
            UploadedAt = a.UploadedAt
        };
    }
}

public class VacunacionService : IVacunacionService
{
    private readonly IVacunacionRepository _vacunacionRepository;

    public VacunacionService(IVacunacionRepository vacunacionRepository)
    {
        _vacunacionRepository = vacunacionRepository;
    }

    public async Task<VacunacionDto?> GetByIdAsync(Guid id)
    {
        var vacunacion = await _vacunacionRepository.GetByIdAsync(id);
        return vacunacion == null ? null : MapToDto(vacunacion);
    }

    public async Task<List<VacunacionDto>> GetByMascotaAsync(Guid mascotaId)
    {
        var vacunaciones = await _vacunacionRepository.GetByMascotaAsync(mascotaId);
        return vacunaciones.Select(MapToDto).ToList();
    }

    public async Task<List<VacunacionDto>> GetUpcomingDueAsync(int days = 30)
    {
        var vacunaciones = await _vacunacionRepository.GetUpcomingDueAsync(days);
        return vacunaciones.Select(MapToDto).ToList();
    }

    public async Task<VacunacionDto> CreateAsync(CreateVacunacionDto dto, Guid userId)
    {
        var vacunacion = new Vacunacion
        {
            MascotaId = dto.MascotaId,
            VaccineName = dto.VaccineName,
            Dose = dto.Dose,
            Lot = dto.Lot,
            AppliedAt = DateTime.UtcNow,
            NextDueAt = dto.NextDueAt,
            VeterinarioId = dto.VeterinarioId,
            Notes = dto.Notes,
            ReaccionAdversa = dto.ReaccionAdversa
        };

        await _vacunacionRepository.AddAsync(vacunacion);

        var created = await _vacunacionRepository.GetByIdAsync(vacunacion.Id);
        return MapToDto(created!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var vacunacion = await _vacunacionRepository.GetByIdAsync(id);
        if (vacunacion == null)
        {
            throw new KeyNotFoundException("Vacunación no encontrada");
        }

        await _vacunacionRepository.DeleteAsync(vacunacion);
    }

    private static VacunacionDto MapToDto(Vacunacion v)
    {
        return new VacunacionDto
        {
            Id = v.Id,
            MascotaId = v.MascotaId,
            MascotaNombre = v.Mascota?.Nombre ?? string.Empty,
            VaccineName = v.VaccineName,
            Dose = v.Dose,
            Lot = v.Lot,
            AppliedAt = v.AppliedAt,
            NextDueAt = v.NextDueAt,
            VeterinarioId = v.VeterinarioId,
            VeterinarioNombre = "Veterinario", // No hay navigation property
            Notes = v.Notes,
            ReaccionAdversa = v.ReaccionAdversa
        };
    }
}

public class DesparasitacionService : IDesparasitacionService
{
    private readonly IDesparasitacionRepository _desparasitacionRepository;

    public DesparasitacionService(IDesparasitacionRepository desparasitacionRepository)
    {
        _desparasitacionRepository = desparasitacionRepository;
    }

    public async Task<DesparasitacionDto?> GetByIdAsync(Guid id)
    {
        var desparasitacion = await _desparasitacionRepository.GetByIdAsync(id);
        return desparasitacion == null ? null : MapToDto(desparasitacion);
    }

    public async Task<List<DesparasitacionDto>> GetByMascotaAsync(Guid mascotaId)
    {
        var desparasitaciones = await _desparasitacionRepository.GetByMascotaAsync(mascotaId);
        return desparasitaciones.Select(MapToDto).ToList();
    }

    public async Task<List<DesparasitacionDto>> GetUpcomingDueAsync(int days = 30)
    {
        var desparasitaciones = await _desparasitacionRepository.GetUpcomingDueAsync(days);
        return desparasitaciones.Select(MapToDto).ToList();
    }

    public async Task<DesparasitacionDto> CreateAsync(CreateDesparasitacionDto dto, Guid userId)
    {
        var desparasitacion = new Desparasitacion
        {
            MascotaId = dto.MascotaId,
            Product = dto.ProductName,
            Tipo = Enum.Parse<TipoDesparasitante>(dto.TipoParasito),
            AppliedAt = DateTime.UtcNow,
            NextDueAt = dto.NextDueAt,
            VeterinarioId = dto.VeterinarioId,
            Notes = dto.Notes
        };

        await _desparasitacionRepository.AddAsync(desparasitacion);

        var created = await _desparasitacionRepository.GetByIdAsync(desparasitacion.Id);
        return MapToDto(created!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var desparasitacion = await _desparasitacionRepository.GetByIdAsync(id);
        if (desparasitacion == null)
        {
            throw new KeyNotFoundException("Desparasitación no encontrada");
        }

        await _desparasitacionRepository.DeleteAsync(desparasitacion);
    }

    private static DesparasitacionDto MapToDto(Desparasitacion d)
    {
        return new DesparasitacionDto
        {
            Id = d.Id,
            MascotaId = d.MascotaId,
            MascotaNombre = d.Mascota?.Nombre ?? string.Empty,
            ProductName = d.Product,
            TipoParasito = d.Tipo.ToString(),
            AppliedAt = d.AppliedAt,
            NextDueAt = d.NextDueAt,
            VeterinarioId = d.VeterinarioId,
            VeterinarioNombre = "Veterinario", // No hay navigation property
            Peso = null, // La entidad no tiene Peso
            Notes = d.Notes
        };
    }
}

public class CirugiaService : ICirugiaService
{
    private readonly ICirugiaRepository _cirugiaRepository;

    public CirugiaService(ICirugiaRepository cirugiaRepository)
    {
        _cirugiaRepository = cirugiaRepository;
    }

    public async Task<CirugiaDto?> GetByIdAsync(Guid id)
    {
        var cirugia = await _cirugiaRepository.GetByIdAsync(id);
        return cirugia == null ? null : MapToDto(cirugia);
    }

    public async Task<List<CirugiaDto>> GetByMascotaAsync(Guid mascotaId)
    {
        var cirugias = await _cirugiaRepository.GetByMascotaAsync(mascotaId);
        return cirugias.Select(MapToDto).ToList();
    }

    public async Task<List<CirugiaDto>> GetByVeterinarioAsync(Guid veterinarioId)
    {
        var cirugias = await _cirugiaRepository.GetByVeterinarioAsync(veterinarioId);
        return cirugias.Select(MapToDto).ToList();
    }

    public async Task<CirugiaDto> CreateAsync(CreateCirugiaDto dto, Guid userId)
    {
        var cirugia = new Cirugia
        {
            MascotaId = dto.MascotaId,
            Tipo = dto.Tipo,
            Descripcion = dto.Descripcion,
            PerformedAt = DateTime.UtcNow,
            VeterinarioId = dto.VeterinarioId,
            Anesthesia = dto.Anesthesia,
            DuracionMin = dto.DuracionMin,
            Complications = dto.Complications,
            Notes = dto.Notes,
            Medicacion = dto.Medicacion,
            CuidadosPostoperatorios = dto.CuidadosPostoperatorios,
            FechaRevision = dto.FechaRevision
        };

        await _cirugiaRepository.AddAsync(cirugia);

        var created = await _cirugiaRepository.GetByIdAsync(cirugia.Id);
        return MapToDto(created!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var cirugia = await _cirugiaRepository.GetByIdAsync(id);
        if (cirugia == null)
        {
            throw new KeyNotFoundException("Cirugía no encontrada");
        }

        await _cirugiaRepository.DeleteAsync(cirugia);
    }

    private static CirugiaDto MapToDto(Cirugia c)
    {
        return new CirugiaDto
        {
            Id = c.Id,
            MascotaId = c.MascotaId,
            MascotaNombre = c.Mascota?.Nombre ?? string.Empty,
            Tipo = c.Tipo,
            Descripcion = c.Descripcion,
            PerformedAt = c.PerformedAt,
            VeterinarioId = c.VeterinarioId,
            VeterinarioNombre = "Veterinario", // No hay navigation property
            Anesthesia = c.Anesthesia,
            DuracionMin = c.DuracionMin,
            Complications = c.Complications,
            Notes = c.Notes,
            Medicacion = c.Medicacion,
            CuidadosPostoperatorios = c.CuidadosPostoperatorios,
            FechaRevision = c.FechaRevision
        };
    }
}

public class ValoracionService : IValoracionService
{
    private readonly IValoracionRepository _valoracionRepository;

    public ValoracionService(IValoracionRepository valoracionRepository)
    {
        _valoracionRepository = valoracionRepository;
    }

    public async Task<ValoracionDto?> GetByIdAsync(Guid id)
    {
        var valoracion = await _valoracionRepository.GetByIdAsync(id);
        return valoracion == null ? null : MapToDto(valoracion);
    }

    public async Task<List<ValoracionDto>> GetByMascotaAsync(Guid mascotaId)
    {
        var valoraciones = await _valoracionRepository.GetByMascotaAsync(mascotaId);
        return valoraciones.Select(MapToDto).ToList();
    }

    public async Task<ValoracionDto?> GetLatestByMascotaAsync(Guid mascotaId)
    {
        var valoracion = await _valoracionRepository.GetLatestByMascotaAsync(mascotaId);
        return valoracion == null ? null : MapToDto(valoracion);
    }

    public async Task<ValoracionDto> CreateAsync(CreateValoracionDto dto, Guid userId)
    {
        var valoracion = new Valoracion
        {
            CitaId = Guid.Empty, // Esto debería venir del DTO o ser nullable
            MascotaId = dto.MascotaId,
            Peso = dto.Peso,
            Temperatura = dto.Temperatura,
            FrecuenciaCardiaca = dto.FrecuenciaCardiaca,
            FrecuenciaRespiratoria = dto.FrecuenciaRespiratoria,
            CondicionCorporal = dto.CondicionCorporal?.ToString(),
            Observaciones = dto.Notas,
            TakenAt = DateTime.UtcNow,
            TakenBy = userId
        };

        await _valoracionRepository.AddAsync(valoracion);

        var created = await _valoracionRepository.GetByIdAsync(valoracion.Id);
        return MapToDto(created!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var valoracion = await _valoracionRepository.GetByIdAsync(id);
        if (valoracion == null)
        {
            throw new KeyNotFoundException("Valoración no encontrada");
        }

        await _valoracionRepository.DeleteAsync(valoracion);
    }

    private static ValoracionDto MapToDto(Valoracion v)
    {
        return new ValoracionDto
        {
            Id = v.Id,
            MascotaId = v.MascotaId,
            MascotaNombre = v.Mascota?.Nombre ?? string.Empty,
            VeterinarioId = v.TakenBy, // Usamos TakenBy como veterinario
            VeterinarioNombre = "Veterinario", // No hay navigation property
            Peso = v.Peso,
            Temperatura = v.Temperatura,
            FrecuenciaCardiaca = v.FrecuenciaCardiaca,
            FrecuenciaRespiratoria = v.FrecuenciaRespiratoria,
            CondicionCorporal = v.CondicionCorporal,
            Fecha = v.TakenAt,
            Notas = v.Observaciones
        };
    }
}
