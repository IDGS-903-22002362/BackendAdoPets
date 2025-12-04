using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using PayPalCheckoutSdk.Orders;
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

        // Establecer MontoRestante según el tipo de pago
        if (pago.MontoTotal.HasValue)
        {
            if (pago.EsAnticipo)
            {
                // Si es anticipo, calcular lo que queda por pagar
                pago.MontoRestante = pago.MontoTotal.Value - pago.Monto;
            }
            else
            {
                // Si es pago completo (100%), MontoRestante debe ser 0
                pago.MontoRestante = 0;
            }
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
        _logger.LogInformation(
            "CreatePayPalOrderAsync - CitaId: {CitaId}, UsuarioId: {UsuarioId}, Monto: {Monto}, EsAnticipo: {EsAnticipo}, MontoTotal: {MontoTotal}",
            dto.CitaId, dto.UsuarioId, dto.Monto, dto.EsAnticipo, dto.MontoTotal);

        // Validaciones
        if (dto.Monto <= 0)
        {
            _logger.LogWarning("CreatePayPalOrderAsync - Monto inválido: {Monto}", dto.Monto);
            throw new Exception("El monto debe ser mayor a 0");
        }

        if (!dto.MontoTotal.HasValue || dto.MontoTotal.Value <= 0)
        {
            _logger.LogWarning("CreatePayPalOrderAsync - MontoTotal inválido: {MontoTotal}", dto.MontoTotal);
            throw new Exception("El monto total debe ser mayor a 0");
        }

        if (dto.Monto > dto.MontoTotal.Value)
        {
            _logger.LogWarning(
                "CreatePayPalOrderAsync - Monto ({Monto}) mayor que MontoTotal ({MontoTotal})",
                dto.Monto, dto.MontoTotal.Value);
            throw new Exception("El monto a pagar no puede ser mayor al monto total");
        }

        // Crear el pago en estado pendiente
        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = dto.UsuarioId,
            Monto = dto.Monto,
            Moneda = "MXN",
            // Determinar el tipo de pago correctamente:
            // - Anticipo: Es anticipo (EsAnticipo = true)
            // - PagoCompleto: No es anticipo Y el monto es igual al monto total (100%)
            // - PagoComplementario: No es anticipo Y el monto es menor al monto total (pago restante)
            Tipo = dto.EsAnticipo 
                ? TipoPago.Anticipo 
                : (dto.Monto >= dto.MontoTotal.GetValueOrDefault() 
                    ? TipoPago.PagoCompleto 
                    : TipoPago.PagoComplementario),
            Metodo = MetodoPago.PayPal,
            Concepto = dto.Concepto,
            CitaId = dto.CitaId,
            EsAnticipo = dto.EsAnticipo,
            MontoTotal = dto.MontoTotal,
            Estado = EstadoPago.Pendiente,
            CreatedBy = createdBy
        };

        pago.NumeroPago = pago.GenerarNumeroPago();

        // Establecer MontoRestante según el tipo de pago
        if (pago.MontoTotal.HasValue)
        {
            if (pago.EsAnticipo || pago.Tipo == TipoPago.PagoComplementario)
            {
                // Si es anticipo o pago complementario, calcular lo que queda por pagar
                pago.MontoRestante = pago.MontoTotal.Value - pago.Monto;
            }
            else
            {
                // Si es pago completo (100%), MontoRestante debe ser 0
                pago.MontoRestante = 0;
            }
        }

        _logger.LogInformation(
            "Pago creado - PagoId: {PagoId}, Monto: {Monto}, MontoTotal: {MontoTotal}, MontoRestante: {MontoRestante}, Tipo: {Tipo}, EsAnticipo: {EsAnticipo}",
            pago.Id, pago.Monto, pago.MontoTotal, pago.MontoRestante, pago.Tipo, pago.EsAnticipo);

        try
        {
            // Crear orden en PayPal usando el SDK moderno
            _logger.LogInformation("Creando orden en PayPal - Monto: {Monto} {Moneda}, Concepto: {Concepto}",
                dto.Monto, "MXN", dto.Concepto);
            
            var order = await _paypalClient.CreateOrderAsync(
                dto.Monto,
                "MXN",
                dto.Concepto,
                dto.ReturnUrl,
                dto.CancelUrl
            );

            _logger.LogInformation("Orden de PayPal recibida del SDK - OrderId: {OrderId}, Status: {Status}", order.Id, order.Status);

            pago.PayPalOrderId = order.Id;
            
            _logger.LogInformation("Guardando pago con PayPalOrderId: {PayPalOrderId}", pago.PayPalOrderId);

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pago guardado en BD - PagoId: {PagoId}, PayPalOrderId: {PayPalOrderId}", pago.Id, pago.PayPalOrderId);

            // Si hay solicitud de cita, vincular el pago
            if (dto.SolicitudCitaId.HasValue)
            {
                var solicitud = await _context.SolicitudesCitasDigitales.FindAsync(dto.SolicitudCitaId.Value);
                if (solicitud != null)
                {
                    solicitud.MarcarPagoRecibido(pago.Id);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Solicitud {SolicitudId} vinculada con pago {PagoId}", dto.SolicitudCitaId.Value, pago.Id);
                }
            }

            // Obtener el approval URL
            var approvalUrl = order.Links?.FirstOrDefault(l => l.Rel == "approve")?.Href ?? string.Empty;

            // Extraer el token de la URL de aprobación
            string token = order.Id; // Por defecto usar el orderId
            if (!string.IsNullOrEmpty(approvalUrl))
            {
                try
                {
                    var uri = new Uri(approvalUrl);
                    var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var tokenParam = queryParams["token"];
                    if (!string.IsNullOrEmpty(tokenParam))
                    {
                        token = tokenParam;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo extraer el token de la URL de aprobación. Usando orderId como fallback.");
                }
            }

            _logger.LogInformation(
                "Orden de PayPal creada exitosamente: OrderId={OrderId}, Token={Token}, ApprovalUrl={ApprovalUrl}", 
                order.Id, token, approvalUrl);

            return new PayPalOrderResponseDto
            {
                OrderId = order.Id,
                Token = token,
                ApprovalUrl = approvalUrl,
                Status = order.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear orden de PayPal para pago {PagoId}, Message: {Message}", pago.Id, ex.Message);
            throw new Exception($"Error al crear orden de PayPal: {ex.Message}", ex);
        }
    }

    public async Task<PagoDto> CapturePayPalPaymentAsync(string orderIdOrToken)
    {
        _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - INICIO. OrderId/Token recibido: {OrderIdOrToken}", orderIdOrToken);
        
        if (string.IsNullOrWhiteSpace(orderIdOrToken))
        {
            _logger.LogError("PagoService.CapturePayPalPaymentAsync - OrderId/Token está vacío o es null");
            throw new ArgumentException("El OrderId o Token es requerido", nameof(orderIdOrToken));
        }
        
        Pago? pago = null;
        string finalOrderId = orderIdOrToken;

        // Estrategia 1: Buscar directamente con el valor recibido
        _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Buscando pago con OrderId: {OrderId}", orderIdOrToken);
        
        pago = await _context.Pagos
            .FirstOrDefaultAsync(p => p.PayPalOrderId == orderIdOrToken);

        if (pago != null)
        {
            _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago encontrado con búsqueda directa. PagoId: {PagoId}", pago.Id);
            finalOrderId = pago.PayPalOrderId!;
        }
        else
        {
            _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago no encontrado con búsqueda directa. Intentando variantes...");

            // Estrategia 2: Si tiene prefijo "EC-", removerlo y buscar
            if (orderIdOrToken.StartsWith("EC-", StringComparison.OrdinalIgnoreCase))
            {
                var cleanOrderId = orderIdOrToken.Substring(3);
                _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Removiendo prefijo EC-. Buscando con: {CleanOrderId}", cleanOrderId);
                
                pago = await _context.Pagos
                    .FirstOrDefaultAsync(p => p.PayPalOrderId == cleanOrderId);
                
                if (pago != null)
                {
                    _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago encontrado sin prefijo EC-. PagoId: {PagoId}", pago.Id);
                    finalOrderId = cleanOrderId;
                }
            }
            // Estrategia 3: Si NO tiene prefijo "EC-", agregarlo y buscar
            else
            {
                var tokenWithPrefix = $"EC-{orderIdOrToken}";
                _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Agregando prefijo EC-. Buscando con: {TokenWithPrefix}", tokenWithPrefix);
                
                pago = await _context.Pagos
                    .FirstOrDefaultAsync(p => p.PayPalOrderId == tokenWithPrefix);
                
                if (pago != null)
                {
                    _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago encontrado con prefijo EC-. PagoId: {PagoId}", pago.Id);
                    finalOrderId = tokenWithPrefix;
                }
            }
        }

        if (pago == null)
        {
            _logger.LogError("PagoService.CapturePayPalPaymentAsync - Pago no encontrado después de todas las estrategias. OrderId/Token original: {OrderIdOrToken}", orderIdOrToken);
            
            // Log adicional: listar todos los PayPalOrderIds pendientes para debug
            var pagosPendientes = await _context.Pagos
                .Where(p => p.Estado == EstadoPago.Pendiente && p.PayPalOrderId != null)
                .Select(p => new { p.Id, p.PayPalOrderId, p.Monto, p.EsAnticipo })
                .ToListAsync();
            
            _logger.LogWarning("PagoService.CapturePayPalPaymentAsync - Pagos pendientes en BD: {PagosPendientes}",
                string.Join(", ", pagosPendientes.Select(p => $"[{p.Id}:{p.PayPalOrderId} ${p.Monto} Anticipo:{p.EsAnticipo}]")));
            
            throw new KeyNotFoundException($"Pago no encontrado para OrderId/Token: {orderIdOrToken}");
        }

        // ? IDEMPOTENCIA: Si el pago YA está completado, devolverlo directamente
        if (pago.Estado == EstadoPago.Completado)
        {
            _logger.LogWarning(
                "PagoService.CapturePayPalPaymentAsync - El pago ya fue capturado previamente. " +
                "PagoId: {PagoId}, PayPalOrderId: {PayPalOrderId}, Estado: {Estado}, FechaPago: {FechaPago}. " +
                "Devolviendo pago existente (idempotencia).",
                pago.Id, pago.PayPalOrderId, pago.Estado, pago.FechaPago);
            
            return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al obtener pago");
        }

        _logger.LogInformation(
            "PagoService.CapturePayPalPaymentAsync - Pago encontrado. PagoId: {PagoId}, PayPalOrderId: {PayPalOrderId}, Monto: {Monto}, Tipo: {Tipo}, EsAnticipo: {EsAnticipo}, MontoTotal: {MontoTotal}, MontoRestante: {MontoRestante}, Estado: {Estado}",
            pago.Id, pago.PayPalOrderId, pago.Monto, pago.Tipo, pago.EsAnticipo, pago.MontoTotal, pago.MontoRestante, pago.Estado);

        try
        {
            // PASO 1: Verificar el estado actual de la orden en PayPal
            _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Verificando estado de la orden en PayPal. OrderId: {OrderId}", finalOrderId);
            
            Order? orderDetails = null;
            try
            {
                orderDetails = await _paypalClient.GetOrderDetailsAsync(finalOrderId);
                _logger.LogInformation(
                    "PagoService.CapturePayPalPaymentAsync - Estado de la orden en PayPal. OrderId: {OrderId}, Status: {Status}, Intent: {Intent}",
                    finalOrderId, orderDetails.Status, orderDetails.CheckoutPaymentIntent);
                
                // Validar que la orden esté en estado correcto para capturar
                if (orderDetails.Status?.ToUpper() == "COMPLETED")
                {
                    _logger.LogWarning("PagoService.CapturePayPalPaymentAsync - La orden ya fue completada previamente. OrderId: {OrderId}", finalOrderId);
                    
                    // Marcar el pago como completado si aún no lo está
                    if (pago.Estado != EstadoPago.Completado)
                    {
                        var captureDetails = orderDetails.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault();
                        var payerDetails = orderDetails.Payer;
                        
                        pago.MarcarComoPagado(
                            captureDetails?.Id ?? finalOrderId,
                            payerDetails?.Email,
                            payerDetails?.Name?.GivenName + " " + payerDetails?.Name?.Surname
                        );
                        
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago marcado como completado. PagoId: {PagoId}", pago.Id);
                    }
                    
                    return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al obtener pago");
                }
                else if (orderDetails.Status?.ToUpper() != "APPROVED")
                {
                    _logger.LogWarning(
                        "PagoService.CapturePayPalPaymentAsync - La orden no está en estado APPROVED. OrderId: {OrderId}, Status actual: {Status}",
                        finalOrderId, orderDetails.Status);
                    throw new InvalidOperationException($"La orden no puede ser capturada. Estado actual: {orderDetails.Status}. Debe estar en estado APPROVED.");
                }
            }
            catch (Exception exDetails)
            {
                _logger.LogWarning(exDetails, "PagoService.CapturePayPalPaymentAsync - No se pudo verificar el estado de la orden. Continuando con la captura... OrderId: {OrderId}", finalOrderId);
            }
            
            // PASO 2: Capturar la orden usando el OrderId correcto que tiene PayPal
            _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Enviando solicitud de captura a PayPal. OrderId: {FinalOrderId}", finalOrderId);
            
            var captureResponse = await _paypalClient.CaptureOrderAsync(finalOrderId);

            _logger.LogInformation(
                "PagoService.CapturePayPalPaymentAsync - Respuesta de captura recibida de PayPal. OrderId: {OrderId}, Status: {Status}, CaptureId: {CaptureId}",
                finalOrderId, captureResponse.Status, captureResponse.CaptureId ?? "NULL");

            // Verificar que el pago fue exitoso
            if (!string.IsNullOrEmpty(captureResponse.Status) && captureResponse.Status.ToUpper() == "COMPLETED")
            {
                _logger.LogInformation(
                    "PagoService.CapturePayPalPaymentAsync - Pago completado exitosamente. CaptureId: {CaptureId}, PayerEmail: {PayerEmail}",
                    captureResponse.CaptureId ?? finalOrderId, captureResponse.PayerEmail ?? "NULL");

                pago.MarcarComoPagado(
                    captureResponse.CaptureId ?? finalOrderId,
                    captureResponse.PayerEmail,
                    captureResponse.PayerName
                );

                await _context.SaveChangesAsync();

                _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago guardado como completado en BD. PagoId: {PagoId}", pago.Id);
            }
            else if (!string.IsNullOrEmpty(captureResponse.Status))
            {
                _logger.LogWarning("PagoService.CapturePayPalPaymentAsync - El pago no fue completado. OrderId: {OrderId}, Status: {Status}", finalOrderId, captureResponse.Status);
                throw new InvalidOperationException($"El pago no fue completado. Estado: {captureResponse.Status}");
            }
            else
            {
                // Si no hay status pero hay captureId, usar el capture
                if (!string.IsNullOrEmpty(captureResponse.CaptureId))
                {
                    _logger.LogInformation(
                        "PagoService.CapturePayPalPaymentAsync - No hay status pero se encontró captureId. Asumiendo pago exitoso. CaptureId: {CaptureId}",
                        captureResponse.CaptureId);
                    
                    pago.MarcarComoPagado(
                        captureResponse.CaptureId,
                        captureResponse.PayerEmail,
                        captureResponse.PayerName
                    );

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("PagoService.CapturePayPalPaymentAsync - Pago guardado como completado en BD usando captureId. PagoId: {PagoId}", pago.Id);
                }
                else
                {
                    _logger.LogError(
                        "PagoService.CapturePayPalPaymentAsync - PayPal devolvió Status NULL y no hay captureId. OrderId: {OrderId}, Response: {@Response}",
                        finalOrderId, captureResponse);
                    
                    throw new InvalidOperationException("PayPal devolvió una respuesta sin status y sin captureId. No se puede confirmar el pago.");
                }
            }

            return await GetPagoByIdAsync(pago.Id) ?? throw new Exception("Error al capturar pago");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PagoService.CapturePayPalPaymentAsync - Error al capturar pago. OrderId: {OrderId}, PagoId: {PagoId}, Message: {Message}, StackTrace: {StackTrace}",
                finalOrderId, pago.Id, ex.Message, ex.StackTrace);
            
            // Marcar el pago como fallido solo si NO es un error de duplicación
            if (!ex.Message.Contains("ya fue capturado") && !ex.Message.Contains("COMPLETED"))
            {
                pago.Estado = EstadoPago.Fallido;
                await _context.SaveChangesAsync();
            }
            
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

    public async Task<List<PagoDto>> GetPagosByCitaIdAsync(Guid citaId)
    {
        return await _context.Pagos
            .Include(p => p.Usuario)
            .Where(p => p.CitaId == citaId)
            .OrderBy(p => p.CreatedAt)
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
            .ToListAsync();
    }

    public async Task<List<PagoPendienteDto>> GetPagosPendientesAsync()
    {
        // Obtener todas las citas programadas o en proceso
        var citasProgramadas = await _context.Citas
            .Include(c => c.Propietario)
            .Include(c => c.Mascota)
            .Where(c => c.Status == StatusCita.Programada || c.Status == StatusCita.EnProceso)
            .ToListAsync();

        var resultado = new List<PagoPendienteDto>();

        foreach (var cita in citasProgramadas)
        {
            // Obtener los pagos COMPLETADOS de esta cita
            var pagos = await _context.Pagos
                .Where(p => p.CitaId == cita.Id && p.Estado == EstadoPago.Completado)
                .ToListAsync();

            var totalPagado = pagos.Sum(p => p.Monto);
            var pagoAnticipo = pagos.FirstOrDefault(p => p.EsAnticipo);
            
            // Intentar obtener el monto total desde varias fuentes (en orden de prioridad)
            decimal montoTotal = 0;
            
            // 1. Desde el pago de anticipo
            if (pagoAnticipo != null && pagoAnticipo.MontoTotal.HasValue)
            {
                montoTotal = pagoAnticipo.MontoTotal.Value;
            }
            // 2. Desde cualquier otro pago que tenga MontoTotal
            else if (pagos.Any(p => p.MontoTotal.HasValue))
            {
                montoTotal = pagos.First(p => p.MontoTotal.HasValue).MontoTotal!.Value;
            }
            // 3. Desde la solicitud de cita digital
            else
            {
                var solicitud = await _context.SolicitudesCitasDigitales
                    .Include(s => s.Servicio)
                    .Where(s => s.CitaId == cita.Id)
                    .FirstOrDefaultAsync();

                if (solicitud != null)
                {
                    // Primero intenta con el CostoEstimado de la solicitud
                    if (solicitud.CostoEstimado > 0)
                    {
                        montoTotal = solicitud.CostoEstimado;
                    }
                    // Luego intenta con el PrecioSugerido del Servicio
                    else if (solicitud.Servicio != null && solicitud.Servicio.PrecioSugerido.HasValue && solicitud.Servicio.PrecioSugerido.Value > 0)
                    {
                        montoTotal = solicitud.Servicio.PrecioSugerido.Value;
                    }
                }
                
                // 4. Si no hay solicitud digital, buscar servicio por categoría del TipoCita
                if (montoTotal == 0)
                {
                    var categoriaServicio = MapearTipoCitaACategoria(cita.Tipo);
                    
                    var servicio = await _context.Servicios
                        .Where(s => s.Categoria == categoriaServicio && s.Activo)
                        .Where(s => s.PrecioSugerido.HasValue && s.PrecioSugerido.Value > 0)
                        .OrderByDescending(s => s.Id) // Usar Id en lugar de CreatedAt
                        .FirstOrDefaultAsync();

                    if (servicio != null && servicio.PrecioSugerido.HasValue)
                    {
                        montoTotal = servicio.PrecioSugerido.Value;
                    }
                }
            }

            // Si NO se pudo obtener el monto total de ninguna fuente, saltar esta cita
            if (montoTotal == 0)
            {
                _logger.LogWarning(
                    "Cita {CitaId} (Tipo: {TipoCita}) no tiene monto total definido. " +
                    "No se puede calcular pago pendiente.", 
                    cita.Id, cita.Tipo);
                continue;
            }

            var montoPendiente = montoTotal - totalPagado;

            // ? IMPORTANTE: Incluir citas con saldo pendiente (incluso 100%)
            if (montoPendiente > 0)
            {
                var porcentajePagado = montoTotal > 0 ? (totalPagado / montoTotal) * 100 : 0;
                
                string estadoPago;
                if (pagoAnticipo != null && porcentajePagado >= 45 && porcentajePagado <= 55)
                {
                    estadoPago = "Anticipo Pagado (50%)";
                }
                else if (porcentajePagado == 0)
                {
                    estadoPago = "Pago Pendiente (100%)";
                }
                else
                {
                    estadoPago = $"Pagado ({porcentajePagado:F0}%)";
                }

                // Obtener descripción del servicio desde la solicitud digital si existe
                var solicitud = await _context.SolicitudesCitasDigitales
                    .Where(s => s.CitaId == cita.Id)
                    .FirstOrDefaultAsync();

                resultado.Add(new PagoPendienteDto
                {
                    CitaId = cita.Id,
                    FechaCita = cita.StartAt,
                    NombreMascota = cita.Mascota?.Nombre,
                    NombrePropietario = cita.Propietario?.NombreCompleto,
                    PropietarioId = cita.PropietarioId,
                    ServicioDescripcion = solicitud?.DescripcionServicio ?? cita.Tipo.ToString(),
                    
                    PagoAnticipoId = pagoAnticipo?.Id,
                    MontoAnticipoPagado = pagoAnticipo?.Monto,
                    FechaPagoAnticipo = pagoAnticipo?.FechaPago,
                    
                    MontoTotal = montoTotal,
                    MontoPendiente = montoPendiente,
                    PorcentajePagado = porcentajePagado,
                    
                    TieneAnticipoPagado = pagoAnticipo != null,
                    EstadoPago = estadoPago
                });
            }
        }

        return resultado.OrderBy(p => p.FechaCita).ToList();
    }

    /// <summary>
    /// Mapea TipoCita a CategoriaServicio para buscar el precio sugerido
    /// </summary>
    private Domain.Entities.Servicios.CategoriaServicio MapearTipoCitaACategoria(TipoCita tipoCita)
    {
        return tipoCita switch
        {
            TipoCita.Consulta => Domain.Entities.Servicios.CategoriaServicio.Consulta,
            TipoCita.Cirugia => Domain.Entities.Servicios.CategoriaServicio.Cirugia,
            TipoCita.Baño => Domain.Entities.Servicios.CategoriaServicio.Estetica,
            TipoCita.Vacuna => Domain.Entities.Servicios.CategoriaServicio.Vacunacion,
            TipoCita.Procedimiento => Domain.Entities.Servicios.CategoriaServicio.Diagnostico,
            TipoCita.Urgencia => Domain.Entities.Servicios.CategoriaServicio.Emergencia,
            TipoCita.Seguimiento => Domain.Entities.Servicios.CategoriaServicio.Consulta,
            _ => Domain.Entities.Servicios.CategoriaServicio.Consulta
        };
    }

    public async Task<List<PagoPendienteDto>> GetPagosPendientesByUsuarioAsync(Guid usuarioId)
    {
        var todasLasCitasPendientes = await GetPagosPendientesAsync();
        
        return todasLasCitasPendientes
            .Where(p => p.PropietarioId == usuarioId)
            .ToList();
    }

    public async Task<PagoDto> CompletarPagoRestanteAsync(CompletarPagoRestanteDto dto, Guid userId)
    {
        // 1. Verificar que la cita existe
        var cita = await _context.Citas
            .Include(c => c.Propietario)
            .FirstOrDefaultAsync(c => c.Id == dto.CitaId)
            ?? throw new KeyNotFoundException($"Cita con ID {dto.CitaId} no encontrada");

        // 2. Obtener los pagos existentes de la cita
        var pagosExistentes = await _context.Pagos
            .Where(p => p.CitaId == dto.CitaId && p.Estado == EstadoPago.Completado)
            .ToListAsync();

        if (!pagosExistentes.Any())
        {
            throw new InvalidOperationException("Esta cita no tiene pagos registrados");
        }

        // 3. Obtener el pago de anticipo (si existe)
        Pago? pagoAnticipo = null;
        if (dto.PagoAnticipoId.HasValue)
        {
            pagoAnticipo = pagosExistentes.FirstOrDefault(p => p.Id == dto.PagoAnticipoId.Value);
            
            if (pagoAnticipo == null)
            {
                throw new KeyNotFoundException($"Pago de anticipo con ID {dto.PagoAnticipoId.Value} no encontrado");
            }
        }
        else
        {
            // Buscar el primer pago que sea anticipo
            pagoAnticipo = pagosExistentes.FirstOrDefault(p => p.EsAnticipo);
        }

        // 4. Calcular el monto pendiente
        decimal montoTotal = 0;
        
        if (pagoAnticipo != null && pagoAnticipo.MontoTotal.HasValue)
        {
            montoTotal = pagoAnticipo.MontoTotal.Value;
        }
        else
        {
            throw new InvalidOperationException("No se puede determinar el monto total de la cita");
        }

        var totalPagado = pagosExistentes.Sum(p => p.Monto);
        var montoPendiente = montoTotal - totalPagado;

        if (montoPendiente <= 0)
        {
            throw new InvalidOperationException("Esta cita ya está completamente pagada");
        }

        // 5. Crear el pago complementario
        var pagoComplementario = new Pago
        {
            Id = Guid.NewGuid(),
            UsuarioId = cita.PropietarioId,
            Monto = montoPendiente,
            Moneda = "MXN",
            Tipo = TipoPago.PagoComplementario,
            Metodo = (MetodoPago)dto.MetodoPago,
            Concepto = "Pago restante de cita",
            Referencia = dto.Referencia,
            CitaId = dto.CitaId,
            EsAnticipo = false,
            MontoTotal = montoTotal,
            MontoRestante = 0, // Ya no queda nada por pagar
            PagoPrincipalId = pagoAnticipo?.Id,
            Notas = dto.Notas,
            Estado = EstadoPago.Completado,
            FechaPago = DateTime.UtcNow,
            FechaConfirmacion = DateTime.UtcNow,
            CreatedBy = userId
        };

        pagoComplementario.NumeroPago = pagoComplementario.GenerarNumeroPago();

        _context.Pagos.Add(pagoComplementario);

        // 6. Actualizar el MontoRestante del pago anticipo
        if (pagoAnticipo != null)
        {
            pagoAnticipo.MontoRestante = 0;
            pagoAnticipo.UpdatedBy = userId;
            pagoAnticipo.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Pago restante completado para cita {CitaId}. Monto: {Monto}, Método: {Metodo}",
            dto.CitaId, montoPendiente, dto.MetodoPago);

        return await GetPagoByIdAsync(pagoComplementario.Id) 
            ?? throw new Exception("Error al obtener el pago creado");
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
