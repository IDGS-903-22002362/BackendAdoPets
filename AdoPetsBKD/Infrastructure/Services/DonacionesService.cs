using Microsoft.EntityFrameworkCore;
using System.Text.Json; 
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Infrastructure.Data;
using AdoPetsBKD.Application.DTOs.Donaciones;
using AdoPetsBKD.Domain.Entities.Donaciones;

namespace AdoPetsBKD.Infrastructure.Services
{
    public class DonacionesService : IDonacionesService
    {
        private readonly AdoPetsDbContext _context;
        private readonly ILogger<DonacionesService> _logger;
        private readonly IPayPalClient _paypalClient;

        public DonacionesService(
            AdoPetsDbContext context, 
            IPayPalClient paypalClient, 
            ILogger<DonacionesService> logger)
        {
            _context = context;
            _paypalClient = paypalClient;
            _logger = logger;
        }

        // Crear una donación simple (sin PayPal)
        public async Task<DonacionDto> CreateDonacionAsync(CreateDonacionDto dto, Guid userId)
        {
            var donacion = new Donacion
            {
                Id = Guid.NewGuid(),
                UsuarioId = dto.Anonima ? null : dto.UsuarioId, 
                Monto = dto.Monto,
                Moneda = dto.Moneda,
                Status = (StatusDonacion)dto.Status,
                Source = (SourceDonacion)dto.Source,
                Mensaje = dto.Mensaje,
                Anonima = dto.Anonima,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Donaciones.Add(donacion);
            await _context.SaveChangesAsync();
            
            return await GetDonacionByIdAsync(donacion.Id) ?? throw new Exception("Error al crear donación");
        }

        // Crear orden de PayPal para donación
        public async Task<PayPalDonacionResponseDto> CreatePayPayDonacionAsync(CreateDonacionDto dto, Guid createdBy)
        {
            // Validar DTO para PayPal
            if (string.IsNullOrEmpty(dto.Moneda))
            {
                dto.Moneda = "MXN";
            }

            // Crear la donación en estado PENDING
            var donacion = new Donacion
            {
                Id = Guid.NewGuid(),
                UsuarioId = dto.Anonima ? null : dto.UsuarioId,
                Monto = dto.Monto,
                Moneda = dto.Moneda,
                Status = StatusDonacion.PENDING,
                Source = SourceDonacion.Checkout,
                Mensaje = dto.Mensaje,
                Anonima = dto.Anonima,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var concepto = dto.Anonima 
                    ? $"Donación anónima de ${dto.Monto:F2} para AdoPets" 
                    : $"Donación de ${dto.Monto:F2} para AdoPets";

                var returnUrl = "https://adopets.com/donacion/success";
                var cancelUrl = "https://adopets.com/donacion/cancel";

                var order = await _paypalClient.CreateOrderAsync(
                    donacion.Monto,
                    donacion.Moneda,
                    concepto,
                    returnUrl,
                    cancelUrl
                );

                donacion.PaypalOrderId = order.Id;

                _context.Donaciones.Add(donacion);
                await _context.SaveChangesAsync();

                var approvalUrl = string.Empty;
                if (order.Links != null && order.Links.Count > 0)
                {
                    var approveLink = order.Links.FirstOrDefault(l => 
                        l.Rel != null && l.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase));
                    
                    if (approveLink != null)
                    {
                        approvalUrl = approveLink.Href ?? string.Empty;
                    }
                }

                _logger.LogInformation(
                    "Orden de PayPal creada exitosamente: {OrderId} para donación {DonacionId}. ApprovalUrl: {ApprovalUrl}", 
                    order.Id, 
                    donacion.Id,
                    approvalUrl
                );

                return new PayPalDonacionResponseDto
                {
                    DonacionId = donacion.Id,
                    OrderId = order.Id,
                    ApprovalUrl = approvalUrl,
                    Status = order.Status ?? "CREATED"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de PayPal para donación {DonacionId}", donacion.Id);
                throw new Exception($"Error al crear orden de PayPal: {ex.Message}", ex);
            }
        }

        // Capturar pago de PayPal para donación
        public async Task<DonacionDto> CapturePayPalDonacionAsync(string orderId)
        {
            var donacion = await _context.Donaciones
                .FirstOrDefaultAsync(d => d.PaypalOrderId == orderId)
                ?? throw new Exception("Donación no encontrada");

            try
            {
                var order = await _paypalClient.CaptureOrderAsync(orderId);

                // Verificar que el pago fue exitoso
                if (order.Status?.ToUpper() == "COMPLETED")
                {
                    var capture = order.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault();
                    var payer = order.Payer;

                    donacion.Capturar(
                        capture?.Id ?? orderId,
                        payer?.Email,
                        payer?.Name?.GivenName + " " + payer?.Name?.Surname
                    );

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Donación de PayPal capturada exitosamente: {OrderId}", orderId);
                }
                else
                {
                    throw new Exception($"La donación no fue completada. Estado: {order.Status}");
                }

                return await GetDonacionByIdAsync(donacion.Id) ?? throw new Exception("Error al capturar donación");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al capturar donación de PayPal {OrderId}", orderId);
                
                // Marcar la donación como fallida
                donacion.Fallar($"Error al capturar: {ex.Message}");
                await _context.SaveChangesAsync();
                
                throw new Exception($"Error al capturar donación de PayPal: {ex.Message}", ex);
            }
        }

        // Obtener donación por ID
        public async Task<DonacionDto?> GetDonacionByIdAsync(Guid id)
        {
            return await _context.Donaciones
                .Include(d => d.Usuario)
                .Where(d => d.Id == id)
                .Select(d => new DonacionDto
                {
                    Id = d.Id,
                    UsuarioId = d.UsuarioId,
                    NombreUsuario = d.Usuario != null ? d.Usuario.NombreCompleto : null,
                    Monto = d.Monto,
                    Moneda = d.Moneda,
                    Status = (int)d.Status,
                    StatusNombre = d.Status.ToString(),
                    Source = (int)d.Source,
                    SourceNombre = d.Source.ToString(),
                    Mensaje = d.Mensaje,
                    Anonima = d.Anonima,
                    PayPalOrderId = d.PaypalOrderId,
                    PayPalCaptureId = d.PaypalCaptureId,
                    PayPalPayerEmail = d.PayerEmail,
                    PayPalPayerName = d.PayerName,
                    CapturedAt = d.CapturedAt,
                    CancelledAt = d.CancelledAt,
                    CancellationReason = d.CancellationReason,
                    CreatedAt = d.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        // Obtener donación por PayPal Order ID
        public async Task<DonacionDto?> GetDonacionByPayPalOrderIdAsync(string paypalOrderId)
        {
            return await _context.Donaciones
                .Include(d => d.Usuario)
                .Where(d => d.PaypalOrderId == paypalOrderId)
                .Select(d => new DonacionDto
                {
                    Id = d.Id,
                    UsuarioId = d.UsuarioId,
                    NombreUsuario = d.Usuario != null ? d.Usuario.NombreCompleto : null,
                    Monto = d.Monto,
                    Moneda = d.Moneda,
                    Status = (int)d.Status,
                    StatusNombre = d.Status.ToString(),
                    Source = (int)d.Source,
                    SourceNombre = d.Source.ToString(),
                    Mensaje = d.Mensaje,
                    Anonima = d.Anonima,
                    PayPalOrderId = d.PaypalOrderId,
                    PayPalCaptureId = d.PaypalCaptureId,
                    PayPalPayerEmail = d.PayerEmail,
                    PayPalPayerName = d.PayerName,
                    CapturedAt = d.CapturedAt,
                    CancelledAt = d.CancelledAt,
                    CancellationReason = d.CancellationReason,
                    CreatedAt = d.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        // Obtener donaciones de un usuario
        public async Task<List<DonacionDto>> GetDonacionesByUsuarioAsync(Guid usuarioId)
        {
            return await _context.Donaciones
                .Include(d => d.Usuario)
                .Where(d => d.UsuarioId == usuarioId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DonacionDto
                {
                    Id = d.Id,
                    UsuarioId = d.UsuarioId,
                    NombreUsuario = d.Usuario != null ? d.Usuario.NombreCompleto : null,
                    Monto = d.Monto,
                    Moneda = d.Moneda,
                    Status = (int)d.Status,
                    StatusNombre = d.Status.ToString(),
                    Source = (int)d.Source,
                    SourceNombre = d.Source.ToString(),
                    Mensaje = d.Mensaje,
                    Anonima = d.Anonima,
                    PayPalOrderId = d.PaypalOrderId,
                    PayPalCaptureId = d.PaypalCaptureId,
                    PayPalPayerEmail = d.PayerEmail,
                    PayPalPayerName = d.PayerName,
                    CapturedAt = d.CapturedAt,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        // Obtener donaciones (paginadas)
        public async Task<List<DonacionDto>> GetDonacionesAsync(int pageNumber = 1, int pageSize = 10, FiltroDonacionAnonima filtro = FiltroDonacionAnonima.SoloPublicas)
        {
            var query = _context.Donaciones
                .Include(d => d.Usuario)
                .Where(d => d.Status == StatusDonacion.CAPTURED); // Solo donaciones completadas

            // Aplicar filtro según opción
            query = filtro switch
            {
                FiltroDonacionAnonima.SoloPublicas => query.Where(d => !d.Anonima),
                FiltroDonacionAnonima.SoloAnonimas => query.Where(d => d.Anonima),
                FiltroDonacionAnonima.Todas => query,
                _ => query.Where(d => !d.Anonima) // Default: solo públicas
            };

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DonacionDto
                {
                    Id = d.Id,
                    UsuarioId = d.UsuarioId,
                    NombreUsuario = d.Anonima ? "Anónimo" : (d.Usuario != null ? d.Usuario.NombreCompleto : "Usuario eliminado"),
                    Monto = d.Monto,
                    Moneda = d.Moneda,
                    Status = (int)d.Status,
                    StatusNombre = d.Status.ToString(),
                    Source = (int)d.Source,
                    SourceNombre = d.Source.ToString(),
                    Mensaje = d.Mensaje,
                    Anonima = d.Anonima,
                    CapturedAt = d.CapturedAt,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        // Cancelar donación
        public async Task<DonacionDto> CancelarDonacionAsync(Guid donacionId, Guid canceladoPorId, string? motivo = null)
        {
            var donacion = await _context.Donaciones.FindAsync(donacionId)
                ?? throw new Exception("Donación no encontrada");

            donacion.Cancelar(motivo);
            donacion.UpdatedBy = canceladoPorId;
            donacion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetDonacionByIdAsync(donacionId) ?? throw new Exception("Error al cancelar donación");
        }

        // Procesar webhook de PayPal
        public async Task ProcessWebhookAsync(PayPalWebhookDonacionDto webhook)
        {
            // Guardar el webhook para auditoría
            var webhookEvent = new WebhookEvent
            {
                Id = Guid.NewGuid(),
                Provider = ProviderWebhook.PayPal,
                EventId = webhook.EventId,
                Tipo = webhook.EventType,
                PayloadJson = JsonSerializer.Serialize(webhook.Resource),
                ReceivedAt = DateTime.UtcNow,
                Status = StatusWebhook.Pending
            };

            _context.WebhookEvents.Add(webhookEvent);

            try
            {
                switch (webhook.EventType)
                {
                    case "CHECKOUT.ORDER.COMPLETED":
                    case "PAYMENT.CAPTURE.COMPLETED":
                        await HandleDonacionCaptureCompleted(webhook);
                        break;
                    case "PAYMENT.CAPTURE.DENIED":
                        await HandleDonacionCaptureDenied(webhook);
                        break;
                }

                webhookEvent.MarcarComoProcesado();
            }
            catch (Exception ex)
            {
                webhookEvent.MarcarComoFallido(ex.Message);
                _logger.LogError(ex, "Error al procesar webhook de donación: {EventId}", webhook.EventId);
            }

            await _context.SaveChangesAsync();
        }

        private async Task HandleDonacionCaptureCompleted(PayPalWebhookDonacionDto webhook)
        {
            try
            {
                var resourceJson = JsonSerializer.Serialize(webhook.Resource);
                var resource = JsonSerializer.Deserialize<Dictionary<string, object>>(resourceJson);

                if (resource != null)
                {
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
                        var donacion = await _context.Donaciones.FirstOrDefaultAsync(d => d.PaypalOrderId == orderId);
                        
                        if (donacion != null && donacion.Status == StatusDonacion.PENDING)
                        {
                            var captureId = resource.TryGetValue("id", out var capIdObj) ? capIdObj?.ToString() : null;
                            var payerEmail = resource.TryGetValue("email_address", out var emailObj) ? emailObj?.ToString() : null;
                            
                            donacion.Capturar(captureId ?? orderId, payerEmail, null);
                            await _context.SaveChangesAsync();
                            
                            _logger.LogInformation("Webhook procesado: Donación {DonacionId} marcada como capturada", donacion.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar webhook de donación completada");
                throw;
            }
        }

        private async Task HandleDonacionCaptureDenied(PayPalWebhookDonacionDto webhook)
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
                        var donacion = await _context.Donaciones.FirstOrDefaultAsync(d => d.PaypalOrderId == orderId);
                        
                        if (donacion != null)
                        {
                            donacion.Fallar("Pago denegado por PayPal");
                            await _context.SaveChangesAsync();
                            
                            _logger.LogWarning("Webhook procesado: Donación {DonacionId} marcada como fallida", donacion.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar webhook de donación denegada");
                throw;
            }
        }
    }
}
