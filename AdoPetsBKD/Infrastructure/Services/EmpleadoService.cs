using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Empleados;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IUsuarioService _usuarioService;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEspecialidadRepositoy _especialidadRepository;

        public EmpleadoService(
            IEmpleadoRepository empleadoRepository, 
            IUsuarioService usuarioService,
            IUsuarioRepository usuarioRepository,
            IEspecialidadRepositoy especialidadRepository)
        {
            _empleadoRepository = empleadoRepository;
            _usuarioService = usuarioService;
            _usuarioRepository = usuarioRepository;
            _especialidadRepository = especialidadRepository;
        }

        public async Task<PagedResponse<EmpleadoListDto>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false)
        {
            var empleados = await _empleadoRepository.GetAllAsync(pageNumber, pageSize, includeInactive);

            var totalCount = await _empleadoRepository.GetTotalCountAsync(includeInactive);

            var empleadosDto = empleados.Select(e => new EmpleadoListDto
            {
                Id = e.Id,
                UsuarioId = e.UsuarioId,
                NombreCompleto = e.Usuario != null ? e.Usuario.Nombre + " " + e.Usuario.ApellidoPaterno + " " + e.Usuario.ApellidoMaterno : string.Empty,
                EmailLaboral = e.EmailLaboral,
                TelefonoLaboral = e.TelefonoLaboral,
                FechaContratacion = e.FechaContratacion,
                TipoEmpleado = e.Usuario != null ? string.Join(", ", e.Usuario.UsuarioRoles.Select(ur => ur.Rol != null ? ur.Rol.Nombre : string.Empty)) : string.Empty,
                Sueldo = e.Sueldo,
                Especialidades = e.Especialidades.Select(es => new EspecialidadSimpleDto
                {
                    Id = es.EspecialidadId,
                    Codigo = es.Especialidad.Codigo,
                    Descripcion = es.Especialidad.Descripcion
                }).ToList()
            }).ToList();

            return new PagedResponse<EmpleadoListDto>
            {
                Items = empleadosDto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }; 
        }

        public async Task<EmpleadoDetailDto?> GetByIdAsync(Guid id)
        {
            var empleado = await _empleadoRepository.GetByIdWithEspecialidadesAsync(id);

            if (empleado == null || empleado.Usuario == null) 
            {
                return null;
            }

            return new EmpleadoDetailDto
            {
                Id = empleado.Id,
                UsuarioId = empleado.UsuarioId,
                Nombre = empleado.Usuario.Nombre,
                ApellidoPaterno = empleado.Usuario.ApellidoPaterno,
                ApellidoMaterno = empleado.Usuario.ApellidoMaterno,
                NombreCompleto = empleado.Usuario.Nombre + " " + empleado.Usuario.ApellidoPaterno + " " + empleado.Usuario.ApellidoMaterno,
                EmailLaboral = empleado.EmailLaboral,
                TelefonoLaboral = empleado.TelefonoLaboral,
                FechaContratacion = empleado.FechaContratacion,
                TipoEmpleado = string.Join(", ", empleado.Usuario.UsuarioRoles.Select(ur => ur.Rol != null ? ur.Rol.Nombre : string.Empty)),
                Sueldo = empleado.Sueldo,
                Disponibilidad = empleado.Disponibilidad,
                Cedula = empleado.Cedula,
                Especialidades = empleado.Especialidades.Select(ee => new EspecialidadEmpleadoDto
                {
                    EspecialidadId = ee.EspecialidadId,
                    Descripcion = ee.Especialidad.Descripcion,
                    Codigo = ee.Especialidad.Codigo,
                    Certificacion = ee.Certificacion,
                    ObtainedAt = ee.ObtainedAt
                }).ToList()
            }; 
        }

        public async Task<EmpleadoDetailDto> CreateAsync(CreateEmpleadoDto dto, Guid createdBy)
        {
            // Crear el usuario usando UsuarioService 
            var usuarioEmpleado = await _usuarioService.CreateAsync(dto.Usuario, createdBy);

            // Crear al empleado vinculado
            var empleado = new Empleado
            {
                UsuarioId = usuarioEmpleado.Id,
                Cedula = dto.Cedula,
                Disponibilidad = dto.Disponibilidad,
                EmailLaboral = dto.EmailLaboral,
                TelefonoLaboral = dto.TelefonoLaboral,
                Tipo = (TipoEmpleado)dto.Tipo,
                Sueldo = dto.Sueldo,
                Activo = true, 
                CreatedBy = createdBy, 
                FechaContratacion = DateTime.UtcNow
            }; 

            await _empleadoRepository.CreateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();

            // Retornar el empleado creado 
            return (await GetByIdAsync(empleado.Id))!;
        }

        public async Task<EmpleadoDetailDto> UpdateAsync(Guid id, EmpleadoUpdateDto dto, Guid updateBy)
        {
            var empleado = await _empleadoRepository.GetByIdWithEspecialidadesAsync(id); 

            if (empleado == null)
            {
                throw new InvalidOperationException("Empleado no encontrado");
            }

            // Validar unicidad del email laboral antes de asignarlo al usuario relacionado
            if (!string.Equals(empleado.Usuario.Email, dto.EmailLaboral, StringComparison.OrdinalIgnoreCase))
            {
                // EmailExistsAsync acepta excludeUserId para evitar false positive con el mismo usuario
                if (await _usuarioRepository.EmailExistsAsync(dto.EmailLaboral, empleado.Usuario.Id))
                {
                    throw new InvalidOperationException("El email laboral ya está registrado por otro usuario");
                }

                empleado.Usuario.Email = dto.EmailLaboral.ToLower();
                empleado.Usuario.UpdatedBy = updateBy;
            }

            // Actualizar los datos del empleado 
            empleado.Cedula = dto.Cedula;
            empleado.Disponibilidad = dto.Disponibilidad;
            empleado.EmailLaboral = dto.EmailLaboral;
            empleado.TelefonoLaboral = dto.TelefonoLaboral;
            empleado.Tipo = (TipoEmpleado)dto.Tipo;
            empleado.Sueldo = dto.Sueldo;
            empleado.UpdatedBy = updateBy;

            // Actualizar datos el usuario asociado al empleado 
            empleado.Usuario.Nombre = dto.Nombre;
            empleado.Usuario.ApellidoPaterno = dto.ApellidoPaterno;
            empleado.Usuario.ApellidoMaterno = dto.ApellidoMaterno;
            empleado.Usuario.Telefono = dto.TelefonoLaboral; 

            // Actualizar especialidades si se proporcionan
            if (dto.Especialidades != null)
            {
                // Validar que todas las especialidades existan
                var especialidadesIds = dto.Especialidades.Select(e => e.EspecialidadId).ToList();
                var especialidades = await _especialidadRepository.GetAllAsync();
                var especialidadesExistentes = especialidades.Where(e => especialidadesIds.Contains(e.Id)).ToList();

                if (especialidadesExistentes.Count != especialidadesIds.Count)
                {
                    throw new InvalidOperationException("Una o más especialidades no existen");
                }

                // Limpiar especialidades anteriores
                empleado.Especialidades.Clear();

                // Agregar nuevas especialidades
                foreach (var especialidadDto in dto.Especialidades)
                {
                    empleado.Especialidades.Add(new EmpleadoEspecialidad
                    {
                        EmpleadoId = id,
                        EspecialidadId = especialidadDto.EspecialidadId,
                        Certificacion = especialidadDto.Certificacion,
                        ObtainedAt = DateTime.UtcNow
                    });
                }
            }

            // Guardar cambios
            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();

            // Retornar el DTO actualizado
            return (await GetByIdAsync(id))!;
        }

        public async Task DeleteAsync(Guid id, Guid deleteBy)
        {
            if (!await _empleadoRepository.ExistsAsync(id))
            {
                throw new InvalidOperationException("Empleado no encontrado");
            }

            await _empleadoRepository.DeleteAsync(id);
            await _empleadoRepository.SaveChangesAsync();
        }

        public async Task<EmpleadoDetailDto> DarDeBajaAsync(Guid id, Guid performedBy)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(id);

            if (empleado == null)
            {
                throw new InvalidOperationException("Empleado no encontrado");
            }

            empleado.DarDeBaja(performedBy);
            empleado.UpdatedAt = DateTime.UtcNow;
            empleado.UpdatedBy = performedBy;

            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();

            return (await GetByIdAsync(id))!;
        }

        public async Task<EmpleadoDetailDto> ReactivarAsync(Guid id, Guid performedBy)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(id);

            if (empleado == null)
            {
                throw new InvalidOperationException("Empleado no encontrado");
            }

            empleado.Reactivar(performedBy);
            empleado.UpdatedAt = DateTime.UtcNow;
            empleado.UpdatedBy = performedBy;

            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();

            return (await GetByIdAsync(id))!;
        }

        public async Task<EmpleadoDetailDto> AsignarEspecialidadesAsync(Guid empleadoId, AsignarEspecialidadesDto dto, Guid performedBy)
        {
            var empleado = await _empleadoRepository.GetByIdWithEspecialidadesAsync(empleadoId);
            if (empleado == null)
            {
                throw new InvalidOperationException("Empleado no encontrado");
            }

            // Validar que todas las especialidades existan
            var especialidadesIds = dto.Especialidades.Select(e => e.EspecialidadId).ToList();
            var especialidades = await _especialidadRepository.GetAllAsync();
            var especialidadesExistentes = especialidades.Where(e => especialidadesIds.Contains(e.Id)).ToList();

            if (especialidadesExistentes.Count != especialidadesIds.Count)
            {
                throw new InvalidOperationException("Una o más especialidades no existen");
            }

            // Limpiar especialidades anteriores
            empleado.Especialidades.Clear();

            // Agregar nuevas especialidades
            foreach (var especialidadDto in dto.Especialidades)
            {
                empleado.Especialidades.Add(new EmpleadoEspecialidad
                {
                    EmpleadoId = empleadoId,
                    EspecialidadId = especialidadDto.EspecialidadId,
                    Certificacion = especialidadDto.Certificacion,
                    ObtainedAt = DateTime.UtcNow
                });
            }

            empleado.UpdatedBy = performedBy;
            empleado.UpdatedAt = DateTime.UtcNow;

            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();

            return (await GetByIdAsync(empleadoId))!;
        }

        public async Task<EmpleadoDetailDto> RemoverEspecialidadAsync(Guid empleadoId, Guid especialidadId, Guid performedBy)
        {
            var empleado = await _empleadoRepository.GetByIdWithEspecialidadesAsync(empleadoId);
            if (empleado == null)
            {
                throw new InvalidOperationException("Empleado no encontrado");
            }

            var especialidad = empleado.Especialidades.FirstOrDefault(e => e.EspecialidadId == especialidadId);
            if (especialidad == null)
            {
                throw new InvalidOperationException("El empleado no tiene asignada esta especialidad");
            }

            empleado.Especialidades.Remove(especialidad);
            empleado.UpdatedBy = performedBy;
            empleado.UpdatedAt = DateTime.UtcNow;

            await _empleadoRepository.UpdateAsync(empleado);
            await _empleadoRepository.SaveChangesAsync();

            return (await GetByIdAsync(empleadoId))!;
        }
    }
}
