using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PagosController : ControllerBase
{
    private readonly IPagoService _pagoService;
    private readonly ILogger<PagosController> _logger;

    public PagosController(IPagoService pagoService, ILogger<PagosController> logger)
    {
        _pagoService = pagoService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CreatePago([FromBody] CreatePagoDto dto)
    {
        try
        {
            var userId = GetUserId();
            var pago = await _pagoService.CreatePagoAsync(dto, userId);
            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago creado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear pago");
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("paypal/create-order")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PayPalOrderResponseDto>>> CreatePayPalOrder([FromBody] CreatePagoPayPalDto dto)
    {
        try
        {
            var userId = GetUserId();
            var order = await _pagoService.CreatePayPalOrderAsync(dto, userId);
            return Ok(ApiResponse<PayPalOrderResponseDto>.SuccessResponse(order, "Orden de PayPal creada exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden PayPal");
            return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("paypal/capture/{orderId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CapturePayPalPayment(string orderId)
    {
        try
        {
            _logger.LogInformation("PagosController.CapturePayPalPayment - INICIO. OrderId recibido: {OrderId}", orderId);
            
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogError("PagosController.CapturePayPalPayment - OrderId está vacío o es null");
                return BadRequest(ApiResponse<PagoDto>.ErrorResponse("El OrderId es requerido"));
            }
            
            var pago = await _pagoService.CapturePayPalPaymentAsync(orderId);
            
            _logger.LogInformation(
                "PagosController.CapturePayPalPayment - Pago capturado exitosamente. OrderId: {OrderId}, PagoId: {PagoId}, Monto: {Monto}",
                orderId, pago.Id, pago.Monto);
            
            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago capturado exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PagosController.CapturePayPalPayment - Error al capturar pago. OrderId: {OrderId}, Message: {Message}", orderId, ex.Message);
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse($"Error al capturar pago: {ex.Message}"));
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
            _logger.LogError(ex, "Error al obtener pago {PagoId}", id);
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
            _logger.LogError(ex, "Error al obtener pago por PayPal OrderId");
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
            _logger.LogError(ex, "Error al obtener pagos del usuario {UsuarioId}", usuarioId);
            return BadRequest(ApiResponse<List<PagoDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Obtiene todos los pagos asociados a una cita específica
    /// </summary>
    [HttpGet("cita/{citaId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<PagoDto>>>> GetPagosByCita(Guid citaId)
    {
        try
        {
            var pagos = await _pagoService.GetPagosByCitaIdAsync(citaId);
            return Ok(ApiResponse<List<PagoDto>>.SuccessResponse(pagos, 
                $"Se encontraron {pagos.Count} pago(s) para la cita"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pagos de la cita {CitaId}", citaId);
            return StatusCode(500, ApiResponse<List<PagoDto>>.ErrorResponse("Error al obtener pagos de la cita"));
        }
    }

    /// <summary>
    /// Obtiene todas las citas con pagos pendientes (anticipo pagado pero saldo pendiente)
    /// </summary>
    [HttpGet("pendientes")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<List<PagoPendienteDto>>>> GetPagosPendientes()
    {
        try
        {
            var pagosPendientes = await _pagoService.GetPagosPendientesAsync();
            return Ok(ApiResponse<List<PagoPendienteDto>>.SuccessResponse(pagosPendientes,
                $"Se encontraron {pagosPendientes.Count} cita(s) con pagos pendientes"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pagos pendientes");
            return StatusCode(500, ApiResponse<List<PagoPendienteDto>>.ErrorResponse("Error al obtener pagos pendientes"));
        }
    }

    /// <summary>
    /// Obtiene los pagos pendientes de un usuario específico
    /// </summary>
    [HttpGet("pendientes/usuario/{usuarioId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<PagoPendienteDto>>>>
        GetPagosPendientesByUsuario(Guid usuarioId)
    {
        try
        {
            // Verificar que el usuario autenticado sea el mismo o sea admin
            var currentUserId = GetUserId();
            var isAdmin = User.IsInRole("Admin");
            
            if (currentUserId != usuarioId && !isAdmin)
            {
                return Forbid();
            }

            var pagosPendientes = await _pagoService.GetPagosPendientesByUsuarioAsync(usuarioId);
            return Ok(ApiResponse<List<PagoPendienteDto>>.SuccessResponse(pagosPendientes,
                $"Se encontraron {pagosPendientes.Count} cita(s) con pagos pendientes"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener pagos pendientes del usuario {UsuarioId}", usuarioId);
            return StatusCode(500, ApiResponse<List<PagoPendienteDto>>.ErrorResponse("Error al obtener pagos pendientes"));
        }
    }

    /// <summary>
    /// Completa el pago restante de una cita (pago del 50% faltante)
    /// </summary>
    [HttpPost("completar-pago")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CompletarPagoRestante([FromBody] CompletarPagoRestanteDto dto)
    {
        try
        {
            // Log de entrada
            _logger.LogInformation(
                "CompletarPagoRestante - CitaId: {CitaId}, MetodoPago: {MetodoPago}, ModelState: {IsValid}",
                dto.CitaId, dto.MetodoPago, ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning(
                    "CompletarPagoRestante - Datos inválidos para CitaId: {CitaId}. Errores: {Errors}",
                    dto.CitaId, string.Join(", ", errors));
                
                return BadRequest(ApiResponse<PagoDto>.ErrorResponse("Datos inválidos", errors));
            }

            // Validación adicional para PayPal
            if (dto.MetodoPago == 1)
            {
                _logger.LogWarning(
                    "CompletarPagoRestante - Intento de usar PayPal (método 1) para CitaId: {CitaId}. Usar /completar-pago/paypal en su lugar",
                    dto.CitaId);
                
                return BadRequest(ApiResponse<PagoDto>.ErrorResponse(
                    "Para pagos con PayPal use el endpoint /completar-pago/paypal"));
            }

            var userId = GetUserId();
            var pago = await _pagoService.CompletarPagoRestanteAsync(dto, userId);

            _logger.LogInformation(
                "CompletarPagoRestante - Pago completado exitosamente. PagoId: {PagoId}, CitaId: {CitaId}, Monto: {Monto}",
                pago.Id, dto.CitaId, pago.Monto);

            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago completado exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "CompletarPagoRestante - Cita o pago no encontrado: CitaId={CitaId}", dto.CitaId);
            return NotFound(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CompletarPagoRestante - Operación inválida: CitaId={CitaId}", dto.CitaId);
            return BadRequest(ApiResponse<PagoDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CompletarPagoRestante - Error inesperado al completar pago: CitaId={CitaId}", dto.CitaId);
            return StatusCode(500, ApiResponse<PagoDto>.ErrorResponse("Error al completar pago"));
        }
    }

    /// <summary>
    /// Crea orden PayPal para completar el pago restante de una cita
    /// </summary>
    [HttpPost("completar-pago/paypal")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PayPalOrderResponseDto>>> CompletarPagoRestantePayPal([FromBody] CompletarPagoRestantePayPalDto dto)
    {
        try
        {
            _logger.LogInformation(
                "CompletarPagoRestantePayPal - INICIO. CitaId: {CitaId}, ReturnUrl: {ReturnUrl}, CancelUrl: {CancelUrl}, DTO recibido: {@Dto}",
                dto?.CitaId, dto?.ReturnUrl, dto?.CancelUrl, dto);

            if (dto == null)
            {
                _logger.LogError("CompletarPagoRestantePayPal - DTO es NULL");
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("No se recibieron datos en la solicitud"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning(
                    "CompletarPagoRestantePayPal - Datos inválidos para CitaId: {CitaId}. Errores: {Errors}",
                    dto.CitaId, string.Join(", ", errors));
                
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();

            // 1. Verificar que la cita existe y obtener el propietario
            var cita = await _pagoService.GetPagosByCitaIdAsync(dto.CitaId);
            if (cita == null || !cita.Any())
            {
                _logger.LogWarning("CompletarPagoRestantePayPal - Cita no encontrada: {CitaId}", dto.CitaId);
                return NotFound(ApiResponse<PayPalOrderResponseDto>.ErrorResponse($"Cita con ID {dto.CitaId} no encontrada o sin pagos"));
            }

            // 2. Calcular el monto pendiente
            var pagos = cita.Where(p => p.Estado == 3).ToList(); // EstadoPago.Completado = 3
            
            if (!pagos.Any())
            {
                _logger.LogWarning("CompletarPagoRestantePayPal - No hay pagos completados para la cita: {CitaId}", dto.CitaId);
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("Esta cita no tiene pagos registrados"));
            }

            var pagoAnticipo = pagos.FirstOrDefault(p => p.EsAnticipo);
            
            if (pagoAnticipo == null)
            {
                _logger.LogWarning("CompletarPagoRestantePayPal - No hay pago de anticipo para la cita: {CitaId}", dto.CitaId);
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("Esta cita no tiene un pago de anticipo"));
            }

            if (!pagoAnticipo.MontoTotal.HasValue || pagoAnticipo.MontoTotal.Value <= 0)
            {
                _logger.LogWarning("CompletarPagoRestantePayPal - No se puede determinar el monto total para la cita: {CitaId}", dto.CitaId);
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("No se puede determinar el monto total de la cita"));
            }

            var totalPagado = pagos.Sum(p => p.Monto);
            var montoPendiente = pagoAnticipo.MontoTotal.Value - totalPagado;

            if (montoPendiente <= 0)
            {
                _logger.LogWarning("CompletarPagoRestantePayPal - La cita ya está completamente pagada: {CitaId}", dto.CitaId);
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("Esta cita ya está completamente pagada"));
            }

            _logger.LogInformation(
                "CompletarPagoRestantePayPal - Calculando pago. CitaId: {CitaId}, MontoTotal: {MontoTotal}, TotalPagado: {TotalPagado}, MontoPendiente: {MontoPendiente}",
                dto.CitaId, pagoAnticipo.MontoTotal.Value, totalPagado, montoPendiente);

            // 3. Crear el DTO para PayPal con los datos correctos
            var createDto = new CreatePagoPayPalDto
            {
                CitaId = dto.CitaId,
                UsuarioId = pagoAnticipo.UsuarioId, // Usar el usuario del pago de anticipo
                Monto = montoPendiente,
                MontoTotal = pagoAnticipo.MontoTotal.Value,
                Concepto = $"Pago restante de cita - ${montoPendiente:F2} MXN",
                EsAnticipo = false,
                ReturnUrl = dto.ReturnUrl,
                CancelUrl = dto.CancelUrl
            };

            var order = await _pagoService.CreatePayPalOrderAsync(createDto, userId);

            _logger.LogInformation(
                "CompletarPagoRestantePayPal - Orden creada exitosamente. CitaId: {CitaId}, OrderId: {OrderId}, Monto: {Monto}",
                dto.CitaId, order.OrderId, montoPendiente);

            return Ok(ApiResponse<PayPalOrderResponseDto>.SuccessResponse(order, 
                $"Orden de PayPal creada para completar pago de ${montoPendiente:F2} MXN"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "CompletarPagoRestantePayPal - Cita no encontrada: CitaId={CitaId}", dto?.CitaId);
            return NotFound(ApiResponse<PayPalOrderResponseDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "CompletarPagoRestantePayPal - Operación inválida: CitaId={CitaId}", dto?.CitaId);
            return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CompletarPagoRestantePayPal - Error inesperado: CitaId={CitaId}, Message={Message}, StackTrace={StackTrace}", 
                dto?.CitaId, ex.Message, ex.StackTrace);
            return StatusCode(500, ApiResponse<PayPalOrderResponseDto>.ErrorResponse($"Error al crear orden de pago: {ex.Message}"));
        }
    }

    /// <summary>
    /// Crea una orden de PayPal para pagar el 100% de una cita presencial (sin anticipo previo)
    /// </summary>
    [HttpPost("crear-pago-completo/paypal")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PayPalOrderResponseDto>>> CrearPagoCompletoPayPal([FromBody] CrearPagoCompletoPayPalDto dto)
    {
        try
        {
            _logger.LogInformation(
                "CrearPagoCompletoPayPal - INICIO. CitaId: {CitaId}, Monto: {Monto}, DTO recibido: {@Dto}",
                dto?.CitaId, dto?.Monto, dto);

            if (dto == null)
            {
                _logger.LogError("CrearPagoCompletoPayPal - DTO es NULL");
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("No se recibieron datos en la solicitud"));
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogError("CrearPagoCompletoPayPal - Datos inválidos. Errores: {Errors}", string.Join(", ", errors));
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();

            // Verificar si ya existe algún pago completado para esta cita
            var pagosExistentes = await _pagoService.GetPagosByCitaIdAsync(dto.CitaId);
            var pagosCompletados = pagosExistentes?.Where(p => p.Estado == 3).ToList() ?? new List<PagoDto>();

            if (pagosCompletados.Any())
            {
                var totalPagado = pagosCompletados.Sum(p => p.Monto);
                
                _logger.LogWarning(
                    "CrearPagoCompletoPayPal - La cita ya tiene pagos registrados: CitaId={CitaId}, TotalPagado={TotalPagado}",
                    dto.CitaId, totalPagado);
                
                return BadRequest(ApiResponse<PayPalOrderResponseDto>.ErrorResponse(
                    $"Esta cita ya tiene pagos registrados por un total de ${totalPagado:F2} MXN. " +
                    "Use /completar-pago/paypal para pagar el saldo restante."));
            }

            // Crear el DTO para PayPal
            var createDto = new CreatePagoPayPalDto
            {
                CitaId = dto.CitaId,
                UsuarioId = dto.UsuarioId ?? userId,
                Monto = dto.Monto,
                MontoTotal = dto.Monto,
                Concepto = dto.Concepto ?? $"Pago completo de cita - ${dto.Monto:F2} MXN",
                EsAnticipo = false,
                ReturnUrl = dto.ReturnUrl,
                CancelUrl = dto.CancelUrl
            };

            _logger.LogInformation(
                "CrearPagoCompletoPayPal - Llamando CreatePayPalOrderAsync con: {@CreateDto}",
                createDto);

            var order = await _pagoService.CreatePayPalOrderAsync(createDto, userId);

            _logger.LogInformation(
                "CrearPagoCompletoPayPal - Orden creada exitosamente. CitaId: {CitaId}, OrderId: {OrderId}, Monto: {Monto}",
                dto.CitaId, order.OrderId, dto.Monto);

            return Ok(ApiResponse<PayPalOrderResponseDto>.SuccessResponse(order, 
                $"Orden de PayPal creada para pago completo de ${dto.Monto:F2} MXN"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CrearPagoCompletoPayPal - Error al crear orden: CitaId={CitaId}, Message={Message}, StackTrace={StackTrace}", 
                dto?.CitaId, ex.Message, ex.StackTrace);
            return StatusCode(500, ApiResponse<PayPalOrderResponseDto>.ErrorResponse($"Error al crear orden de pago: {ex.Message}"));
        }
    }

    [HttpPut("{id}/cancelar")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagoDto>>> CancelarPago(Guid id, [FromBody] string? motivo)
    {
        try
        {
            var userId = GetUserId();
            var pago = await _pagoService.CancelarPagoAsync(id, userId, motivo);
            return Ok(ApiResponse<PagoDto>.SuccessResponse(pago, "Pago cancelado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar pago {PagoId}", id);
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
            _logger.LogError(ex, "Error al procesar webhook PayPal");
            return BadRequest(new { error = ex.Message });
        }
    }
}
