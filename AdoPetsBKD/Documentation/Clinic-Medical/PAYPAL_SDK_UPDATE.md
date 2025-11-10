# ?? SDK de PayPal Actualizado - .NET 9

## ? Cambios Realizados

### SDK Anterior (Deprecado)
```xml
<PackageReference Include="PayPal" Version="1.9.1" />
```
? **SDK antiguo y deprecado**

### SDK Nuevo (Moderno y Actualizado)
```xml
<PackageReference Include="PayPalCheckoutSdk" Version="1.0.4" />
```
? **SDK oficial moderno de PayPal**

---

## ?? Información del Nuevo SDK

### PayPalCheckoutSdk v1.0.4

| Característica | Detalle |
|----------------|---------|
| **Paquete** | `PayPalCheckoutSdk` |
| **Versión** | 1.0.4 (Última estable) |
| **Dependencias** | `PayPalHttp 1.0.1` |
| **Compatible con** | .NET Core 2.0+, .NET 5+, .NET 6+, .NET 9 ? |
| **Repositorio** | https://github.com/paypal/Checkout-NET-SDK |
| **Documentación** | https://developer.paypal.com/docs/checkout/reference/server-integration/ |

---

## ?? Principales Diferencias

### Antes (SDK Antiguo)
```csharp
// Configuración compleja
var config = new Dictionary<string, string>
{
    { "mode", "sandbox" },
    { "clientId", "..." },
    { "clientSecret", "..." }
};

var accessToken = new OAuthTokenCredential(config).GetAccessToken();
var apiContext = new APIContext(accessToken) { Config = config };

// Crear pago
var payment = new Payment { ... };
var createdPayment = payment.Create(apiContext);
```

### Ahora (SDK Moderno)
```csharp
// Configuración simplificada
PayPalEnvironment environment = new SandboxEnvironment(clientId, clientSecret);
PayPalHttpClient client = new PayPalHttpClient(environment);

// Crear orden
var request = new OrdersCreateRequest();
request.RequestBody(orderRequest);
var response = await client.Execute(request);
var order = response.Result<Order>();
```

---

## ? Ventajas del Nuevo SDK

### 1. Patrón Moderno
- ? Usa `async/await` de forma nativa
- ? Mejor manejo de errores
- ? API más limpia y fácil de usar

### 2. Mejor Performance
- ? Menos overhead
- ? Conexiones HTTP reutilizables
- ? Mejor gestión de memoria

### 3. Mejor Documentación
- ? Ejemplos actualizados
- ? Mejor soporte de la comunidad
- ? Activamente mantenido por PayPal

### 4. Compatibilidad
- ? Compatible con .NET 9
- ? Soporte para .NET Standard 2.0
- ? Compatible con contenedores Docker

---

## ?? Archivos Actualizados

### 1. IPayPalClient.cs
```csharp
// Antes
Task<Payment> CreatePaymentAsync(...)

// Ahora
Task<Order> CreateOrderAsync(...)
```

### 2. PayPalClient.cs
```csharp
// Antes
var accessToken = new OAuthTokenCredential(...).GetAccessToken();
var apiContext = new APIContext(accessToken);

// Ahora
PayPalEnvironment environment = new SandboxEnvironment(clientId, clientSecret);
PayPalHttpClient client = new PayPalHttpClient(environment);
```

### 3. PagoService.cs
```csharp
// Antes
var payment = await _paypalClient.CreatePaymentAsync(...);
var approvalUrl = payment.links.FirstOrDefault(l => l.rel == "approval_url").href;

// Ahora
var order = await _paypalClient.CreateOrderAsync(...);
var approvalUrl = order.Links?.FirstOrDefault(l => l.Rel == "approve")?.Href;
```

---

## ?? API del Nuevo SDK

### Crear Orden
```csharp
var orderRequest = new OrderRequest
{
    CheckoutPaymentIntent = "CAPTURE",
    PurchaseUnits = new List<PurchaseUnitRequest>
    {
        new PurchaseUnitRequest
        {
            AmountWithBreakdown = new AmountWithBreakdown
            {
                CurrencyCode = "MXN",
                Value = "500.00"
            },
            Description = "Consulta veterinaria"
        }
    },
    ApplicationContext = new ApplicationContext
    {
        ReturnUrl = "https://example.com/success",
        CancelUrl = "https://example.com/cancel",
        BrandName = "AdoPets",
        LandingPage = "BILLING",
        UserAction = "PAY_NOW"
    }
};

var request = new OrdersCreateRequest();
request.Prefer("return=representation");
request.RequestBody(orderRequest);

var response = await client.Execute(request);
var order = response.Result<Order>();
```

### Capturar Orden
```csharp
var request = new OrdersCaptureRequest(orderId);
request.Prefer("return=representation");
request.RequestBody(new OrderActionRequest());

var response = await client.Execute(request);
var order = response.Result<Order>();
```

### Obtener Detalles de Orden
```csharp
var request = new OrdersGetRequest(orderId);
var response = await client.Execute(request);
var order = response.Result<Order>();
```

---

## ?? Comparación de Objetos

### Payment (Antiguo) vs Order (Nuevo)

| Propiedad Antigua | Propiedad Nueva |
|-------------------|-----------------|
| `payment.id` | `order.Id` |
| `payment.state` | `order.Status` |
| `payment.links` | `order.Links` |
| `payment.payer.payer_info.email` | `order.Payer.Email` |
| `payment.transactions` | `order.PurchaseUnits` |

---

## ?? Configuración de Ambientes

### Sandbox
```csharp
var environment = new SandboxEnvironment(clientId, clientSecret);
var client = new PayPalHttpClient(environment);
```

### Live (Producción)
```csharp
var environment = new LiveEnvironment(clientId, clientSecret);
var client = new PayPalHttpClient(environment);
```

---

## ?? Pruebas

### El flujo de pruebas NO cambia:

1. **Crear Orden** ? `POST /api/pagos/paypal/create-order`
2. **Aprobar en PayPal** ? Usuario aprueba en la URL
3. **Capturar Pago** ? `POST /api/pagos/paypal/capture`

### URLs de Sandbox siguen igual:
- Dashboard: https://developer.paypal.com/
- Checkout: https://www.sandbox.paypal.com/

---

## ?? Credenciales

Las credenciales se siguen configurando igual en `appsettings.json`:

```json
{
  "PayPal": {
    "Mode": "sandbox",
    "ClientId": "TU_CLIENT_ID",
    "ClientSecret": "TU_CLIENT_SECRET"
  }
}
```

---

## ? Migración Automática Completada

? Todos los archivos han sido actualizados automáticamente:
- [x] IPayPalClient.cs
- [x] PayPalClient.cs
- [x] PagoService.cs
- [x] ServiceCollectionExtensions.cs (sin cambios)
- [x] PagosController.cs (sin cambios)

---

## ?? Recursos Adicionales

### Documentación Oficial
- **Checkout SDK**: https://github.com/paypal/Checkout-NET-SDK
- **API Reference**: https://developer.paypal.com/api/orders/v2/
- **Ejemplos**: https://github.com/paypal/Checkout-NET-SDK/tree/master/Samples

### Tutoriales
- **Checkout Integration**: https://developer.paypal.com/docs/checkout/
- **Orders API**: https://developer.paypal.com/docs/api/orders/v2/

---

## ?? Troubleshooting

### Error: "Authentication Failed"
**Solución**: Verifica que uses `SandboxEnvironment` para sandbox y `LiveEnvironment` para producción

### Error: "Unable to resolve service"
**Solución**: Asegúrate de que `IPayPalClient` esté registrado en DI (ya está configurado)

### Error: "Order not found"
**Solución**: Verifica que el orderId sea correcto y que la orden exista en PayPal

---

## ? Verificación de Instalación

Para verificar que el SDK está correctamente instalado:

```bash
dotnet list package
```

Deberías ver:
```
Proyecto 'AdoPetsBKD' tiene las siguientes referencias de paquete
   [net9.0]: 
   Paquete principal                         Solicitado    Resuelto
   > PayPalCheckoutSdk                      1.0.4         1.0.4
   > PayPalHttp                             (Transitivo)  1.0.1
```

---

## ?? ¡Listo para Usar!

El SDK moderno de PayPal está **completamente instalado y configurado**. 

### Siguiente Paso:
1. Obtén tus credenciales de PayPal Sandbox
2. Actualiza `appsettings.json`
3. ¡Prueba la integración!

---

**Fecha de Actualización**: Enero 2024  
**SDK Versión**: PayPalCheckoutSdk 1.0.4  
**Compatible con**: .NET 9  
**Status**: ? Completado
