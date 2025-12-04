using PayPalCheckoutSdk.Orders;
using PayPalCheckoutSdk.Core;
using AdoPetsBKD.Application.DTOs.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el cliente de PayPal usando el SDK moderno
/// </summary>
public interface IPayPalClient
{
    /// <summary>
    /// Obtiene el cliente HTTP de PayPal configurado
    /// </summary>
    PayPalHttpClient GetClient();

    /// <summary>
    /// Crea una orden de pago en PayPal
    /// </summary>
    Task<Order> CreateOrderAsync(decimal amount, string currency, string description, string returnUrl, string cancelUrl);

    /// <summary>
    /// Captura una orden aprobada por el usuario
    /// Devuelve información extraída manualmente del JSON de PayPal
    /// </summary>
    Task<PayPalCaptureResponseDto> CaptureOrderAsync(string orderId);

    /// <summary>
    /// Obtiene los detalles de una orden
    /// </summary>
    Task<Order> GetOrderDetailsAsync(string orderId);
}
