using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Usuarios;
using AdoPetsBKD.Application.DTOs.Roles;
using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Domain.Entities.Mascotas;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de usuarios
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdoPetsDbContext _context;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher,
        AdoPetsDbContext context)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
        _context = context;
    }

    public async Task<PagedResponse<UsuarioListDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var usuarios = await _usuarioRepository.GetAllAsync(pageNumber, pageSize, includeRoles: true);
        var totalCount = await _usuarioRepository.GetTotalCountAsync();

        var usuariosDto = usuarios.Select(u => new UsuarioListDto
        {
            Id = u.Id,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            Telefono = u.Telefono,
            Estatus = u.Estatus,
            Roles = u.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList(),
            UltimoAccesoAt = u.UltimoAccesoAt,
            CreatedAt = u.CreatedAt
        }).ToList();

        return new PagedResponse<UsuarioListDto>
        {
            Items = usuariosDto,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<UsuarioDetailDto?> GetByIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id, includeRoles: true);

        if (usuario == null)
        {
            return null;
        }

        return new UsuarioDetailDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            Telefono = usuario.Telefono,
            Estatus = usuario.Estatus,
            UltimoAccesoAt = usuario.UltimoAccesoAt,
            AceptoPoliticasVersion = usuario.AceptoPoliticasVersion,
            AceptoPoliticasAt = usuario.AceptoPoliticasAt,
            Roles = usuario.UsuarioRoles.Select(ur => new RolDto
            {
                Id = ur.Rol.Id,
                Nombre = ur.Rol.Nombre,
                Descripcion = ur.Rol.Descripcion
            }).ToList(),
            CreatedAt = usuario.CreatedAt,
            UpdatedAt = usuario.UpdatedAt
        };
    }

    public async Task<UsuarioDetailDto> CreateAsync(CreateUsuarioDto dto, Guid createdBy)
    {
        // Validar que el email no exista
        if (await _usuarioRepository.EmailExistsAsync(dto.Email))
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        // Validar que los roles existan
        var roles = await _rolRepository.GetByIdsAsync(dto.RolesIds);
        if (roles.Count != dto.RolesIds.Count)
        {
            throw new InvalidOperationException("Uno o más roles no existen");
        }

        // Crear hash de contraseña
        _passwordHasher.CreatePasswordHash(dto.Password, out string passwordHash, out string passwordSalt);

        // Crear usuario
        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            ApellidoPaterno = dto.ApellidoPaterno,
            ApellidoMaterno = dto.ApellidoMaterno,
            Email = dto.Email.ToLower(),
            Telefono = dto.Telefono,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Estatus = EstatusUsuario.Activo,
            CreatedBy = createdBy
        };

        // Asignar roles
        foreach (var rol in roles)
        {
            usuario.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = rol.Id
            });
        }

        await _usuarioRepository.CreateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        // Retornar usuario creado
        return (await GetByIdAsync(usuario.Id))!;
    }

    public async Task<UsuarioDetailDto> UpdateAsync(Guid id, UpdateUsuarioDto dto, Guid updatedBy)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id, includeRoles: true);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // Actualizar datos básicos
        usuario.Nombre = dto.Nombre;
        usuario.ApellidoPaterno = dto.ApellidoPaterno;
        usuario.ApellidoMaterno = dto.ApellidoMaterno;
        usuario.Telefono = dto.Telefono;
        usuario.UpdatedBy = updatedBy;

        // Actualizar roles si se proporcionaron
        if (dto.RolesIds.Any())
        {
            var roles = await _rolRepository.GetByIdsAsync(dto.RolesIds);
            if (roles.Count != dto.RolesIds.Count)
            {
                throw new InvalidOperationException("Uno o más roles no existen");
            }

            // Remover roles actuales
            usuario.UsuarioRoles.Clear();

            // Agregar nuevos roles
            foreach (var rol in roles)
            {
                usuario.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rol.Id
                });
            }
        }

        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return (await GetByIdAsync(usuario.Id))!;
    }

    public async Task DeleteAsync(Guid id, Guid deletedBy)
    {
        if (!await _usuarioRepository.ExistsAsync(id))
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        await _usuarioRepository.DeleteAsync(id);
        await _usuarioRepository.SaveChangesAsync();
    }

    public async Task<bool> ActivateAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);

        if (usuario == null)
        {
            return false;
        }

        usuario.Estatus = EstatusUsuario.Activo;
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateAsync(Guid id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);

        if (usuario == null)
        {
            return false;
        }

        usuario.Estatus = EstatusUsuario.Inactivo;
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AssignRolesAsync(Guid userId, List<Guid> rolesIds)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId, includeRoles: true);

        if (usuario == null)
        {
            return false;
        }

        var roles = await _rolRepository.GetByIdsAsync(rolesIds);
        if (roles.Count != rolesIds.Count)
        {
            throw new InvalidOperationException("Uno o más roles no existen");
        }

        // Remover roles actuales
        usuario.UsuarioRoles.Clear();

        // Agregar nuevos roles
        foreach (var rol in roles)
        {
            usuario.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = rol.Id
            });
        }

        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<AdoptanteConMascotasDto>> GetAdoptantesConMascotasAsync()
    {
        // Obtener todos los usuarios con rol "Adoptante"
        var adoptantes = await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .Where(u => u.UsuarioRoles.Any(ur => ur.Rol.Nombre == "Adoptante"))
            .ToListAsync();

        var resultado = new List<AdoptanteConMascotasDto>();

        foreach (var adoptante in adoptantes)
        {
            var adoptanteDto = await GetAdoptanteConMascotasByIdAsync(adoptante.Id);
            if (adoptanteDto != null)
            {
                resultado.Add(adoptanteDto);
            }
        }

        return resultado;
    }

    public async Task<AdoptanteConMascotasDto?> GetAdoptanteConMascotasByIdAsync(Guid usuarioId)
    {
        // Obtener el usuario
        var usuario = await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario == null)
        {
            return null;
        }

        // Verificar que sea adoptante
        var esAdoptante = usuario.UsuarioRoles.Any(ur => ur.Rol.Nombre == "Adoptante");
        
        var adoptanteDto = new AdoptanteConMascotasDto
        {
            UsuarioId = usuario.Id,
            Nombre = usuario.Nombre,
            ApellidoPaterno = usuario.ApellidoPaterno,
            ApellidoMaterno = usuario.ApellidoMaterno,
            NombreCompleto = usuario.NombreCompleto,
            Email = usuario.Email,
            Telefono = usuario.Telefono,
            UltimoAccesoAt = usuario.UltimoAccesoAt,
            CreatedAt = usuario.CreatedAt,
            Mascotas = new List<MascotaAdoptanteDto>()
        };

        // 1. Obtener mascotas adoptadas del refugio (solicitudes aprobadas)
        var solicitudesAprobadas = await _context.SolicitudesAdopcion
            .Include(s => s.Mascota)
                .ThenInclude(m => m.Fotos)
            .Where(s => s.UsuarioId == usuarioId && s.Estado == EstadoSolicitudAdopcion.Aprobada)
            .ToListAsync();

        foreach (var solicitud in solicitudesAprobadas)
        {
            if (solicitud.Mascota != null)
            {
                adoptanteDto.Mascotas.Add(new MascotaAdoptanteDto
                {
                    MascotaId = solicitud.Mascota.Id,
                    Nombre = solicitud.Mascota.Nombre,
                    Especie = solicitud.Mascota.Especie,
                    Raza = solicitud.Mascota.Raza,
                    Sexo = (int)solicitud.Mascota.Sexo,
                    FechaNacimiento = solicitud.Mascota.FechaNacimiento,
                    EdadEnAnios = solicitud.Mascota.FechaNacimiento.HasValue
                        ? (int)((DateTime.Now - solicitud.Mascota.FechaNacimiento.Value).TotalDays / 365.25)
                        : null,
                    Personalidad = solicitud.Mascota.Personalidad,
                    EstadoSalud = solicitud.Mascota.EstadoSalud,
                    Estatus = (int)solicitud.Mascota.Estatus,
                    EstatusNombre = solicitud.Mascota.Estatus.ToString(),
                    Tipo = (int)solicitud.Mascota.Tipo,
                    FechaAdquisicion = solicitud.FechaAprobacion ?? solicitud.FechaSolicitud,
                    FechaSolicitudAdopcion = solicitud.FechaSolicitud,
                    FechaAprobacionAdopcion = solicitud.FechaAprobacion,
                    Fotos = solicitud.Mascota.Fotos?.Select(f => new AddMascotaFotoDto
                    {
                        Id = f.Id,
                        StorageKey = f.StorageKey,
                        MimeType = f.MimeType,
                        Orden = f.Orden,
                        EsPrincipal = f.EsPrincipal
                    }).OrderBy(f => f.Orden).ToList() ?? new List<AddMascotaFotoDto>()
                });
            }
        }

        // 2. Obtener mascotas registradas directamente por el usuario
        var mascotasRegistradas = await _context.Mascotas
            .Include(m => m.Fotos)
            .Where(m => m.PropietarioId == usuarioId && m.Tipo == TipoMascota.DeUsuario && m.DeletedAt == null)
            .ToListAsync();

        foreach (var mascota in mascotasRegistradas)
        {
            adoptanteDto.Mascotas.Add(new MascotaAdoptanteDto
            {
                MascotaId = mascota.Id,
                Nombre = mascota.Nombre,
                Especie = mascota.Especie,
                Raza = mascota.Raza,
                Sexo = (int)mascota.Sexo,
                FechaNacimiento = mascota.FechaNacimiento,
                EdadEnAnios = mascota.FechaNacimiento.HasValue
                    ? (int)((DateTime.Now - mascota.FechaNacimiento.Value).TotalDays / 365.25)
                    : null,
                Personalidad = mascota.Personalidad,
                EstadoSalud = mascota.EstadoSalud,
                Estatus = (int)mascota.Estatus,
                EstatusNombre = mascota.Estatus.ToString(),
                Tipo = (int)mascota.Tipo,
                FechaAdquisicion = mascota.CreatedAt,
                FechaSolicitudAdopcion = null,
                FechaAprobacionAdopcion = null,
                Fotos = mascota.Fotos?.Select(f => new AddMascotaFotoDto
                {
                    Id = f.Id,
                    StorageKey = f.StorageKey,
                    MimeType = f.MimeType,
                    Orden = f.Orden,
                    EsPrincipal = f.EsPrincipal
                }).OrderBy(f => f.Orden).ToList() ?? new List<AddMascotaFotoDto>()
            });
        }

        // Calcular totales
        adoptanteDto.TotalMascotas = adoptanteDto.Mascotas.Count;
        adoptanteDto.MascotasAdoptadas = adoptanteDto.Mascotas.Count(m => m.Tipo == 1);
        adoptanteDto.MascotasRegistradas = adoptanteDto.Mascotas.Count(m => m.Tipo == 2);

        // Ordenar mascotas por fecha de adquisición (más recientes primero)
        adoptanteDto.Mascotas = adoptanteDto.Mascotas.OrderByDescending(m => m.FechaAdquisicion).ToList();

        return adoptanteDto;
    }
}
