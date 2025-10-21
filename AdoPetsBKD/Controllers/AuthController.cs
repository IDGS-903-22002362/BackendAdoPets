using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Auth;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

/// <summary>
/// Controlador de autenticación
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar sesión
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Inicio de sesión exitoso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Intento de inicio de sesión fallido: {Email}", request.Email);
            return Unauthorized(ApiResponse<object>.ErrorResponse("Credenciales inválidas", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión: {Email}", request.Email);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al iniciar sesión", ex.Message));
        }
    }

    /// <summary>
    /// Registrar nuevo usuario
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            _logger.LogInformation("Usuario registrado exitosamente: {Email}", request.Email);
            return CreatedAtAction(nameof(Register), ApiResponse<LoginResponseDto>.SuccessResponse(result, "Usuario registrado exitosamente"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al registrar usuario: {Message}", ex.Message);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al registrar usuario", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al registrar usuario: {Email}", request.Email);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al registrar usuario", ex.Message));
        }
    }

    /// <summary>
    /// Refrescar token de acceso
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Token renovado exitosamente"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Intento de renovación de token fallido");
            return Unauthorized(ApiResponse<object>.ErrorResponse("Token inválido", ex.Message));
        }
        catch (NotImplementedException)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Funcionalidad pendiente", "La renovación de tokens aún no está implementada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renovar token");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al renovar token", ex.Message));
        }
    }

    /// <summary>
    /// Cambiar contraseña del usuario autenticado
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _authService.ChangePasswordAsync(userId, request);
            _logger.LogInformation("Contraseña cambiada exitosamente para usuario: {UserId}", userId);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Contraseña cambiada exitosamente"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Contraseña incorrecta", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contraseña");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al cambiar contraseña", ex.Message));
        }
    }

    /// <summary>
    /// Cerrar sesión
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _authService.LogoutAsync(userId);
            _logger.LogInformation("Usuario cerró sesión: {UserId}", userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Sesión cerrada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar sesión");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al cerrar sesión", ex.Message));
        }
    }

    /// <summary>
    /// Obtener información del usuario autenticado
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UsuarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userId = GetCurrentUserId();
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var usuario = new UsuarioDto
            {
                Id = userId,
                Email = email,
                Roles = roles
            };

            return Ok(ApiResponse<UsuarioDto>.SuccessResponse(usuario, "Usuario autenticado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario autenticado");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al obtener usuario", ex.Message));
        }
    }

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        return userId;
    }

    #endregion
}
