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
    // TODO: Inyectar cliente de PayPal cuando se configure
    // private readonly IPayPalClient _paypalClient;

    public PagoService(AdoPetsDbContext context)
    {
        _context = context;
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

        // TODO: Integrar con PayPal API
        // Por ahora simulamos la creación de la orden
        var orderId = $"PAYPAL-ORDER-{Guid.NewGuid()}";
        pago.PayPalOrderId = orderId;

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

        return new PayPalOrderResponseDto
        {
            OrderId = orderId,
            ApprovalUrl = $"https://www.paypal.com/checkoutnow?token={orderId}",
            Status = "CREATED"
        };

        /* TODO: Implementación real con PayPal SDK
        var request = new OrdersCreateRequest();
        request.Prefer("return=representation");
        request.RequestBody(new OrderRequest()
        {
            CheckoutPaymentIntent = "CAPTURE",
            PurchaseUnits = new List<PurchaseUnitRequest>()
            {
                new PurchaseUnitRequest()
                {
                    AmountWithBreakdown = new AmountWithBreakdown()
                    {
                        CurrencyCode = "MXN",
                        Value = dto.Monto.ToString("F2")
                    },
                    Description = dto.Concepto
                }
            },
            ApplicationContext = new ApplicationContext()
            {
                ReturnUrl = dto.ReturnUrl,
                CancelUrl = dto.CancelUrl
            }
        });

        var response = await _paypalClient.Execute(request);
        var result = response.Result<Order>();
        
        pago.PayPalOrderId = result.Id;
        await _context.SaveChangesAsync();

        var approvalUrl = result.Links.FirstOrDefault(l => l.Rel == "approve")?.Href;

        return new PayPalOrderResponseDto
        {
            OrderId = result.Id,
            ApprovalUrl = approvalUrl ?? string.Empty,
            Status = result.Status
        };
        */
    }

    public async Task<PagoDto> CapturePayPalPaymentAsync(string orderId)
    {
        var pago = await _context.Pagos
            .FirstOrDefaultAsync(p => p.PayPalOrderId == orderId)
            ?? throw new Exception("Pago no encontrado");

        // TODO: Implementar captura real con PayPal SDK
        var captureId = $"CAPTURE-{Guid.NewGuid()}";
        pago.MarcarComoPagado(captureId, "cliente@example.com", "Cliente Test");

        await _context.SaveChangesAsync();

        return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al capturar pago");

        /* TODO: Implementación real con PayPal SDK
        var request = new OrdersCaptureRequest(orderId);
        request.RequestBody(new OrderActionRequest());
        
        var response = await _paypalClient.Execute(request);
        var result = response.Result<Order>();
        
        var capture = result.PurchaseUnits[0].Payments.Captures[0];
        
        pago.MarcarComoPagado(
            capture.Id,
            result.Payer?.Email,
            result.Payer?.Name?.GivenName + " " + result.Payer?.Name?.Surname
        );

        await _context.SaveChangesAsync();

        return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al capturar pago");
        */
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
        // TODO: Extraer información del resource y actualizar el pago
        // var resource = JsonSerializer.Deserialize<PayPalCapture>(webhook.Resource.ToString());
        // var pago = await _context.Pagos.FirstOrDefaultAsync(p => p.PayPalOrderId == resource.OrderId);
        // if (pago != null)
        // {
        //     pago.MarcarComoPagado(resource.Id, resource.PayerEmail, resource.PayerName);
        //     await _context.SaveChangesAsync();
        // }
        await Task.CompletedTask;
    }

    private async Task HandlePaymentCaptureDenied(PayPalWebhookDto webhook)
    {
        // TODO: Manejar pago denegado
        await Task.CompletedTask;
    }
}
