using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using DomainStatusCita = AdoPetsBKD.Domain.Entities.Clinica.StatusCita;
using DomainTipoCita = AdoPetsBKD.Domain.Entities.Clinica.TipoCita;

namespace AdoPetsBKD.Infrastructure.Services;

public class CitaService : ICitaService
{
    private readonly ICitaRepository _citaRepository;
    private readonly ISalaRepository _salaRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public CitaService(
        ICitaRepository citaRepository,
        ISalaRepository salaRepository,
        IUsuarioRepository usuarioRepository)
    {
        _citaRepository = citaRepository;
        _salaRepository = salaRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<CitaDetailDto?> GetByIdAsync(Guid id)
    {
        var cita = await _citaRepository.GetByIdAsync(id);
        return cita == null ? null : MapToDetailDto(cita);
    }

    public async Task<List<CitaListDto>> GetAllAsync()
    {
        var citas = await _citaRepository.GetAllAsync();
        return citas.Select(MapToListDto).ToList();
    }

    public async Task<List<CitaListDto>> GetByVeterinarioAsync(Guid veterinarioId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var citas = await _citaRepository.GetByVeterinarioAsync(veterinarioId, startDate, endDate);
        return citas.Select(MapToListDto).ToList();
    }

    public async Task<List<CitaListDto>> GetByMascotaAsync(Guid mascotaId)
    {
        var citas = await _citaRepository.GetByMascotaAsync(mascotaId);
        return citas.Select(MapToListDto).ToList();
    }

    public async Task<List<CitaListDto>> GetByPropietarioAsync(Guid propietarioId)
    {
        var citas = await _citaRepository.GetByPropietarioAsync(propietarioId);
        return citas.Select(MapToListDto).ToList();
    }

    public async Task<List<CitaListDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var citas = await _citaRepository.GetByDateRangeAsync(startDate, endDate);
        return citas.Select(MapToListDto).ToList();
    }

    public async Task<List<CitaListDto>> GetByStatusAsync(DomainStatusCita status)
    {
        var citas = await _citaRepository.GetByStatusAsync(status);
        return citas.Select(MapToListDto).ToList();
    }

    public async Task<CitaDetailDto> CreateAsync(CreateCitaDto dto, Guid userId)
    {
        // Validar que al menos mascota o propietario esté presente
        if (!dto.MascotaId.HasValue && !dto.PropietarioId.HasValue)
        {
            throw new ArgumentException("Debe especificar una mascota o un propietario");
        }

        // Calcular EndAt
        var endAt = dto.StartAt.AddMinutes(dto.DuracionMin);

        // Validar solapamiento de veterinario
        var hasOverlap = await _citaRepository.HasOverlappingAppointmentAsync(
            dto.VeterinarioId,
            dto.StartAt,
            endAt);

        if (hasOverlap)
        {
            throw new InvalidOperationException("El veterinario ya tiene una cita en ese horario");
        }

        // Validar solapamiento de sala si se especificó
        if (dto.SalaId.HasValue)
        {
            var hasSalaOverlap = await _citaRepository.HasSalaOverlappingAsync(
                dto.SalaId.Value,
                dto.StartAt,
                endAt);

            if (hasSalaOverlap)
            {
                throw new InvalidOperationException("La sala ya está ocupada en ese horario");
            }
        }

        var cita = new Cita
        {
            MascotaId = dto.MascotaId,
            PropietarioId = dto.PropietarioId,
            VeterinarioId = dto.VeterinarioId,
            SalaId = dto.SalaId,
            Tipo = dto.Tipo,
            Status = DomainStatusCita.Programada,
            StartAt = dto.StartAt,
            EndAt = endAt,
            DuracionMin = dto.DuracionMin,
            Notas = dto.Notas,
            MotivoConsulta = dto.MotivoConsulta,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        // Crear recordatorios automáticos
        cita.Recordatorios = new List<CitaRecordatorio>
        {
            new CitaRecordatorio
            {
                Tipo = TipoRecordatorio.Horas24,
                SentAt = null
            },
            new CitaRecordatorio
            {
                Tipo = TipoRecordatorio.Hora1,
                SentAt = null
            }
        };

        // Crear entrada en historial de estado
        cita.HistorialEstados = new List<CitaHistorialEstado>
        {
            new CitaHistorialEstado
            {
                FromStatus = DomainStatusCita.Programada,
                ToStatus = DomainStatusCita.Programada,
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow,
                Notas = "Cita creada"
            }
        };

        await _citaRepository.AddAsync(cita);

        var createdCita = await _citaRepository.GetByIdAsync(cita.Id);
        return MapToDetailDto(createdCita!);
    }

    public async Task<CitaDetailDto> UpdateAsync(Guid id, UpdateCitaDto dto, Guid userId)
    {
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new KeyNotFoundException("Cita no encontrada");
        }

        if (cita.Status == DomainStatusCita.Completada || cita.Status == DomainStatusCita.Cancelada)
        {
            throw new InvalidOperationException("No se puede actualizar una cita completada o cancelada");
        }

        var needsOverlapCheck = false;
        var newStartAt = cita.StartAt;
        var newEndAt = cita.EndAt;
        var newVetId = cita.VeterinarioId;
        var newSalaId = cita.SalaId;

        if (dto.StartAt.HasValue || dto.DuracionMin.HasValue || dto.VeterinarioId.HasValue || dto.SalaId.HasValue)
        {
            needsOverlapCheck = true;

            if (dto.StartAt.HasValue)
                newStartAt = dto.StartAt.Value;

            if (dto.DuracionMin.HasValue)
                newEndAt = newStartAt.AddMinutes(dto.DuracionMin.Value);

            if (dto.VeterinarioId.HasValue)
                newVetId = dto.VeterinarioId.Value;

            if (dto.SalaId.HasValue)
                newSalaId = dto.SalaId.Value;
        }

        if (needsOverlapCheck)
        {
            // Validar solapamiento de veterinario
            var hasOverlap = await _citaRepository.HasOverlappingAppointmentAsync(
                newVetId,
                newStartAt,
                newEndAt,
                id);

            if (hasOverlap)
            {
                throw new InvalidOperationException("El veterinario ya tiene una cita en ese horario");
            }

            // Validar solapamiento de sala
            if (newSalaId.HasValue)
            {
                var hasSalaOverlap = await _citaRepository.HasSalaOverlappingAsync(
                    newSalaId.Value,
                    newStartAt,
                    newEndAt,
                    id);

                if (hasSalaOverlap)
                {
                    throw new InvalidOperationException("La sala ya está ocupada en ese horario");
                }
            }
        }

        // Aplicar cambios
        if (dto.MascotaId.HasValue)
            cita.MascotaId = dto.MascotaId.Value;

        if (dto.PropietarioId.HasValue)
            cita.PropietarioId = dto.PropietarioId.Value;

        if (dto.VeterinarioId.HasValue)
            cita.VeterinarioId = dto.VeterinarioId.Value;

        if (dto.SalaId.HasValue)
            cita.SalaId = dto.SalaId;

        if (dto.Tipo.HasValue)
            cita.Tipo = dto.Tipo.Value;

        if (dto.StartAt.HasValue)
            cita.StartAt = dto.StartAt.Value;

        if (dto.DuracionMin.HasValue)
        {
            cita.DuracionMin = dto.DuracionMin.Value;
            cita.EndAt = cita.StartAt.AddMinutes(dto.DuracionMin.Value);
        }

        if (dto.Notas != null)
            cita.Notas = dto.Notas;

        if (dto.MotivoConsulta != null)
            cita.MotivoConsulta = dto.MotivoConsulta;

        cita.UpdatedAt = DateTime.UtcNow;
        cita.UpdatedBy = userId;

        await _citaRepository.UpdateAsync(cita);

        var updatedCita = await _citaRepository.GetByIdAsync(id);
        return MapToDetailDto(updatedCita!);
    }

    public async Task<CitaDetailDto> CancelarAsync(Guid id, CancelarCitaDto dto, Guid userId)
    {
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new KeyNotFoundException("Cita no encontrada");
        }

        if (cita.Status == DomainStatusCita.Completada)
        {
            throw new InvalidOperationException("No se puede cancelar una cita completada");
        }

        if (cita.Status == DomainStatusCita.Cancelada)
        {
            throw new InvalidOperationException("La cita ya está cancelada");
        }

        var oldStatus = cita.Status;
        cita.Status = DomainStatusCita.Cancelada;
        cita.MotivoRechazo = dto.MotivoRechazo;
        cita.UpdatedAt = DateTime.UtcNow;
        cita.UpdatedBy = userId;

        // Agregar al historial
        cita.HistorialEstados.Add(new CitaHistorialEstado
        {
            CitaId = cita.Id,
            FromStatus = oldStatus,
            ToStatus = DomainStatusCita.Cancelada,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow,
            Notas = dto.MotivoRechazo
        });

        await _citaRepository.UpdateAsync(cita);

        var updatedCita = await _citaRepository.GetByIdAsync(id);
        return MapToDetailDto(updatedCita!);
    }

    public async Task<CitaDetailDto> CompletarAsync(Guid id, CompletarCitaDto dto, Guid userId)
    {
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new KeyNotFoundException("Cita no encontrada");
        }

        if (cita.Status == DomainStatusCita.Cancelada)
        {
            throw new InvalidOperationException("No se puede completar una cita cancelada");
        }

        if (cita.Status == DomainStatusCita.Completada)
        {
            throw new InvalidOperationException("La cita ya está completada");
        }

        var oldStatus = cita.Status;
        cita.Status = DomainStatusCita.Completada;
        
        if (dto.Notas != null)
            cita.Notas = dto.Notas;

        cita.UpdatedAt = DateTime.UtcNow;
        cita.UpdatedBy = userId;

        // Agregar al historial
        cita.HistorialEstados.Add(new CitaHistorialEstado
        {
            CitaId = cita.Id,
            FromStatus = oldStatus,
            ToStatus = DomainStatusCita.Completada,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow,
            Notas = dto.Notas
        });

        await _citaRepository.UpdateAsync(cita);

        var updatedCita = await _citaRepository.GetByIdAsync(id);
        return MapToDetailDto(updatedCita!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var cita = await _citaRepository.GetByIdAsync(id);
        if (cita == null)
        {
            throw new KeyNotFoundException("Cita no encontrada");
        }

        await _citaRepository.DeleteAsync(cita);
    }

    public async Task<DisponibilidadResponseDto> GetDisponibilidadAsync(DisponibilidadQueryDto query)
    {
        // Obtener citas del veterinario para el día especificado
        var startOfDay = query.Fecha.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var citas = await _citaRepository.GetByVeterinarioAsync(
            query.VeterinarioId,
            startOfDay,
            endOfDay);

        // Generar slots de 30 minutos desde las 8:00 hasta las 18:00
        var horariosDisponibles = new List<HorarioDisponibleDto>();
        var horaActual = new TimeSpan(8, 0, 0);
        var horaFin = new TimeSpan(18, 0, 0);

        while (horaActual < horaFin)
        {
            var slotStart = startOfDay.Add(horaActual);
            var slotEnd = slotStart.AddMinutes(30);

            // Verificar si hay cita en este horario
            var tieneConflicto = citas.Any(c =>
                c.Status != DomainStatusCita.Cancelada &&
                c.Status != DomainStatusCita.NoAsistio &&
                c.StartAt < slotEnd &&
                c.EndAt > slotStart);

            horariosDisponibles.Add(new HorarioDisponibleDto
            {
                HoraInicio = horaActual,
                HoraFin = horaActual.Add(TimeSpan.FromMinutes(30)),
                Disponible = !tieneConflicto,
                Motivo = tieneConflicto ? "Ocupado" : null
            });

            horaActual = horaActual.Add(TimeSpan.FromMinutes(30));
        }

        return new DisponibilidadResponseDto
        {
            Fecha = query.Fecha,
            HorariosDisponibles = horariosDisponibles
        };
    }

    public async Task<bool> HasOverlappingAppointmentAsync(Guid veterinarioId, DateTime startAt, DateTime endAt, Guid? excludeCitaId = null)
    {
        return await _citaRepository.HasOverlappingAppointmentAsync(veterinarioId, startAt, endAt, excludeCitaId);
    }

    // Mappers
    private static CitaListDto MapToListDto(Cita cita)
    {
        return new CitaListDto
        {
            Id = cita.Id,
            MascotaId = cita.MascotaId,
            MascotaNombre = cita.Mascota?.Nombre,
            PropietarioId = cita.PropietarioId,
            PropietarioNombre = cita.Propietario != null
                ? $"{cita.Propietario.Nombre} {cita.Propietario.ApellidoPaterno}"
                : null,
            VeterinarioId = cita.VeterinarioId,
            VeterinarioNombre = $"{cita.Veterinario.Nombre} {cita.Veterinario.ApellidoPaterno}",
            SalaId = cita.SalaId,
            SalaNombre = cita.Sala?.Nombre,
            Tipo = cita.Tipo,
            Status = cita.Status,
            StartAt = cita.StartAt,
            EndAt = cita.EndAt,
            DuracionMin = cita.DuracionMin
        };
    }

    private static CitaDetailDto MapToDetailDto(Cita cita)
    {
        return new CitaDetailDto
        {
            Id = cita.Id,
            MascotaId = cita.MascotaId,
            MascotaNombre = cita.Mascota?.Nombre,
            PropietarioId = cita.PropietarioId,
            PropietarioNombre = cita.Propietario != null
                ? $"{cita.Propietario.Nombre} {cita.Propietario.ApellidoPaterno}"
                : null,
            PropietarioEmail = cita.Propietario?.Email,
            PropietarioTelefono = cita.Propietario?.Telefono,
            VeterinarioId = cita.VeterinarioId,
            VeterinarioNombre = $"{cita.Veterinario.Nombre} {cita.Veterinario.ApellidoPaterno}",
            VeterinarioEmail = cita.Veterinario.Email,
            SalaId = cita.SalaId,
            SalaNombre = cita.Sala?.Nombre,
            Tipo = cita.Tipo,
            Status = cita.Status,
            StartAt = cita.StartAt,
            EndAt = cita.EndAt,
            DuracionMin = cita.DuracionMin,
            Notas = cita.Notas,
            MotivoConsulta = cita.MotivoConsulta,
            MotivoRechazo = cita.MotivoRechazo,
            PagoId = cita.PagoId,
            CreatedAt = cita.CreatedAt,
            UpdatedAt = cita.UpdatedAt,
            Recordatorios = cita.Recordatorios.Select(r => new CitaRecordatorioDto
            {
                Id = r.Id,
                TipoRecordatorio = r.Tipo.ToString(),
                MinutosAntes = r.Tipo == TipoRecordatorio.Horas24 ? 1440 : r.Tipo == TipoRecordatorio.Horas2 ? 120 : 60,
                Enviado = r.WasSent,
                EnviadoAt = r.SentAt,
                Error = null
            }).ToList(),
            Historial = cita.HistorialEstados.Select(h => new CitaHistorialEstadoDto
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                ChangedBy = h.ChangedBy,
                ChangedByNombre = "Usuario", // No hay navigation property
                ChangedAt = h.ChangedAt,
                Notas = h.Notas
            }).ToList()
        };
    }
}
