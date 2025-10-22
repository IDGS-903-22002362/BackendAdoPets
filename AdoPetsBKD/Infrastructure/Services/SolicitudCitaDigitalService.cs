using Microsoft.EntityFrameworkCore;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Infrastructure.Data;

namespace AdoPetsBKD.Infrastructure.Services;

public class SolicitudCitaDigitalService : ISolicitudCitaDigitalService
{
    private readonly AdoPetsDbContext _context;

    public SolicitudCitaDigitalService(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<SolicitudCitaDigitalDto> CreateSolicitudAsync(CreateSolicitudCitaDigitalDto dto, Guid createdBy)
    {
        // Verificar disponibilidad
        var disponibilidad = await VerificarDisponibilidadAsync(new VerificarDisponibilidadDto
        {
            FechaHoraInicio = dto.FechaHoraSolicitada,
            DuracionMin = dto.DuracionEstimadaMin,
            VeterinarioId = dto.VeterinarioPreferidoId,
            SalaId = dto.SalaPreferidaId
        });

        var solicitud = new SolicitudCitaDigital
        {
            Id = Guid.NewGuid(),
            SolicitanteId = dto.SolicitanteId,
            MascotaId = dto.MascotaId,
            NombreMascota = dto.NombreMascota,
            EspecieMascota = dto.EspecieMascota,
            RazaMascota = dto.RazaMascota,
            ServicioId = dto.ServicioId,
            DescripcionServicio = dto.DescripcionServicio,
            MotivoConsulta = dto.MotivoConsulta,
            FechaHoraSolicitada = dto.FechaHoraSolicitada,
            DuracionEstimadaMin = dto.DuracionEstimadaMin,
            VeterinarioPreferidoId = dto.VeterinarioPreferidoId,
            SalaPreferidaId = dto.SalaPreferidaId,
            CostoEstimado = dto.CostoEstimado,
            FechaSolicitud = DateTime.UtcNow,
            DisponibilidadVerificada = disponibilidad.Disponible,
            CreatedBy = createdBy
        };

        solicitud.NumeroSolicitud = solicitud.GenerarNumeroSolicitud();
        solicitud.CalcularMontoAnticipo();

        // Si hay conflictos, marcar como pendiente de revisión
        if (!disponibilidad.Disponible)
        {
            solicitud.Estado = EstadoSolicitudCita.Pendiente;
        }
        else
        {
            solicitud.Estado = EstadoSolicitudCita.PendientePago;
        }

        _context.SolicitudesCitasDigitales.Add(solicitud);
        await _context.SaveChangesAsync();

        return await GetSolicitudByIdAsync(solicitud.Id) ?? throw new Exception("Error al crear solicitud");
    }

    public async Task<SolicitudCitaDigitalDto?> GetSolicitudByIdAsync(Guid id)
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Mascota)
            .Include(s => s.Servicio)
            .Include(s => s.VeterinarioPreferido)
            .Where(s => s.Id == id)
            .Select(s => new SolicitudCitaDigitalDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                SolicitanteId = s.SolicitanteId,
                NombreSolicitante = s.Solicitante.NombreCompleto,
                EmailSolicitante = s.Solicitante.Email,
                MascotaId = s.MascotaId,
                NombreMascota = s.NombreMascota,
                EspecieMascota = s.EspecieMascota,
                RazaMascota = s.RazaMascota,
                ServicioId = s.ServicioId,
                DescripcionServicio = s.DescripcionServicio,
                MotivoConsulta = s.MotivoConsulta,
                FechaHoraSolicitada = s.FechaHoraSolicitada,
                DuracionEstimadaMin = s.DuracionEstimadaMin,
                VeterinarioPreferidoId = s.VeterinarioPreferidoId,
                NombreVeterinarioPreferido = s.VeterinarioPreferido != null ? s.VeterinarioPreferido.NombreCompleto : null,
                CostoEstimado = s.CostoEstimado,
                MontoAnticipo = s.MontoAnticipo,
                Estado = (int)s.Estado,
                EstadoNombre = s.Estado.ToString(),
                FechaSolicitud = s.FechaSolicitud,
                FechaRevision = s.FechaRevision,
                FechaConfirmacion = s.FechaConfirmacion,
                MotivoRechazo = s.MotivoRechazo,
                PagoAnticipoId = s.PagoAnticipoId,
                CitaId = s.CitaId,
                DisponibilidadVerificada = s.DisponibilidadVerificada
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<SolicitudCitaDigitalDto>> GetSolicitudesByUsuarioAsync(Guid usuarioId)
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Servicio)
            .Where(s => s.SolicitanteId == usuarioId)
            .OrderByDescending(s => s.FechaSolicitud)
            .Select(s => new SolicitudCitaDigitalDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                SolicitanteId = s.SolicitanteId,
                NombreSolicitante = s.Solicitante.NombreCompleto,
                EmailSolicitante = s.Solicitante.Email,
                NombreMascota = s.NombreMascota,
                DescripcionServicio = s.DescripcionServicio,
                FechaHoraSolicitada = s.FechaHoraSolicitada,
                CostoEstimado = s.CostoEstimado,
                MontoAnticipo = s.MontoAnticipo,
                Estado = (int)s.Estado,
                EstadoNombre = s.Estado.ToString(),
                FechaSolicitud = s.FechaSolicitud,
                DisponibilidadVerificada = s.DisponibilidadVerificada
            })
            .ToListAsync();
    }

    public async Task<List<SolicitudCitaDigitalDto>> GetSolicitudesPendientesAsync()
    {
        return await _context.SolicitudesCitasDigitales
            .Include(s => s.Solicitante)
            .Include(s => s.Servicio)
            .Where(s => s.Estado == EstadoSolicitudCita.Pendiente || 
                       s.Estado == EstadoSolicitudCita.EnRevision ||
                       s.Estado == EstadoSolicitudCita.PagadaPendienteConfirmacion)
            .OrderBy(s => s.FechaSolicitud)
            .Select(s => new SolicitudCitaDigitalDto
            {
                Id = s.Id,
                NumeroSolicitud = s.NumeroSolicitud,
                SolicitanteId = s.SolicitanteId,
                NombreSolicitante = s.Solicitante.NombreCompleto,
                EmailSolicitante = s.Solicitante.Email,
                NombreMascota = s.NombreMascota,
                DescripcionServicio = s.DescripcionServicio,
                FechaHoraSolicitada = s.FechaHoraSolicitada,
                CostoEstimado = s.CostoEstimado,
                MontoAnticipo = s.MontoAnticipo,
                Estado = (int)s.Estado,
                EstadoNombre = s.Estado.ToString(),
                FechaSolicitud = s.FechaSolicitud,
                PagoAnticipoId = s.PagoAnticipoId,
                DisponibilidadVerificada = s.DisponibilidadVerificada
            })
            .ToListAsync();
    }

    public async Task<DisponibilidadResponseDto> VerificarDisponibilidadAsync(VerificarDisponibilidadDto dto)
    {
        var resultado = new DisponibilidadResponseDto { Disponible = true };
        var fechaFin = dto.FechaHoraInicio.AddMinutes(dto.DuracionMin);

        // Verificar conflictos con otras citas
        var citasConflicto = await _context.Citas
            .Where(c => c.Status != StatusCita.Cancelada &&
                       ((dto.VeterinarioId == null || c.VeterinarioId == dto.VeterinarioId) ||
                        (dto.SalaId == null || c.SalaId == dto.SalaId)) &&
                       c.StartAt < fechaFin && c.EndAt > dto.FechaHoraInicio)
            .ToListAsync();

        if (citasConflicto.Any())
        {
            resultado.Disponible = false;
            resultado.Mensaje = "Existen conflictos de horario";
            resultado.Conflictos = citasConflicto.Select(c => new ConflictoDto
            {
                Tipo = c.VeterinarioId == dto.VeterinarioId ? "Veterinario" : "Sala",
                HoraInicio = c.StartAt,
                HoraFin = c.EndAt,
                Descripcion = $"Cita programada de {c.StartAt:HH:mm} a {c.EndAt:HH:mm}"
            }).ToList();
        }

        // Verificar horarios del veterinario si se especificó
        if (dto.VeterinarioId.HasValue)
        {
            var diaSemana = dto.FechaHoraInicio.DayOfWeek;
            var hora = dto.FechaHoraInicio.TimeOfDay;

            var empleado = await _context.Empleados
                .Include(e => e.Horarios)
                .FirstOrDefaultAsync(e => e.UsuarioId == dto.VeterinarioId.Value);

            if (empleado != null)
            {
                var horarioDelDia = empleado.Horarios
                    .FirstOrDefault(h => h.DiaSemana == diaSemana && h.Tipo == TipoHorario.Turno);

                if (horarioDelDia == null || !horarioDelDia.HoraEntrada.HasValue || !horarioDelDia.HoraSalida.HasValue ||
                    hora < horarioDelDia.HoraEntrada.Value || hora > horarioDelDia.HoraSalida.Value)
                {
                    resultado.Disponible = false;
                    resultado.Mensaje = "El veterinario no tiene horario disponible en ese día/hora";
                }
            }
        }

        return resultado;
    }

    public async Task<SolicitudCitaDigitalDto> MarcarComoEnRevisionAsync(Guid solicitudId, Guid revisadoPorId)
    {
        var solicitud = await _context.SolicitudesCitasDigitales.FindAsync(solicitudId)
            ?? throw new Exception("Solicitud no encontrada");

        solicitud.MarcarComoEnRevision(revisadoPorId);
        await _context.SaveChangesAsync();

        return await GetSolicitudByIdAsync(solicitudId) ?? throw new Exception("Error al actualizar solicitud");
    }

    public async Task<SolicitudCitaDigitalDto> ConfirmarSolicitudAsync(ConfirmarSolicitudCitaDto dto)
    {
        var solicitud = await _context.SolicitudesCitasDigitales.FindAsync(dto.SolicitudId)
            ?? throw new Exception("Solicitud no encontrada");

        // Crear la cita
        var cita = new Cita
        {
            Id = Guid.NewGuid(),
            MascotaId = solicitud.MascotaId,
            PropietarioId = solicitud.SolicitanteId,
            VeterinarioId = dto.VeterinarioId,
            SalaId = dto.SalaId,
            Tipo = TipoCita.Procedimiento,
            Status = StatusCita.Programada,
            StartAt = dto.FechaHoraConfirmada,
            EndAt = dto.FechaHoraConfirmada.AddMinutes(dto.DuracionMin),
            DuracionMin = dto.DuracionMin,
            MotivoConsulta = solicitud.MotivoConsulta,
            CreatedBy = dto.ConfirmadoPorId
        };

        _context.Citas.Add(cita);
        solicitud.Confirmar(dto.ConfirmadoPorId, cita.Id);
        
        await _context.SaveChangesAsync();

        return await GetSolicitudByIdAsync(dto.SolicitudId) ?? throw new Exception("Error al confirmar solicitud");
    }

    public async Task<SolicitudCitaDigitalDto> RechazarSolicitudAsync(RechazarSolicitudCitaDto dto)
    {
        var solicitud = await _context.SolicitudesCitasDigitales.FindAsync(dto.SolicitudId)
            ?? throw new Exception("Solicitud no encontrada");

        solicitud.Rechazar(dto.RechazadoPorId, dto.Motivo);
        await _context.SaveChangesAsync();

        return await GetSolicitudByIdAsync(dto.SolicitudId) ?? throw new Exception("Error al rechazar solicitud");
    }

    public async Task<SolicitudCitaDigitalDto> CancelarSolicitudAsync(Guid solicitudId, Guid usuarioId)
    {
        var solicitud = await _context.SolicitudesCitasDigitales.FindAsync(solicitudId)
            ?? throw new Exception("Solicitud no encontrada");

        solicitud.Estado = EstadoSolicitudCita.Cancelada;
        solicitud.UpdatedBy = usuarioId;
        solicitud.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetSolicitudByIdAsync(solicitudId) ?? throw new Exception("Error al cancelar solicitud");
    }
}
