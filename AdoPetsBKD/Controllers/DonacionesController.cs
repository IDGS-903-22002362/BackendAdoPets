using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Donaciones;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DonacionesController : ControllerBase
{
    private readonly IDonacionesService _donacionesService;
    private readonly ILogger<DonacionesController> _logger;

    public DonacionesController(
        IDonacionesService donacionesService,
        ILogger<DonacionesController> logger)
    {
        _donacionesService = donacionesService;
        _logger = logger;
    }

    // Crear una nueva donación

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DonacionDto>>> CreateDonacion([FromBody] CreateDonacionDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var donacion = await _donacionesService.CreateDonacionAsync(dto, userId);
            return Ok(ApiResponse<DonacionDto>.SuccessResponse(donacion, "Donación creada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear donación");
            return BadRequest(ApiResponse<DonacionDto>.ErrorResponse(ex.Message));
        }
    }

    // Crear orden de PayPal para donación
    [HttpPost("paypal/create-order")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PayPalDonacionResponseDto>>> CreatePayPalOrder([FromBody] CreatePayPalDonacionDto dto)
    {
        try
        {
            var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : Guid.Empty;
            
            // Si es anónima o el usuarioId no existe, usar null
            Guid? usuarioId = dto.Anonima ? null : dto.UsuarioId;
            
            var order = await _donacionesService.CreatePayPayDonacionAsync(
                new CreateDonacionDto
                {
                    UsuarioId = usuarioId,
                    Monto = dto.Monto,
                    Moneda = dto.Moneda,
                    Mensaje = dto.Mensaje,
                    Anonima = dto.Anonima
                },
                userId
            );
            return Ok(ApiResponse<PayPalDonacionResponseDto>.SuccessResponse(order, "Orden de PayPal creada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden de PayPal");
            return BadRequest(ApiResponse<PayPalDonacionResponseDto>.ErrorResponse(ex.Message));
        }
    }

    // Capturar donación vía PayPal
    [HttpPost("paypal/capture")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DonacionDto>>> CapturePayPalDonacion([FromBody] CapturePayPalDonacionDto dto)
    {
        try
        {
            var donacion = await _donacionesService.CapturePayPalDonacionAsync(dto.OrderId);
            return Ok(ApiResponse<DonacionDto>.SuccessResponse(donacion, "Donación capturada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al capturar donación");
            return BadRequest(ApiResponse<DonacionDto>.ErrorResponse(ex.Message));
        }
    }

    // Obtener donación por ID
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DonacionDto>>> GetDonacionById(Guid id)
    {
        try
        {
            var donacion = await _donacionesService.GetDonacionByIdAsync(id);
            if (donacion == null)
                return NotFound(ApiResponse<DonacionDto>.ErrorResponse("Donación no encontrada"));

            return Ok(ApiResponse<DonacionDto>.SuccessResponse(donacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener donación");
            return BadRequest(ApiResponse<DonacionDto>.ErrorResponse(ex.Message));
        }
    }
    // Obtener donación por PayPal Order ID

    [HttpGet("paypal/{orderId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<DonacionDto>>> GetDonacionByPayPalOrderId(string orderId)
    {
        try
        {
            var donacion = await _donacionesService.GetDonacionByPayPalOrderIdAsync(orderId);
            if (donacion == null)
                return NotFound(ApiResponse<DonacionDto>.ErrorResponse("Donación no encontrada"));

            return Ok(ApiResponse<DonacionDto>.SuccessResponse(donacion));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener donación");
            return BadRequest(ApiResponse<DonacionDto>.ErrorResponse(ex.Message));
        }
    }

    // Obtener donaciones por usuario
    [HttpGet("usuario/{usuarioId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<DonacionDto>>>> GetDonacionesByUsuario(Guid usuarioId)
    {
        try
        {
            var donaciones = await _donacionesService.GetDonacionesByUsuarioAsync(usuarioId);
            return Ok(ApiResponse<List<DonacionDto>>.SuccessResponse(donaciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener donaciones del usuario");
            return BadRequest(ApiResponse<List<DonacionDto>>.ErrorResponse(ex.Message));
        }
    }

    // Obtener donaciones públicas con paginación y filtro de anónimas
    [HttpGet("publicas")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<DonacionDto>>>> GetDonacionesPublicas(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] FiltroDonacionAnonima filtro = FiltroDonacionAnonima.SoloPublicas)
    {
        try
        {
            var donaciones = await _donacionesService.GetDonacionesAsync(pageNumber, pageSize, filtro);
            return Ok(ApiResponse<List<DonacionDto>>.SuccessResponse(donaciones));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener donaciones públicas");
            return BadRequest(ApiResponse<List<DonacionDto>>.ErrorResponse(ex.Message));
        }
    }

    // Cancelar donación
    [HttpPut("{id}/cancelar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DonacionDto>>> CancelarDonacion(
        Guid id, 
        [FromBody] string? motivo)
    {
        try
        {
            var userId = GetCurrentUserId();
            var donacion = await _donacionesService.CancelarDonacionAsync(id, userId, motivo);
            return Ok(ApiResponse<DonacionDto>.SuccessResponse(donacion, "Donación cancelada"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar donación");
            return BadRequest(ApiResponse<DonacionDto>.ErrorResponse(ex.Message));
        }
    }

    // Webhook de PayPal para notificaciones de donaciones
    [HttpPost("webhook/paypal")]
    [AllowAnonymous]
    public async Task<IActionResult> PayPalWebhook([FromBody] PayPalWebhookDonacionDto webhook)
    {
        try
        {
            await _donacionesService.ProcessWebhookAsync(webhook);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar webhook de PayPal");
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }
        return userId;
    }
}
