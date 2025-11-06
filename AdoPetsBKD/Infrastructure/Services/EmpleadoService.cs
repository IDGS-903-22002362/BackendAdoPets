using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Empleados;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IUsuarioService _usuarioService;
        private readonly IUsuarioRepository _usuarioRepository;

        public EmpleadoService(
            IEmpleadoRepository empleadoRepository, 
            IUsuarioService usuarioService,
            IUsuarioRepository usuarioRepository)
        {
            _empleadoRepository = empleadoRepository;
            _usuarioService = usuarioService;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<PagedResponse<EmpleadoListDto>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false)
        {
            var empleados = await _empleadoRepository.GetAllAsync(pageNumber, pageSize, includeInactive);

            var totalCount = await _empleadoRepository.GetTotalCountAsync(includeInactive);

            var empleadosDto = empleados.Select(e => new EmpleadoListDto
            {
                Id = e.Id,
                NombreCompleto = e.Usuario != null ? e.Usuario.Nombre + " " + e.Usuario.ApellidoPaterno + " " + e.Usuario.ApellidoMaterno : string.Empty,
                EmailLaboral = e.EmailLaboral,
                TelefonoLaboral = e.TelefonoLaboral,
                FechaContratacion = e.FechaContratacion,
                TipoEmpleado = e.Usuario != null ? string.Join(", ", e.Usuario.UsuarioRoles.Select(ur => ur.Rol != null ? ur.Rol.Nombre : string.Empty)) : string.Empty,
                Sueldo = e.Sueldo,
                Especialidades = e.Especialidades != null ? e.Especialidades.Select(es => es.Especialidad.Descripcion).ToList() : new List<string>()
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
            var empleado = await _empleadoRepository.GetByIdAsync(id);

            if (empleado == null || empleado.Usuario == null) 
            {
                return null;
            }

            return new EmpleadoDetailDto
            {
                Id = empleado.Id,
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
                Cedula = empleado.Cedula
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
            var empleado = await _empleadoRepository.GetByIdAsync(id); 

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
    }
}
