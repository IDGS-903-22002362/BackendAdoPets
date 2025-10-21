using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Usuarios;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Security;

namespace AdoPetsBKD.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de usuarios
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
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
}
