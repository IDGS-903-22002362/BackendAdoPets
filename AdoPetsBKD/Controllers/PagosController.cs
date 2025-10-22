using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;

    public PagosController(IPagoService pagoService)
    {
        _pagoService = pagoService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CreatePago([FromBody] CreatePagoDto dto)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var pago = await _pagoService.CreatePagoAsync(dto, userId);
            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago creado exitosamente"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("paypal/create-order")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PayPalOrderResponseDto>>> CreatePayPalOrder([FromBody] CreatePagoPayPalDto dto)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var order = await _pagoService.CreatePayPalOrderAsync(dto, userId);
            return Ok(ApiResponse<PayPalOrderResponseDto>.SuccessResponse(order, "Orden de PayPal creada exitosamente"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("paypal/capture")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CapturePayPalPayment([FromBody] CapturePayPalPaymentDto dto)
    {
        try
        {
            var pago = await _pagoService.CapturePayPalPaymentAsync(dto.OrderId);
            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago capturado exitosamente"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> GetPagoById(Guid id)
    {
        try
        {
            var pago = await _pagoService.GetPagoByIdAsync(id);
            if (pago == null)
                return NotFound(ApiResponse<PagoDto>.ErrorResponse("Pago no encontrado"));

            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("paypal/{orderId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> GetPagoByPayPalOrderId(string orderId)
    {
        try
        {
            var pago = await _pagoService.GetPagoByPayPalOrderIdAsync(orderId);
            if (pago == null)
                return NotFound(ApiResponse<PagoDto>.ErrorResponse("Pago no encontrado"));

            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("usuario/{usuarioId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<PagoDto>>>> GetPagosByUsuario(Guid usuarioId)
    {
        try
        {
            var pagos = await _pagoService.GetPagosByUsuarioAsync(usuarioId);
            return Ok(ApiResponse<List<PagoDto>>.SuccessResponse(pagos));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<PagoDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/cancelar")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CancelarPago(Guid id, [FromBody] string? motivo)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var pago = await _pagoService.CancelarPagoAsync(id, userId, motivo);
            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago cancelado"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("webhook/paypal")]
    [AllowAnonymous]
    public async Task<IActionResult> PayPalWebhook([FromBody] PayPalWebhookDto webhook)
    {
        try
        {
            await _pagoService.ProcessWebhookAsync(webhook);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
