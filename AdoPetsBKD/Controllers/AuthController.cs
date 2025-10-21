using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Auth;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

/// <summary>
/// Controlador de autenticaci�n
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
    /// Iniciar sesi�n
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
            return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(result, "Inicio de sesi�n exitoso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Intento de inicio de sesi�n fallido: {Email}", request.Email);
            return Unauthorized(ApiResponse<object>.ErrorResponse("Credenciales inv�lidas", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesi�n: {Email}", request.Email);
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al iniciar sesi�n", ex.Message));
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
            _logger.LogWarning("Intento de renovaci�n de token fallido");
            return Unauthorized(ApiResponse<object>.ErrorResponse("Token inv�lido", ex.Message));
        }
        catch (NotImplementedException)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Funcionalidad pendiente", "La renovaci�n de tokens a�n no est� implementada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al renovar token");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al renovar token", ex.Message));
        }
    }

    /// <summary>
    /// Cambiar contrase�a del usuario autenticado
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
            _logger.LogInformation("Contrase�a cambiada exitosamente para usuario: {UserId}", userId);
            return Ok(ApiResponse<bool>.SuccessResponse(result, "Contrase�a cambiada exitosamente"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Contrase�a incorrecta", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar contrase�a");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al cambiar contrase�a", ex.Message));
        }
    }

    /// <summary>
    /// Cerrar sesi�n
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
            _logger.LogInformation("Usuario cerr� sesi�n: {UserId}", userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Sesi�n cerrada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cerrar sesi�n");
            return BadRequest(ApiResponse<object>.ErrorResponse("Error al cerrar sesi�n", ex.Message));
        }
    }

    /// <summary>
    /// Obtener informaci�n del usuario autenticado
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
