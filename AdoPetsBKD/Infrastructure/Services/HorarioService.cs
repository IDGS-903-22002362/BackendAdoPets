using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Horarios;
using AdoPetsBKD.Application.Interfaces.Repositories;

namespace AdoPetsBKD.Infrastructure.Services
{
    /// <summary>
    /// Implementación de la interfaz de servicio de horarios
    /// </summary>
    public class HorarioService : IHorarioService
    {
        private readonly IHorarioRepositoy _horarioRepositoy;
        private readonly IEmpleadoRepository _empleadoRepository;

        public HorarioService(IHorarioRepositoy horarioRepositoy, IEmpleadoRepository empleadoRepository) 
        { 
            _horarioRepositoy = horarioRepositoy;
            _empleadoRepository = empleadoRepository;
        }

        public async Task<PagedResponse<ListHorarioDto>> GetAllAsync(int pageNumber, int pageSize, DateTime? fechaInicio = null, DateTime? fechaFin = null, TipoHorario? tipo = null)
        {
            var horarios = await _horarioRepositoy.GetAllAsync(pageNumber, pageSize, fechaInicio, fechaFin, tipo);
            var horariosDto = horarios.Select(h => new ListHorarioDto
            {
                Id = h.Id,
                EmpleadoId = h.EmpleadoId,
                NombreCompletoEmpleado = h.Empleado?.Usuario?.NombreCompleto ?? string.Empty, 
                CedulaEmpleado = h.Empleado?.Cedula,
                TipoEmpleado = h.Empleado?.Usuario?.UsuarioRoles?.FirstOrDefault()?.Rol?.Nombre ?? string.Empty,
                Fecha = h.Fecha,
                RangoInicio = h.RangoInicio,
                RangoFin = h.RangoFin,
                HoraEntrada = h.HoraEntrada,
                HoraSalida = h.HoraSalida,
                Tipo = (int)h.Tipo,
                DiaSemana = h.DiaSemana
            }).ToList();

            var totalCount = await _horarioRepositoy.GetTotalCountAsync(fechaInicio, fechaFin, tipo);

            return new PagedResponse<ListHorarioDto>
            {
                Items = horariosDto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<DetailHorarioDto?> GetByIdAsync(Guid id)
        {
            var horario = await _horarioRepositoy.GetByIdAsync(id);
            
            if (horario == null)
                return null;

            return new DetailHorarioDto
            {
                Id = horario.Id,
                EmpleadoId = horario.EmpleadoId,
                NombreCompletoEmpleado = horario.Empleado?.Usuario?.NombreCompleto ?? string.Empty,
                CedulaEmpleado = horario.Empleado?.Cedula,
                TipoEmpleado = horario.Empleado?.Usuario?.UsuarioRoles?.FirstOrDefault()?.Rol?.Nombre ?? string.Empty,
                EmailLaboralEmpleado = horario.Empleado?.EmailLaboral,
                Fecha = horario.Fecha,
                RangoInicio = horario.RangoInicio,
                RangoFin = horario.RangoFin,
                HoraEntrada = horario.HoraEntrada,
                HoraSalida = horario.HoraSalida,
                Tipo = (int)horario.Tipo,
                DiaSemana = horario.DiaSemana,
                Notas = horario.Notas
            };
        }

        public async Task<DetailHorarioDto> CreateAsync(CreateHorarioDto dto)
        {
            // Validar que el empleado existe
            var empleadoExists = await _empleadoRepository.ExistsAsync(dto.EmpleadoId);
            if (!empleadoExists)
            {
                throw new ArgumentException($"No se encontró un empleado con el ID: {dto.EmpleadoId}");
            }

            // Validar que se proporcione al menos una fecha (específica o rango)
            if (!dto.Fecha.HasValue && (!dto.RangoInicio.HasValue || !dto.RangoFin.HasValue))
            {
                throw new ArgumentException("Debe proporcionar una fecha específica o un rango de fechas (inicio y fin)");
            }

            // Validar que el rango de fechas sea válido
            if (dto.RangoInicio.HasValue && dto.RangoFin.HasValue && dto.RangoInicio.Value > dto.RangoFin.Value)
            {
                throw new ArgumentException("La fecha de inicio del rango no puede ser posterior a la fecha de fin");
            }

            if (dto.RangoInicio.HasValue && dto.RangoFin.HasValue && !dto.DiaSemana.HasValue)
            {
                throw new ArgumentException("Los horarios recurrentes deben tener un día de la semana especificado");
            }

            var tipoHorario = (TipoHorario)dto.Tipo;

            // Validar conflictos con horarios existentes
            var conflictos = await _horarioRepositoy.GetConflictosAsync(
                dto.EmpleadoId, 
                dto.Fecha, 
                dto.RangoInicio, 
                dto.RangoFin,
                dto.DiaSemana
            );

            if (conflictos.Any())
            {
                var prioridadNuevo = ObtenerPrioridad(tipoHorario);
                var conflictosNoAnulables = conflictos
                    .Where(c => c.ObtenerPrioridad() >= prioridadNuevo)
                    .ToList();

                if (conflictosNoAnulables.Any())
                {
                    var mensajesConflicto = conflictosNoAnulables.Select(c => 
                        $"- {c.Tipo} " + 
                        (c.Fecha.HasValue 
                            ? $"el {c.Fecha.Value:dd/MM/yyyy}" 
                            : $"recurrente los {c.DiaSemana} desde {c.RangoInicio:dd/MM/yyyy} hasta {c.RangoFin:dd/MM/yyyy}")
                    );

                    throw new InvalidOperationException(
                        $"No se puede crear el horario porque existe un conflicto con horarios de igual o mayor prioridad:\n" +
                        string.Join("\n", mensajesConflicto) + "\n\n" +
                        "Nota: Las Vacaciones, Permisos y Guardias (prioridad alta) anulan todo. " +
                        "Los Descansos (prioridad media) anulan los Turnos. " +
                        "Los Turnos (prioridad baja) son anulables."
                    );
                }
                
                // Si hay conflictos pero son de menor prioridad, registrar advertencia
                if (conflictos.Any())
                {
                    var tiposAnulados = string.Join(", ", conflictos.Select(c => c.Tipo.ToString()).Distinct());
                }
            }

            var horario = new Horario
            {
                EmpleadoId = dto.EmpleadoId,
                Fecha = dto.Fecha,
                RangoInicio = dto.RangoInicio,
                RangoFin = dto.RangoFin,
                HoraEntrada = dto.HoraEntrada,
                HoraSalida = dto.HoraSalida,
                Tipo = tipoHorario,
                DiaSemana = dto.DiaSemana,
                Notas = dto.Notas
            };

            var createdHorario = await _horarioRepositoy.CreateAsync(horario);
            
            // Obtener el horario con sus relaciones cargadas
            var horarioWithRelations = await _horarioRepositoy.GetByIdAsync(createdHorario.Id);

            return new DetailHorarioDto
            {
                Id = horarioWithRelations!.Id,
                EmpleadoId = horarioWithRelations.EmpleadoId,
                NombreCompletoEmpleado = horarioWithRelations.Empleado?.Usuario?.NombreCompleto ?? string.Empty,
                CedulaEmpleado = horarioWithRelations.Empleado?.Cedula,
                TipoEmpleado = horarioWithRelations.Empleado?.Usuario?.UsuarioRoles?.FirstOrDefault()?.Rol?.Nombre ?? string.Empty,
                EmailLaboralEmpleado = horarioWithRelations.Empleado?.EmailLaboral,
                Fecha = horarioWithRelations.Fecha,
                RangoInicio = horarioWithRelations.RangoInicio,
                RangoFin = horarioWithRelations.RangoFin,
                HoraEntrada = horarioWithRelations.HoraEntrada,
                HoraSalida = horarioWithRelations.HoraSalida,
                Tipo = (int)horarioWithRelations.Tipo,
                DiaSemana = horarioWithRelations.DiaSemana,
                Notas = horarioWithRelations.Notas
            };
        }

        public async Task<DetailHorarioDto> UpdateAsync(Guid id, UpdateHorarioDto dto)
        {
            var horario = await _horarioRepositoy.GetByIdAsync(id);
            
            if (horario == null)
            {
                throw new KeyNotFoundException($"No se encontró un horario con el ID: {id}");
            }

            // Validar que se proporcione al menos una fecha (específica o rango)
            if (!dto.Fecha.HasValue && (!dto.RangoInicio.HasValue || !dto.RangoFin.HasValue))
            {
                throw new ArgumentException("Debe proporcionar una fecha específica o un rango de fechas (inicio y fin)");
            }

            // Validar que el rango de fechas sea válido
            if (dto.RangoInicio.HasValue && dto.RangoFin.HasValue && dto.RangoInicio.Value > dto.RangoFin.Value)
            {
                throw new ArgumentException("La fecha de inicio del rango no puede ser posterior a la fecha de fin");
            }

            // Validar que los horarios recurrentes tengan día de la semana
            if (dto.RangoInicio.HasValue && dto.RangoFin.HasValue && !dto.DiaSemana.HasValue)
            {
                throw new ArgumentException("Los horarios recurrentes deben tener un día de la semana especificado");
            }

            var tipoHorario = (TipoHorario)dto.Tipo;

            var conflictos = await _horarioRepositoy.GetConflictosAsync(
                horario.EmpleadoId,
                dto.Fecha,
                dto.RangoInicio,
                dto.RangoFin,
                dto.DiaSemana,
                id 
            );

            if (conflictos.Any())
            {
                var prioridadNuevo = ObtenerPrioridad(tipoHorario);
                var conflictosNoAnulables = conflictos
                    .Where(c => c.ObtenerPrioridad() >= prioridadNuevo)
                    .ToList();

                if (conflictosNoAnulables.Any())
                {
                    var mensajesConflicto = conflictosNoAnulables.Select(c =>
                        $"- {c.Tipo} " +
                        (c.Fecha.HasValue
                            ? $"el {c.Fecha.Value:dd/MM/yyyy}"
                            : $"recurrente los {c.DiaSemana} desde {c.RangoInicio:dd/MM/yyyy} hasta {c.RangoFin:dd/MM/yyyy}")
                    );

                    throw new InvalidOperationException(
                        $"No se puede actualizar el horario porque existe un conflicto con horarios de igual o mayor prioridad:\n" +
                        string.Join("\n", mensajesConflicto)
                    );
                }
            }

            horario.Fecha = dto.Fecha;
            horario.RangoInicio = dto.RangoInicio;
            horario.RangoFin = dto.RangoFin;
            horario.HoraEntrada = dto.HoraEntrada;
            horario.HoraSalida = dto.HoraSalida;
            horario.Tipo = tipoHorario;
            horario.DiaSemana = dto.DiaSemana;
            horario.Notas = dto.Notas;

            await _horarioRepositoy.UpdateAsync(horario);

            var updatedHorario = await _horarioRepositoy.GetByIdAsync(id);

            return new DetailHorarioDto
            {
                Id = updatedHorario!.Id,
                EmpleadoId = updatedHorario.EmpleadoId,
                NombreCompletoEmpleado = updatedHorario.Empleado?.Usuario?.NombreCompleto ?? string.Empty,
                CedulaEmpleado = updatedHorario.Empleado?.Cedula,
                TipoEmpleado = updatedHorario.Empleado?.Usuario?.UsuarioRoles?.FirstOrDefault()?.Rol?.Nombre ?? string.Empty,
                EmailLaboralEmpleado = updatedHorario.Empleado?.EmailLaboral,
                Fecha = updatedHorario.Fecha,
                RangoInicio = updatedHorario.RangoInicio,
                RangoFin = updatedHorario.RangoFin,
                HoraEntrada = updatedHorario.HoraEntrada,
                HoraSalida = updatedHorario.HoraSalida,
                Tipo = (int)updatedHorario.Tipo,
                DiaSemana = updatedHorario.DiaSemana,
                Notas = updatedHorario.Notas
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var exists = await _horarioRepositoy.ExistsAsync(id);
            
            if (!exists)
            {
                throw new KeyNotFoundException($"No se encontró un horario con el ID: {id}");
            }

            await _horarioRepositoy.DeleteAsync(id);
        }

        public async Task<DetailHorarioDto?> GetHorarioEfectivoAsync(Guid empleadoId, DateTime fecha)
        {
            var horario = await _horarioRepositoy.GetHorarioEfectivoAsync(empleadoId, fecha);
            
            if (horario == null)
                return null;

            return new DetailHorarioDto
            {
                Id = horario.Id,
                EmpleadoId = horario.EmpleadoId,
                NombreCompletoEmpleado = horario.Empleado?.Usuario?.NombreCompleto ?? string.Empty,
                CedulaEmpleado = horario.Empleado?.Cedula,
                TipoEmpleado = horario.Empleado?.Usuario?.UsuarioRoles?.FirstOrDefault()?.Rol?.Nombre ?? string.Empty,
                EmailLaboralEmpleado = horario.Empleado?.EmailLaboral,
                Fecha = horario.Fecha,
                RangoInicio = horario.RangoInicio,
                RangoFin = horario.RangoFin,
                HoraEntrada = horario.HoraEntrada,
                HoraSalida = horario.HoraSalida,
                Tipo = (int)horario.Tipo,
                DiaSemana = horario.DiaSemana,
                Notas = horario.Notas
            };
        }

        public async Task<List<ListHorarioDto>> GetHorariosAplicablesAsync(Guid empleadoId, DateTime fecha)
        {
            var horarios = await _horarioRepositoy.GetHorariosAplicablesAsync(empleadoId, fecha);
            
            return horarios
                .OrderByDescending(h => h.ObtenerPrioridad())
                .Select(h => new ListHorarioDto
                {
                    Id = h.Id,
                    EmpleadoId = h.EmpleadoId,
                    NombreCompletoEmpleado = h.Empleado?.Usuario?.NombreCompleto ?? string.Empty,
                    CedulaEmpleado = h.Empleado?.Cedula,
                    TipoEmpleado = h.Empleado?.Usuario?.UsuarioRoles?.FirstOrDefault()?.Rol?.Nombre ?? string.Empty,
                    Fecha = h.Fecha,
                    RangoInicio = h.RangoInicio,
                    RangoFin = h.RangoFin,
                    HoraEntrada = h.HoraEntrada,
                    HoraSalida = h.HoraSalida,
                    Tipo = (int)h.Tipo,
                    DiaSemana = h.DiaSemana
                }).ToList();
        }

        public async Task<List<CalendarioHorarioDto>> GetCalendarioAsync(Guid empleadoId, DateTime fechaInicio, DateTime fechaFin)
        {
            // Validar que el empleado existe
            var empleadoExists = await _empleadoRepository.ExistsAsync(empleadoId);
            if (!empleadoExists)
            {
                throw new ArgumentException($"No se encontró un empleado con el ID: {empleadoId}");
            }

            // Validar que el rango sea válido
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin");
            }

            var calendario = new List<CalendarioHorarioDto>();

            for (var fecha = fechaInicio.Date; fecha <= fechaFin.Date; fecha = fecha.AddDays(1))
            {
                // Obtener todos los horarios aplicables para esta fecha
                var horariosAplicables = await _horarioRepositoy.GetHorariosAplicablesAsync(empleadoId, fecha);

                if (!horariosAplicables.Any())
                {
                    calendario.Add(new CalendarioHorarioDto
                    {
                        Fecha = fecha,
                        DiaSemana = fecha.DayOfWeek,
                        EmpleadoId = empleadoId,
                        TieneHorario = false,
                        EsExcepcion = false,
                        Prioridad = 0
                    });
                    continue;
                }

                // Ordenar por prioridad y tomar el de mayor prioridad
                var horarioEfectivo = horariosAplicables
                    .OrderByDescending(h => h.ObtenerPrioridad())
                    .ThenByDescending(h => h.Fecha.HasValue)
                    .First();

                var horarioAnulado = horariosAplicables
                    .Where(h => h.Id != horarioEfectivo.Id && h.EsRecurrente())
                    .FirstOrDefault();

                var tipoNombre = horarioEfectivo.Tipo switch
                {
                    TipoHorario.Turno => "Turno",
                    TipoHorario.Descanso => "Descanso",
                    TipoHorario.Vacaciones => "Vacaciones",
                    TipoHorario.Permiso => "Permiso",
                    TipoHorario.Guardia => "Guardia",
                    _ => "Desconocido"
                };

                calendario.Add(new CalendarioHorarioDto
                {
                    Fecha = fecha,
                    DiaSemana = fecha.DayOfWeek,
                    HorarioId = horarioEfectivo.Id,
                    EmpleadoId = empleadoId,
                    NombreCompletoEmpleado = horarioEfectivo.Empleado?.Usuario?.NombreCompleto ?? string.Empty,
                    HoraEntrada = horarioEfectivo.HoraEntrada,
                    HoraSalida = horarioEfectivo.HoraSalida,
                    Tipo = (int)horarioEfectivo.Tipo,
                    TipoNombre = tipoNombre,
                    Notas = horarioEfectivo.Notas,
                    TieneHorario = true,
                    EsExcepcion = horarioEfectivo.EsExcepcion(),
                    HorarioAnuladoId = horarioAnulado?.Id,
                    Prioridad = horarioEfectivo.ObtenerPrioridad()
                });
            }

            return calendario;
        }

        public async Task<List<CalendarioGeneralDto>> GetCalendarioGeneralAsync(DateTime fechaInicio, DateTime fechaFin, bool incluirInactivos = false)
        {
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin");
            }

            // Obtener todos los empleados activos (o todos si se especifica)
            var empleados = await _empleadoRepository.GetAllAsync(1, 1000, !incluirInactivos);

            var calendarioGeneral = new List<CalendarioGeneralDto>();

            foreach (var empleado in empleados)
            {
                var calendarioEmpleado = await GetCalendarioAsync(empleado.Id, fechaInicio, fechaFin);

                // Calcular estadísticas
                var estadisticas = new EstadisticasEmpleadoDto
                {
                    DiasConHorario = calendarioEmpleado.Count(d => d.TieneHorario),
                    DiasSinHorario = calendarioEmpleado.Count(d => !d.TieneHorario),
                    DiasVacaciones = calendarioEmpleado.Count(d => d.Tipo == 3),
                    DiasPermiso = calendarioEmpleado.Count(d => d.Tipo == 4),
                    DiasGuardia = calendarioEmpleado.Count(d => d.Tipo == 5),
                    DiasTurno = calendarioEmpleado.Count(d => d.Tipo == 1),
                    DiasDescanso = calendarioEmpleado.Count(d => d.Tipo == 2),
                    TotalExcepciones = calendarioEmpleado.Count(d => d.EsExcepcion)
                };

                calendarioGeneral.Add(new CalendarioGeneralDto
                {
                    EmpleadoId = empleado.Id,
                    NombreCompleto = empleado.Usuario?.NombreCompleto ?? string.Empty,
                    Cedula = empleado.Cedula,
                    TipoEmpleado = empleado.Usuario != null 
                        ? string.Join(", ", empleado.Usuario.UsuarioRoles.Select(ur => ur.Rol?.Nombre ?? string.Empty))
                        : string.Empty,
                    EmailLaboral = empleado.EmailLaboral,
                    Dias = calendarioEmpleado,
                    Estadisticas = estadisticas
                });
            }

            return calendarioGeneral.OrderBy(c => c.NombreCompleto).ToList();
        }

        private int ObtenerPrioridad(TipoHorario tipo)
        {
            return tipo switch
            {
                TipoHorario.Vacaciones => 3,
                TipoHorario.Permiso => 3,
                TipoHorario.Guardia => 3,
                TipoHorario.Descanso => 2,
                TipoHorario.Turno => 1,
                _ => 0
            };
        }
    }
}
