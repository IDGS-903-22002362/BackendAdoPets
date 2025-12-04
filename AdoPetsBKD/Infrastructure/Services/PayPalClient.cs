using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Infrastructure.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AdoPetsBKD.Infrastructure.Services
{
    public class PayPalClient : IPayPalClient
    {
        private readonly PayPalSettings _settings;
        private readonly ILogger<PayPalClient> _logger;
        private readonly PayPalHttpClient _client;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PayPalClient(IOptions<PayPalSettings> settings, ILogger<PayPalClient> logger, IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _logger = logger;

            PayPalEnvironment environment = _settings.Mode.ToLower() == "live"
                ? new LiveEnvironment(_settings.ClientId, _settings.ClientSecret)
                : new SandboxEnvironment(_settings.ClientId, _settings.ClientSecret);

            _client = new PayPalHttpClient(environment);

            _httpClient = httpClientFactory.CreateClient();
            _baseUrl = _settings.Mode.ToLower() == "live"
                ? "https://api.paypal.com"
                : "https://api.sandbox.paypal.com";
        }

        public PayPalHttpClient GetClient()
        {
            return _client;
        }

        // --------------------------------------------------------------------
        // 1. CREAR ORDEN
        // --------------------------------------------------------------------

        public async Task<Order> CreateOrderAsync(
            decimal amount,
            string currency,
            string description,
            string returnUrl,
            string cancelUrl)
        {
            try
            {
                var orderRequest = new OrderRequest
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    PurchaseUnits = new List<PurchaseUnitRequest>
                    {
                        new PurchaseUnitRequest
                        {
                            AmountWithBreakdown = new AmountWithBreakdown
                            {
                                CurrencyCode = currency,
                                Value = amount.ToString("F2")
                            },
                            Description = description
                        }
                    },
                    ApplicationContext = new ApplicationContext
                    {
                        ReturnUrl = returnUrl,
                        CancelUrl = cancelUrl,
                        BrandName = "AdoPets",
                        LandingPage = "BILLING",
                        UserAction = "PAY_NOW"
                    }
                };

                var request = new OrdersCreateRequest();
                request.Prefer("return=representation");
                request.RequestBody(orderRequest);

                var response = await _client.Execute(request);
                var order = response.Result<Order>();

                _logger.LogInformation("Orden de PayPal creada exitosamente: {OrderId}", order.Id);

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear orden de PayPal");
                throw new Exception($"Error al crear orden de PayPal: {ex.Message}", ex);
            }
        }

        // --------------------------------------------------------------------
        // 2. CAPTURAR ORDEN (CORREGIDO - VERSIÓN 2024-2025)
        // --------------------------------------------------------------------

        public async Task<PayPalCaptureResponseDto> CaptureOrderAsync(string orderId)
        {
            try
            {
                _logger.LogInformation("🔥 PayPalClient.CaptureOrderAsync - VERSIÓN ACTUALIZADA 2024-2025 - Capturando orden: {OrderId}", orderId);

                var accessToken = await GetAccessTokenAsync();
                var url = $"{_baseUrl}/v2/checkout/orders/{orderId}/capture";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                // Headers correctos
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // ⚠️ CRÍTICO: PayPal 2024-2025 REQUIERE body vacío "{}" con Content-Type
                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                _logger.LogInformation("Enviando POST a PayPal: {Url} con body {{}} y Content-Type: application/json", url);

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation(
                    "✅ Respuesta de PayPal recibida: StatusCode={StatusCode}, ContentLength={ContentLength}",
                    response.StatusCode,
                    json?.Length ?? 0);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("PayPal Error {Status}: {Body}", response.StatusCode, json);
                    throw new Exception($"PayPal Error {response.StatusCode}: {json}");
                }

                // Deserializar como JsonDocument para manejar la estructura de forma flexible
                using var jsonDoc = JsonDocument.Parse(json);
                var root = jsonDoc.RootElement;
                
                // Extraer campos clave manualmente
                string? status = root.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
                string? id = root.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                
                _logger.LogInformation("🔍 PayPal Response - ID: {Id}, Status: {Status}", id ?? "NULL", status ?? "NULL");

                // Intentar extraer información del payer
                string? payerEmail = null;
                string? payerName = null;
                
                if (root.TryGetProperty("payer", out var payerEl))
                {
                    _logger.LogInformation("✅ Payer encontrado en la respuesta");
                    
                    if (payerEl.TryGetProperty("email_address", out var emailEl))
                    {
                        payerEmail = emailEl.GetString();
                    }
                    
                    if (payerEl.TryGetProperty("name", out var nameEl))
                    {
                        var givenName = nameEl.TryGetProperty("given_name", out var gn) ? gn.GetString() : "";
                        var surname = nameEl.TryGetProperty("surname", out var sn) ? sn.GetString() : "";
                        payerName = $"{givenName} {surname}".Trim();
                    }
                    
                    _logger.LogInformation("👤 Payer Info - Email: {Email}, Name: {Name}", payerEmail ?? "NULL", payerName ?? "NULL");
                }
                else
                {
                    _logger.LogWarning("⚠️ NO se encontró 'payer' en la respuesta");
                }
                
                // Intentar extraer información del capture
                string? captureId = null;
                string? captureStatus = null;
                
                if (root.TryGetProperty("purchase_units", out var unitsEl) && unitsEl.ValueKind == JsonValueKind.Array)
                {
                    _logger.LogInformation("✅ purchase_units encontrado (Array con {Count} elementos)", unitsEl.GetArrayLength());
                    
                    var firstUnit = unitsEl.EnumerateArray().FirstOrDefault();
                    if (firstUnit.ValueKind != JsonValueKind.Undefined)
                    {
                        if (firstUnit.TryGetProperty("payments", out var paymentsEl))
                        {
                            _logger.LogInformation("✅ payments encontrado");
                            
                            if (paymentsEl.TryGetProperty("captures", out var capturesEl) && capturesEl.ValueKind == JsonValueKind.Array)
                            {
                                _logger.LogInformation("✅ captures encontrado (Array con {Count} elementos)", capturesEl.GetArrayLength());
                                
                                var firstCapture = capturesEl.EnumerateArray().FirstOrDefault();
                                if (firstCapture.ValueKind != JsonValueKind.Undefined)
                                {
                                    captureId = firstCapture.TryGetProperty("id", out var capIdEl) ? capIdEl.GetString() : null;
                                    captureStatus = firstCapture.TryGetProperty("status", out var capStatusEl) ? capStatusEl.GetString() : null;
                                    
                                    _logger.LogInformation("💰 Capture encontrado - ID: {CaptureId}, Status: {CaptureStatus}", captureId ?? "NULL", captureStatus ?? "NULL");
                                }
                                else
                                {
                                    _logger.LogWarning("⚠️ captures array está vacío");
                                }
                            }
                            else
                            {
                                _logger.LogWarning("⚠️ NO se encontró 'captures' en payments o no es un array");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ NO se encontró 'payments' en purchase_unit");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ NO se encontró 'purchase_units' o no es un array");
                }

                // Determinar el status final
                var finalStatus = status ?? captureStatus ?? "UNKNOWN";
                
                _logger.LogInformation(
                    "🎉 Captura exitosa. OrderId: {OrderId}, Status: {Status}, CaptureId: {CaptureId}, PayerEmail: {PayerEmail}",
                    orderId, finalStatus, captureId ?? "NULL", payerEmail ?? "NULL");

                // Devolver DTO con la información extraída
                return new PayPalCaptureResponseDto
                {
                    OrderId = orderId,
                    Status = finalStatus,
                    CaptureId = captureId,
                    CaptureStatus = captureStatus,
                    PayerEmail = payerEmail,
                    PayerName = payerName,
                    CapturedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error capturando orden {OrderId}", orderId);
                throw new Exception($"Error al capturar orden de PayPal: {ex.Message}", ex);
            }
        }

        // --------------------------------------------------------------------
        // 3. DETALLES DE ORDEN
        // --------------------------------------------------------------------

        public async Task<Order> GetOrderDetailsAsync(string orderId)
        {
            try
            {
                var request = new OrdersGetRequest(orderId);
                var response = await _client.Execute(request);
                return response.Result<Order>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles de orden: {OrderId}", orderId);
                throw new Exception($"Error al obtener detalles de la orden: {ex.Message}", ex);
            }
        }

        // --------------------------------------------------------------------
        // 4. TOKEN
        // --------------------------------------------------------------------

        private async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/v1/oauth2/token");

                var credentials = Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}")
                );

                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var response = await _httpClient.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener token: {json}");
                }

                var parsed = JsonSerializer.Deserialize<JsonElement>(json);
                return parsed.GetProperty("access_token").GetString()
                       ?? throw new Exception("PayPal no regresó access_token");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener token de PayPal");
                throw;
            }
        }
    }
}
