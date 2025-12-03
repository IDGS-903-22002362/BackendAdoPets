using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DomainStatusCita = AdoPetsBKD.Domain.Entities.Clinica.StatusCita;
using DomainTipoCita = AdoPetsBKD.Domain.Entities.Clinica.TipoCita;

namespace AdoPetsBKD.Infrastructure.Services;

public class CitaService : ICitaService
{
    private readonly ICitaRepository _citaRepository;
    private readonly ISalaRepository _salaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly AdoPetsDbContext _context;
    private readonly ILogger<CitaService> _logger;

    public CitaService(
        ICitaRepository citaRepository,
        ISalaRepository salaRepository,
        IUsuarioRepository usuarioRepository,
        AdoPetsDbContext context,
        ILogger<CitaService> logger)
    {
        _citaRepository = citaRepository;
        _salaRepository = salaRepository;
        _usuarioRepository = usuarioRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<CitaDetailDto?> GetByIdAsync(Guid id)
    {
        var cita = await _citaRepository.GetByIdAsync(id);
        return cita == null ? null : MapToDetailDto(cita);
    }

    public async Task<CitaDetailDto?> GetBySolicitudDigitalAsync(Guid solicitudId)
    {
        var solicitud = await _context.SolicitudesCitasDigitales
            .FirstOrDefaultAsync(s => s.Id == solicitudId);

        if (solicitud == null || !solicitud.CitaId.HasValue)
        {
            return null;
        }

        var cita = await _citaRepository.GetByIdAsync(solicitud.CitaId.Value);
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
        try
        {
            _logger.LogInformation("=== INICIO VALIDACIÓN CITA ===");
            _logger.LogInformation("VeterinarioId recibido: {VetId}", dto.VeterinarioId);
            _logger.LogInformation("MascotaId: {MascotaId}", dto.MascotaId);
            _logger.LogInformation("PropietarioId: {PropId}", dto.PropietarioId);
            _logger.LogInformation("SalaId: {SalaId}", dto.SalaId);

            // Validar que al menos mascota o propietario esté presente
            if (!dto.MascotaId.HasValue && !dto.PropietarioId.HasValue)
            {
                throw new ArgumentException("Debe especificar una mascota o un propietario");
            }

            // ?? NUEVO: Determinar si el VeterinarioId es un UsuarioId o un EmpleadoId
            // y obtener ambos IDs correctamente
            Guid veterinarioUsuarioId;
            Guid veterinarioEmpleadoId;

            _logger.LogInformation("Buscando empleado con ID: {VetId}", dto.VeterinarioId);
            var empleado = await _context.Empleados
                .AsNoTracking()
                .Include(e => e.Usuario)
                .ThenInclude(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
                .FirstOrDefaultAsync(e => e.Id == dto.VeterinarioId);

            if (empleado != null)
            {
                // El ID recibido es un EmpleadoId
                _logger.LogInformation("ID recibido es EmpleadoId. EmpleadoId: {EmpId}, UsuarioId: {UserId}", 
                    empleado.Id, empleado.UsuarioId);
                veterinarioEmpleadoId = empleado.Id;
                veterinarioUsuarioId = empleado.UsuarioId;

                // Validar que el empleado tiene el rol de veterinario
                var tieneRolVeterinario = empleado.Usuario.UsuarioRoles.Any(ur => 
                    ur.Rol != null && (ur.Rol.Nombre == "Veterinario" || ur.Rol.Nombre == "Admin"));
                if (!tieneRolVeterinario)
                {
                    throw new ArgumentException($"El empleado {empleado.Id} no tiene el rol de Veterinario");
                }
            }
            else
            {
                // El ID recibido podría ser un UsuarioId, buscar el empleado asociado
                _logger.LogInformation("No se encontró empleado con ese ID, buscando por UsuarioId...");
                var empleadoPorUsuario = await _context.Empleados
                    .AsNoTracking()
                    .Include(e => e.Usuario)
                    .ThenInclude(u => u.UsuarioRoles)
                    .ThenInclude(ur => ur.Rol)
                    .FirstOrDefaultAsync(e => e.UsuarioId == dto.VeterinarioId);
                
                if (empleadoPorUsuario != null)
                {
                    _logger.LogInformation("ID recibido es UsuarioId. EmpleadoId: {EmpId}, UsuarioId: {UserId}", 
                        empleadoPorUsuario.Id, empleadoPorUsuario.UsuarioId);
                    veterinarioEmpleadoId = empleadoPorUsuario.Id;
                    veterinarioUsuarioId = empleadoPorUsuario.UsuarioId;

                    // Validar que el empleado tiene el rol de veterinario
                    var tieneRolVeterinario = empleadoPorUsuario.Usuario.UsuarioRoles.Any(ur => 
                        ur.Rol != null && (ur.Rol.Nombre == "Veterinario" || ur.Rol.Nombre == "Admin"));
                    if (!tieneRolVeterinario)
                    {
                        throw new ArgumentException($"El usuario {empleadoPorUsuario.UsuarioId} no tiene el rol de Veterinario");
                    }
                }
                else
                {
                    _logger.LogError("No se encontró empleado ni por EmpleadoId ni por UsuarioId: {VetId}", dto.VeterinarioId);
                    throw new ArgumentException($"No se encontró un empleado (veterinario) con el ID {dto.VeterinarioId}. " +
                        "Asegúrate de usar el EmpleadoId del endpoint /Empleados");
                }
            }

            _logger.LogInformation("IDs confirmados - UsuarioId: {UserId}, EmpleadoId: {EmpId}", 
                veterinarioUsuarioId, veterinarioEmpleadoId);

            // Validar mascota si se especifica
            if (dto.MascotaId.HasValue)
            {
                _logger.LogInformation("Validando mascota: {MascotaId}", dto.MascotaId.Value);
                var mascota = await _context.Mascotas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == dto.MascotaId.Value);
                if (mascota == null)
                {
                    _logger.LogError("Mascota no encontrada: {MascotaId}", dto.MascotaId.Value);
                    throw new ArgumentException($"La mascota con ID {dto.MascotaId.Value} no existe");
                }
                _logger.LogInformation("Mascota encontrada: {Nombre}", mascota.Nombre);
            }

            // Validar propietario si se especifica
            if (dto.PropietarioId.HasValue)
            {
                _logger.LogInformation("Validando propietario: {PropId}", dto.PropietarioId.Value);
                var propietario = await _usuarioRepository.GetByIdAsync(dto.PropietarioId.Value);
                if (propietario == null)
                {
                    _logger.LogError("Propietario no encontrado: {PropId}", dto.PropietarioId.Value);
                    throw new ArgumentException($"El propietario con ID {dto.PropietarioId.Value} no existe");
                }
                _logger.LogInformation("Propietario encontrado: {Email}", propietario.Email);
            }

            // Validar sala si se especifica
            if (dto.SalaId.HasValue)
            {
                _logger.LogInformation("Validando sala: {SalaId}", dto.SalaId.Value);
                var sala = await _salaRepository.GetByIdAsync(dto.SalaId.Value);
                if (sala == null)
                {
                    _logger.LogError("Sala no encontrada: {SalaId}", dto.SalaId.Value);
                    throw new ArgumentException($"La sala con ID {dto.SalaId.Value} no existe");
                }
                if (!sala.Activa)
                {
                    throw new ArgumentException($"La sala {sala.Nombre} no está activa");
                }
                _logger.LogInformation("Sala encontrada: {Nombre}, Activa={Activa}", sala.Nombre, sala.Activa);
            }

            // Si viene de una solicitud digital, validar y vincular
            SolicitudCitaDigital? solicitud = null;
            if (dto.SolicitudCitaDigitalId.HasValue)
            {
                solicitud = await _context.SolicitudesCitasDigitales
                    .AsNoTracking()
                    .Include(s => s.PagoAnticipo)
                    .FirstOrDefaultAsync(s => s.Id == dto.SolicitudCitaDigitalId.Value);

                if (solicitud == null)
                {
                    throw new ArgumentException("La solicitud de cita digital no existe");
                }

                // Validar que la solicitud esté en estado adecuado
                if (solicitud.Estado != EstadoSolicitudCita.PagadaPendienteConfirmacion && 
                    solicitud.Estado != EstadoSolicitudCita.EnRevision)
                {
                    throw new InvalidOperationException($"La solicitud debe estar pagada o en revisión para crear la cita. Estado actual: {solicitud.Estado}");
                }

                // Validar que el pago del 50% esté completado
                if (!solicitud.PagoAnticipoId.HasValue)
                {
                    throw new InvalidOperationException("La solicitud debe tener un pago de anticipo del 50% antes de crear la cita");
                }

                var pagoAnticipo = solicitud.PagoAnticipo;
                if (pagoAnticipo == null || pagoAnticipo.Estado != EstadoPago.Completado)
                {
                    throw new InvalidOperationException("El pago del anticipo debe estar completado para crear la cita");
                }

                if (pagoAnticipo.Monto < solicitud.MontoAnticipo)
                {
                    throw new InvalidOperationException($"El monto del anticipo debe ser al menos {solicitud.MontoAnticipo:C} (50% del costo total)");
                }

                // Usar datos de la solicitud si no se especifican en el DTO
                dto.MascotaId ??= solicitud.MascotaId;
                dto.PropietarioId ??= solicitud.SolicitanteId;
                dto.MotivoConsulta ??= solicitud.MotivoConsulta;
            }

            // Calcular EndAt
            var endAt = dto.StartAt.AddMinutes(dto.DuracionMin);

            _logger.LogInformation("Validando fecha: StartAt={Start}, EndAt={End}", dto.StartAt, endAt);

            // Validar que la fecha de inicio no sea en el pasado
            if (dto.StartAt < DateTime.UtcNow.AddMinutes(-5)) // Permitir 5 minutos de tolerancia
            {
                throw new ArgumentException("La fecha de inicio de la cita no puede ser en el pasado");
            }

            _logger.LogInformation("Validando solapamiento de veterinario usando EmpleadoId: {EmpId}", veterinarioEmpleadoId);

            // Validar solapamiento de veterinario - USAR EL USUARIO ID para la validación
            // porque VeterinarioId en Cita apunta a Usuario
            var hasOverlap = await _citaRepository.HasOverlappingAppointmentAsync(
                veterinarioUsuarioId,
                dto.StartAt,
                endAt);

            if (hasOverlap)
            {
                throw new InvalidOperationException("El veterinario ya tiene una cita en ese horario");
            }

            // Validar solapamiento de sala si se especificó
            if (dto.SalaId.HasValue)
            {
                _logger.LogInformation("Validando solapamiento de sala...");
                var hasSalaOverlap = await _citaRepository.HasSalaOverlappingAsync(
                    dto.SalaId.Value,
                    dto.StartAt,
                    endAt);

                if (hasSalaOverlap)
                {
                    throw new InvalidOperationException("La sala ya está ocupada en ese horario");
                }
            }

            _logger.LogInformation("Todas las validaciones pasadas, creando cita...");

            var cita = new Cita
            {
                MascotaId = dto.MascotaId,
                PropietarioId = dto.PropietarioId,
                VeterinarioId = veterinarioUsuarioId, // ?? USAR EL USUARIO ID (NO EL EMPLEADO ID)
                SalaId = dto.SalaId,
                Tipo = dto.Tipo,
                Status = DomainStatusCita.Programada,
                StartAt = dto.StartAt,
                EndAt = endAt,
                DuracionMin = dto.DuracionMin,
                Notas = dto.Notas,
                MotivoConsulta = dto.MotivoConsulta,
                PagoId = solicitud?.PagoAnticipoId,
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
                    Notas = solicitud != null ? $"Cita creada desde solicitud digital {solicitud.NumeroSolicitud}" : "Cita creada"
                }
            };

            await _citaRepository.AddAsync(cita);

            _logger.LogInformation("Cita creada exitosamente con ID: {CitaId}, VeterinarioUsuarioId: {VetUsrId}", 
                cita.Id, cita.VeterinarioId);

            // Si viene de una solicitud digital, actualizarla
            if (solicitud != null)
            {
                // Cargar la solicitud rastreada para actualizar
                var solicitudToUpdate = await _context.SolicitudesCitasDigitales
                    .FirstOrDefaultAsync(s => s.Id == solicitud.Id);
                
                if (solicitudToUpdate != null)
                {
                    solicitudToUpdate.Confirmar(userId, cita.Id);
                    await _context.SaveChangesAsync();
                }
            }

            var createdCita = await _citaRepository.GetByIdAsync(cita.Id);
            _logger.LogInformation("=== FIN VALIDACIÓN CITA (EXITOSO) ===");
            return MapToDetailDto(createdCita!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateAsync: {Message}", ex.Message);
            _logger.LogInformation("=== FIN VALIDACIÓN CITA (CON ERROR) ===");
            throw;
        }
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

        // Guardar estado anterior
        var estadoAnterior = cita.Status;
        Console.WriteLine($"[CANCELAR] Cita {id} - Estado anterior: {estadoAnterior}");

        // Actualizar estado y motivo directamente
        cita.Status = DomainStatusCita.Cancelada;
        cita.MotivoRechazo = dto.MotivoRechazo;
        cita.UpdatedBy = userId;
        cita.UpdatedAt = DateTime.UtcNow;

        Console.WriteLine($"[CANCELAR] Cita {id} - Nuevo estado: {cita.Status}");

        // Crear el registro de historial directamente en el contexto
        var historialEstado = new CitaHistorialEstado
        {
            CitaId = id,
            FromStatus = estadoAnterior,
            ToStatus = DomainStatusCita.Cancelada,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow,
            Notas = $"Cita cancelada: {dto.MotivoRechazo}"
        };

        _context.Set<CitaHistorialEstado>().Add(historialEstado);

        // Si la cita proviene de una solicitud digital, actualizar su estado
        var solicitud = await _context.SolicitudesCitasDigitales
            .FirstOrDefaultAsync(s => s.CitaId == id);

        if (solicitud != null && solicitud.Estado == EstadoSolicitudCita.Confirmada)
        {
            solicitud.Estado = EstadoSolicitudCita.Cancelada;
            solicitud.MotivoRechazo = $"Cita cancelada: {dto.MotivoRechazo}";
            solicitud.UpdatedBy = userId;
            solicitud.UpdatedAt = DateTime.UtcNow;
            Console.WriteLine($"[CANCELAR] Solicitud {solicitud.Id} actualizada a Cancelada");
        }

        // Guardar todos los cambios en una sola transacción
        await _citaRepository.UpdateAsync(cita);
        Console.WriteLine($"[CANCELAR] Cambios guardados en BD");

        // Refrescar la cita desde la base de datos con AsNoTracking para evitar caché
        var updatedCita = await _context.Citas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Propietario)
            .Include(c => c.Veterinario)
            .Include(c => c.Sala)
            .Include(c => c.Recordatorios)
            .Include(c => c.HistorialEstados)
            .FirstOrDefaultAsync(c => c.Id == id);

        Console.WriteLine($"[CANCELAR] Estado después de refrescar: {updatedCita?.Status}");

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

        // Guardar estado anterior
        var estadoAnterior = cita.Status;

        // Actualizar notas si se proporcionan
        if (dto.Notas != null)
            cita.Notas = dto.Notas;

        // Actualizar estado
        cita.Status = DomainStatusCita.Completada;
        cita.UpdatedBy = userId;
        cita.UpdatedAt = DateTime.UtcNow;

        // Crear el registro de historial directamente en el contexto
        var historialEstado = new CitaHistorialEstado
        {
            CitaId = id,
            FromStatus = estadoAnterior,
            ToStatus = DomainStatusCita.Completada,
            ChangedBy = userId,
            ChangedAt = DateTime.UtcNow,
            Notas = "Cita completada"
        };

        _context.Set<CitaHistorialEstado>().Add(historialEstado);

        // Actualizar la cita
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
