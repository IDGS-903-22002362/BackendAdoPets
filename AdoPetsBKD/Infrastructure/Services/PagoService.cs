using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Infrastructure.Data;

namespace AdoPetsBKD.Infrastructure.Services;

public class PagoService : IPagoService
{
    private readonly AdoPetsDbContext _context;
    private readonly IPayPalClient _paypalClient;
    private readonly ILogger<PagoService> _logger;

    public PagoService(
        AdoPetsDbContext context, 
        IPayPalClient paypalClient,
        ILogger<PagoService> logger)
    {
        _context = context;
        _paypalClient = paypalClient;
        _logger = logger;
    }

    public async Task<PagoDto> CreatePagoAsync(CreatePagoDto dto, Guid createdBy)
    {
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = dto.UsuarioId,
            Monto = dto.Monto,
            Moneda = dto.Moneda,
            Tipo = (TipoPago)dto.Tipo,
            Metodo = (MetodoPago)dto.Metodo,
            Concepto = dto.Concepto,
            Referencia = dto.Referencia,
            CitaId = dto.CitaId,
            TicketId = dto.TicketId,
            EsAnticipo = dto.EsAnticipo,
            MontoTotal = dto.MontoTotal,
            CreatedBy = createdBy
        };

        pago.NumeroPago = pago.GenerarNumeroPago();

        if (pago.EsAnticipo && pago.MontoTotal.HasValue)
        {
            pago.MontoRestante = pago.MontoTotal.Value - pago.Monto;
        }

        // Si es pago en efectivo o manual, marcar como completado
        if (pago.Metodo == MetodoPago.Efectivo)
        {
            pago.Estado = EstadoPago.Completado;
            pago.FechaPago = DateTime.UtcNow;
            pago.FechaConfirmacion = DateTime.UtcNow;
        }

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al crear pago");
    }

    public async Task<PayPalOrderResponseDto> CreatePayPalOrderAsync(CreatePagoPayPalDto dto, Guid createdBy)
    {
        // Crear el pago en estado pendiente
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = dto.UsuarioId,
            Monto = dto.Monto,
            Moneda = "MXN",
            Tipo = dto.EsAnticipo ? TipoPago.Anticipo : TipoPago.PagoCompleto,
            Metodo = MetodoPago.PayPal,
            Concepto = dto.Concepto,
            CitaId = dto.CitaId,
            EsAnticipo = dto.EsAnticipo,
            MontoTotal = dto.MontoTotal,
            Estado = EstadoPago.Pendiente,
            CreatedBy = createdBy
        };

        pago.NumeroPago = pago.GenerarNumeroPago();

        if (pago.EsAnticipo && pago.MontoTotal.HasValue)
        {
            pago.MontoRestante = pago.MontoTotal.Value - pago.Monto;
        }

        try
        {
            // Crear orden en PayPal usando el SDK moderno
            var order = await _paypalClient.CreateOrderAsync(
                dto.Monto,
                "MXN",
                dto.Concepto,
                dto.ReturnUrl,
                dto.CancelUrl
            );

            pago.PayPalOrderId = order.Id;

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            // Si hay solicitud de cita, vincular el pago
            if (dto.SolicitudCitaId.HasValue)
            {
                var solicitud = await _context.SolicitudesCitasDigitales.FindAsync(dto.SolicitudCitaId.Value);
                if (solicitud != null)
                {
                    solicitud.MarcarPagoRecibido(pago.Id);
                    await _context.SaveChangesAsync();
                }
            }

            // Obtener el approval URL
            var approvalUrl = order.Links?.FirstOrDefault(l => l.Rel == "approve")?.Href ?? string.Empty;

            _logger.LogInformation("Orden de PayPal creada exitosamente: {OrderId} para pago {PagoId}", order.Id, pago.Id);

            return new PayPalOrderResponseDto
            {
                OrderId = order.Id,
                ApprovalUrl = approvalUrl,
                Status = order.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden de PayPal para pago {PagoId}", pago.Id);
            throw new Exception($"Error al crear orden de PayPal: {ex.Message}", ex);
        }
    }

    public async Task<PagoDto> CapturePayPalPaymentAsync(string orderId)
    {
        var pago = await _context.Pagos
            .FirstOrDefaultAsync(p => p.PayPalOrderId == orderId)
            ?? throw new Exception("Pago no encontrado");

        try
        {
            // Capturar la orden usando el SDK moderno
            var order = await _paypalClient.CaptureOrderAsync(orderId);

            // Verificar que el pago fue exitoso
            if (order.Status?.ToUpper() == "COMPLETED")
            {
                // Obtener información del capture
                var capture = order.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault();
                var payer = order.Payer;

                pago.MarcarComoPagado(
                    capture?.Id ?? orderId,
                    payer?.Email,
                    payer?.Name?.GivenName + " " + payer?.Name?.Surname
                );

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pago de PayPal capturado exitosamente: {OrderId}", orderId);
            }
            else
            {
                throw new Exception($"El pago no fue completado. Estado: {order.Status}");
            }

            return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al capturar pago");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al capturar pago de PayPal {OrderId}", orderId);
            
            // Marcar el pago como fallido
            pago.Estado = EstadoPago.Fallido;
            await _context.SaveChangesAsync();
            
            throw new Exception($"Error al capturar pago de PayPal: {ex.Message}", ex);
        }
    }

    public async Task<PagoDto?> GetPagoByIdAsync(Guid id)
    {
        return await _context.Pagos
            .Include(p => p.Usuario)
            .Where(p => p.Id == id)
            .Select(p => new PagoDto
            {
                Id = p.Id,
                NumeroPago = p.NumeroPago,
                UsuarioId = p.UsuarioId,
                NombreUsuario = p.Usuario != null ? p.Usuario.NombreCompleto : null,
                Monto = p.Monto,
                Moneda = p.Moneda,
                Tipo = (int)p.Tipo,
                TipoNombre = p.Tipo.ToString(),
                Metodo = (int)p.Metodo,
                MetodoNombre = p.Metodo.ToString(),
                Estado = (int)p.Estado,
                EstadoNombre = p.Estado.ToString(),
                PayPalOrderId = p.PayPalOrderId,
                PayPalCaptureId = p.PayPalCaptureId,
                PayPalPayerEmail = p.PayPalPayerEmail,
                PayPalPayerName = p.PayPalPayerName,
                FechaPago = p.FechaPago,
                FechaConfirmacion = p.FechaConfirmacion,
                Concepto = p.Concepto,
                Referencia = p.Referencia,
                CitaId = p.CitaId,
                TicketId = p.TicketId,
                EsAnticipo = p.EsAnticipo,
                MontoTotal = p.MontoTotal,
                MontoRestante = p.MontoRestante,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PagoDto?> GetPagoByPayPalOrderIdAsync(string paypalOrderId)
    {
        return await _context.Pagos
            .Include(p => p.Usuario)
            .Where(p => p.PayPalOrderId == paypalOrderId)
            .Select(p => new PagoDto
            {
                Id = p.Id,
                NumeroPago = p.NumeroPago,
                UsuarioId = p.UsuarioId,
                NombreUsuario = p.Usuario != null ? p.Usuario.NombreCompleto : null,
                Monto = p.Monto,
                Moneda = p.Moneda,
                Tipo = (int)p.Tipo,
                TipoNombre = p.Tipo.ToString(),
                Metodo = (int)p.Metodo,
                MetodoNombre = p.Metodo.ToString(),
                Estado = (int)p.Estado,
                EstadoNombre = p.Estado.ToString(),
                PayPalOrderId = p.PayPalOrderId,
                PayPalCaptureId = p.PayPalCaptureId,
                PayPalPayerEmail = p.PayPalPayerEmail,
                PayPalPayerName = p.PayPalPayerName,
                FechaPago = p.FechaPago,
                FechaConfirmacion = p.FechaConfirmacion,
                Concepto = p.Concepto,
                Referencia = p.Referencia,
                CitaId = p.CitaId,
                TicketId = p.TicketId,
                EsAnticipo = p.EsAnticipo,
                MontoTotal = p.MontoTotal,
                MontoRestante = p.MontoRestante,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<PagoDto>> GetPagosByUsuarioAsync(Guid usuarioId)
    {
        return await _context.Pagos
            .Include(p => p.Usuario)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PagoDto
            {
                Id = p.Id,
                NumeroPago = p.NumeroPago,
                Monto = p.Monto,
                Moneda = p.Moneda,
                Tipo = (int)p.Tipo,
                TipoNombre = p.Tipo.ToString(),
                Metodo = (int)p.Metodo,
                MetodoNombre = p.Metodo.ToString(),
                Estado = (int)p.Estado,
                EstadoNombre = p.Estado.ToString(),
                FechaPago = p.FechaPago,
                Concepto = p.Concepto,
                EsAnticipo = p.EsAnticipo,
                MontoTotal = p.MontoTotal,
                MontoRestante = p.MontoRestante,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PagoDto> CancelarPagoAsync(Guid pagoId, Guid canceladoPorId, string? motivo = null)
    {
        var pago = await _context.Pagos.FindAsync(pagoId)
            ?? throw new Exception("Pago no encontrado");

        pago.Cancelar(motivo);
        pago.UpdatedBy = canceladoPorId;
        pago.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetPagoByIdAsync(pagoId) ?? throw new Exception("Error al cancelar pago");
    }

    public async Task ProcessWebhookAsync(PayPalWebhookDto webhook)
    {
        // Guardar el webhook para auditoría
        var webhookEvent = new Domain.Entities.Donaciones.WebhookEvent
        {
            Id = Guid.NewGuid(),
            Provider = Domain.Entities.Donaciones.ProviderWebhook.PayPal,
            EventId = webhook.EventId,
            Tipo = webhook.EventType,
            PayloadJson = JsonSerializer.Serialize(webhook.Resource),
            ReceivedAt = DateTime.UtcNow,
            Status = Domain.Entities.Donaciones.StatusWebhook.Pending
        };

        _context.WebhookEvents.Add(webhookEvent);

        try
        {
            // Procesar según el tipo de evento
            switch (webhook.EventType)
            {
                case "CHECKOUT.ORDER.COMPLETED":
                case "PAYMENT.CAPTURE.COMPLETED":
                    await HandlePaymentCaptureCompleted(webhook);
                    break;
                case "PAYMENT.CAPTURE.DENIED":
                    await HandlePaymentCaptureDenied(webhook);
                    break;
                // Agregar más casos según sea necesario
            }

            webhookEvent.MarcarComoProcesado();
        }
        catch (Exception ex)
        {
            webhookEvent.MarcarComoFallido(ex.Message);
        }

        await _context.SaveChangesAsync();
    }

    private async Task HandlePaymentCaptureCompleted(PayPalWebhookDto webhook)
    {
        try
        {
            // Parsear el resource del webhook
            var resourceJson = JsonSerializer.Serialize(webhook.Resource);
            var resource = JsonSerializer.Deserialize<Dictionary<string, object>>(resourceJson);

            if (resource != null)
            {
                // Intentar obtener el orderId de diferentes formas según el tipo de evento
                string? orderId = null;
                
                if (resource.TryGetValue("id", out var idObj))
                {
                    orderId = idObj?.ToString();
                }
                
                if (string.IsNullOrEmpty(orderId) && resource.TryGetValue("supplementary_data", out var suppData))
                {
                    var suppJson = JsonSerializer.Serialize(suppData);
                    var suppDict = JsonSerializer.Deserialize<Dictionary<string, object>>(suppJson);
                    if (suppDict?.TryGetValue("related_ids", out var relatedIds) == true)
                    {
                        var relatedJson = JsonSerializer.Serialize(relatedIds);
                        var relatedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(relatedJson);
                        if (relatedDict?.TryGetValue("order_id", out var orderIdObj) == true)
                        {
                            orderId = orderIdObj?.ToString();
                        }
                    }
                }
                
                if (!string.IsNullOrEmpty(orderId))
                {
                    var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.PayPalOrderId == orderId);
                    
                    if (pago != null && pago.Estado == EstadoPago.Pendiente)
                    {
                        var captureId = resource.TryGetValue("id", out var capIdObj) ? capIdObj?.ToString() : null;
                        var payerEmail = resource.TryGetValue("email_address", out var emailObj) ? emailObj?.ToString() : null;
                        
                        pago.MarcarComoPagado(captureId ?? orderId, payerEmail, null);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("Webhook procesado: Pago {PagoId} marcado como completado", pago.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar webhook de pago completado");
            throw;
        }
    }

    private async Task HandlePaymentCaptureDenied(PayPalWebhookDto webhook)
    {
        try
        {
            var resourceJson = JsonSerializer.Serialize(webhook.Resource);
            var resource = JsonSerializer.Deserialize<Dictionary<string, object>>(resourceJson);

            if (resource != null && resource.TryGetValue("id", out var orderIdObj))
            {
                var orderId = orderIdObj?.ToString();
                
                if (!string.IsNullOrEmpty(orderId))
                {
                    var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.PayPalOrderId == orderId);
                    
                    if (pago != null)
                    {
                        pago.Estado = EstadoPago.Fallido;
                        await _context.SaveChangesAsync();
                        
                        _logger.LogWarning("Webhook procesado: Pago {PagoId} marcado como fallido", pago.Id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar webhook de pago denegado");
            throw;
        }
    }
}
