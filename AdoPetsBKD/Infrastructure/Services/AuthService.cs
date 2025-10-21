using AdoPetsBKD.Application.DTOs.Auth;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Security;
using AdoPetsBKD.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AdoPetsBKD.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly PoliciesSettings _policiesSettings;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IRolRepository rolRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<PoliciesSettings> policiesSettings)
    {
        _usuarioRepository = usuarioRepository;
        _rolRepository = rolRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _policiesSettings = policiesSettings.Value;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Buscar usuario por email
        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email, includeRoles: true);

        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(request.Password, usuario.PasswordHash, usuario.PasswordSalt))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas");
        }

        // Verificar estatus del usuario
        if (usuario.Estatus != EstatusUsuario.Activo)
        {
            throw new UnauthorizedAccessException("Usuario inactivo o bloqueado");
        }

        // Registrar acceso
        usuario.RegistrarAcceso();
        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        // Generar tokens
        var roles = usuario.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList();
        var accessToken = _tokenService.GenerateAccessToken(usuario.Id, usuario.Email, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Crear respuesta
        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600, // 1 hora en segundos
            Usuario = new UsuarioDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                ApellidoPaterno = usuario.ApellidoPaterno,
                ApellidoMaterno = usuario.ApellidoMaterno,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                NombreCompleto = usuario.NombreCompleto,
                Estatus = usuario.Estatus,
                UltimoAccesoAt = usuario.UltimoAccesoAt,
                Roles = roles,
                TienePoliticasAceptadas = usuario.TienePoliticasAceptadas(_policiesSettings.CurrentVersion),
                CreatedAt = usuario.CreatedAt
            }
        };
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Validar que el email no esté registrado
        if (await _usuarioRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("El email ya está registrado");
        }

        // Validar aceptación de políticas
        if (!request.AceptaPoliticas)
        {
            throw new InvalidOperationException("Debe aceptar las políticas de privacidad");
        }

        // Crear hash de contraseña
        _passwordHasher.CreatePasswordHash(request.Password, out string passwordHash, out string passwordSalt);

        // Crear usuario
        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            ApellidoPaterno = request.ApellidoPaterno,
            ApellidoMaterno = request.ApellidoMaterno,
            Email = request.Email.ToLower(),
            Telefono = request.Telefono,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Estatus = EstatusUsuario.Activo
        };

        usuario.AceptarPoliticas(_policiesSettings.CurrentVersion);

        // Asignar rol de Adoptante por defecto
        var rolAdoptante = await _rolRepository.GetByNameAsync(Roles.Adoptante);
        if (rolAdoptante != null)
        {
            usuario.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuario.Id,
                RolId = rolAdoptante.Id
            });
        }

        // Guardar usuario
        await _usuarioRepository.CreateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        // Iniciar sesión automáticamente
        return await LoginAsync(new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password
        });
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // En una implementación real, deberías almacenar los refresh tokens en la base de datos
        // y validar que el token exista y no haya expirado
        // Por ahora, solo validamos el formato

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token inválido");
        }

        // TODO: Implementar validación de refresh token desde la base de datos
        throw new NotImplementedException("Funcionalidad de refresh token pendiente de implementar");
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);

        if (usuario == null)
        {
            throw new InvalidOperationException("Usuario no encontrado");
        }

        // Verificar contraseña actual
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, usuario.PasswordHash, usuario.PasswordSalt))
        {
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta");
        }

        // Crear nuevo hash de contraseña
        _passwordHasher.CreatePasswordHash(request.NewPassword, out string passwordHash, out string passwordSalt);

        usuario.PasswordHash = passwordHash;
        usuario.PasswordSalt = passwordSalt;
        usuario.UpdatedBy = userId;

        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return true;
    }

    public async Task LogoutAsync(Guid userId)
    {
        // En una implementación real, aquí invalidarías el refresh token del usuario
        // almacenado en la base de datos
        await Task.CompletedTask;
    }
}
