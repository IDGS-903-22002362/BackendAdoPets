using Microsoft.Extensions.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Infrastructure.Configuration;

namespace AdoPetsBKD.Infrastructure.Services;

/// <summary>
/// Implementación del cliente de PayPal usando el SDK oficial moderno
/// </summary>
public class PayPalClient : IPayPalClient
{
    private readonly PayPalSettings _settings;
    private readonly ILogger<PayPalClient> _logger;
    private readonly PayPalHttpClient _client;

    public PayPalClient(IOptions<PayPalSettings> settings, ILogger<PayPalClient> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Inicializar el cliente según el modo (sandbox o live)
        PayPalEnvironment environment = _settings.Mode.ToLower() == "live"
            ? new LiveEnvironment(_settings.ClientId, _settings.ClientSecret)
            : new SandboxEnvironment(_settings.ClientId, _settings.ClientSecret);

        _client = new PayPalHttpClient(environment);
    }

    /// <summary>
    /// Obtiene el cliente HTTP de PayPal configurado
    /// </summary>
    public PayPalHttpClient GetClient()
    {
        return _client;
    }

    /// <summary>
    /// Crea una orden de pago en PayPal
    /// </summary>
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

    /// <summary>
    /// Captura una orden después de que el usuario la apruebe
    /// </summary>
    public async Task<Order> CaptureOrderAsync(string orderId)
    {
        try
        {
            var request = new OrdersCaptureRequest(orderId);
            request.Prefer("return=representation");
            request.RequestBody(new OrderActionRequest());

            var response = await _client.Execute(request);
            var order = response.Result<Order>();

            _logger.LogInformation("Orden de PayPal capturada exitosamente: {OrderId}", orderId);

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al capturar orden de PayPal: {OrderId}", orderId);
            throw new Exception($"Error al capturar orden de PayPal: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Obtiene los detalles de una orden existente
    /// </summary>
    public async Task<Order> GetOrderDetailsAsync(string orderId)
    {
        try
        {
            var request = new OrdersGetRequest(orderId);
            var response = await _client.Execute(request);
            var order = response.Result<Order>();

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de la orden: {OrderId}", orderId);
            throw new Exception($"Error al obtener detalles de la orden: {ex.Message}", ex);
        }
    }
}
