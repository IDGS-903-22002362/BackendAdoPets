# ?? Guía de Integración - Captura de Pagos con PayPal

## ?? **IMPORTANTE: El Error Está en el Backend**

### ? **Problema Confirmado**
El frontend está enviando la petición correctamente (sin Content-Type), pero el **backend está enviando Content-Type a PayPal**, lo que causa el error `UNSUPPORTED_MEDIA_TYPE`.

### ? **Solución Implementada**

**Archivo:** `AdoPetsBKD\Infrastructure\Services\PayPalClient.cs`

**Líneas críticas (130-132):**
```csharp
// ?? CRÍTICO: PayPal requiere un body vacío pero SIN Content-Type
request.Content = new StringContent(string.Empty);
request.Content.Headers.ContentType = null; // ? DEBE ESTAR PRESENTE!
```

### ?? **Pasos para Verificar y Corregir**

#### **1?? Verificar el Código en `PayPalClient.cs`**

Abrir el archivo: `AdoPetsBKD\Infrastructure\Services\PayPalClient.cs`

Buscar el método `CaptureOrderAsync` (línea ~110) y verificar que tenga **exactamente** este código:

```csharp
public async Task<Order> CaptureOrderAsync(string orderId)
{
    try
    {
        _logger.LogInformation("PayPalClient.CaptureOrderAsync - Iniciando captura para OrderId: {OrderId}", orderId);
        
        var accessToken = await GetAccessTokenAsync();
        var requestUri = $"{_baseUrl}/v2/checkout/orders/{orderId}/capture";
        
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        // ?? CRÍTICO: Estas dos líneas DEBEN estar presentes
        request.Content = new StringContent(string.Empty);
        request.Content.Headers.ContentType = null; // ? LÍNEA CRÍTICA!
        
        _logger.LogInformation(
            "PayPalClient.CaptureOrderAsync - Enviando petición POST a {Uri} con body vacío (sin Content-Type)",
            requestUri);
        
        var response = await _httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        // ... resto del código
    }
}
```

#### **2?? Recompilar el Proyecto**

```bash
dotnet build AdoPetsBKD.csproj --configuration Release
```

#### **3?? Publicar en Azure**

```bash
# Desde la raíz del proyecto
dotnet publish AdoPetsBKD.csproj -c Release -o ./publish

# O usar GitHub Actions si está configurado
git add .
git commit -m "fix: Remover Content-Type en captura de PayPal"
git push origin main
```

#### **4?? Verificar en los Logs de Azure**

Después de desplegar, revisar **Application Insights** o **Log Stream** en Azure para ver los logs:

**Buscar estos logs:**
```
[INFO] PayPalClient.CaptureOrderAsync - Content-Type después de set null: NULL (debe ser null o vacío)
[INFO] PayPalClient.CaptureOrderAsync - Headers de la petición a PayPal:
  Request Headers: Authorization=Bearer ..., Accept=application/json
  Content Headers: NONE
```

**Si ves esto, significa que el Content-Type fue removido correctamente. ?**

**Si ves esto, hay un problema:**
```
Content Headers: Content-Type=text/plain; charset=utf-8
```

---

## ?? Cambios Realizados en el Backend

### ? Solución al Error `UNSUPPORTED_MEDIA_TYPE`

**Problema identificado:**
PayPal requiere que la petición de captura tenga:
- Un **body vacío** (no puede ser `null`)
- **SIN** el header `Content-Type`

**Solución implementada en `PayPalClient.cs`:**
```csharp
request.Content = new StringContent(string.Empty);
request.Content.Headers.ContentType = null; // ? Remover Content-Type
```

### ?? **Debugging Adicional**

Se agregaron logs detallados para verificar que el Content-Type se está removiendo correctamente:

```csharp
// Log del Content-Type (debe ser null)
var contentTypeValue = request.Content.Headers.ContentType;
_logger.LogInformation(
    "PayPalClient.CaptureOrderAsync - Content-Type después de set null: {ContentType} (debe ser null o vacío)",
    contentTypeValue?.ToString() ?? "NULL");

// Log de todos los headers de la petición
var requestHeaders = string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value)}"));
var contentHeaders = request.Content?.Headers != null 
    ? string.Join(", ", request.Content.Headers.Select(h => $"{h.Key}={string.Join(";", h.Value)}"))
    : "NONE";

_logger.LogInformation(
    "PayPalClient.CaptureOrderAsync - Headers de la petición a PayPal:\n" +
    "  Request Headers: {RequestHeaders}\n" +
    "  Content Headers: {ContentHeaders}",
    requestHeaders, contentHeaders);
```

---

## ?? Endpoints de Pago con PayPal

### 1?? **Crear Orden PayPal** (Pago Restante de Cita)

**Endpoint:**
```
POST /api/v1/Pagos/completar-pago/paypal
```

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "citaId": "guid-de-la-cita",
  "returnUrl": "http://localhost:3000/cobranza?success=true",
  "cancelUrl": "http://localhost:3000/cobranza?cancelled=true"
}
```

**Respuesta Exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Orden de PayPal creada para completar pago de $500.00 MXN",
  "data": {
    "orderId": "8VF91827TN047864P",
    "token": "8VF91827TN047864P",
    "approvalUrl": "https://www.sandbox.paypal.com/checkoutnow?token=8VF91827TN047864P",
    "status": "CREATED"
  }
}
```

---

### 2?? **Capturar Pago de PayPal**

**Endpoint:**
```
POST /api/v1/Pagos/paypal/capture/{orderId}
```

**?? IMPORTANTE:** El `orderId` va en la **ruta**, NO en el body.

**Headers:**
```
Authorization: Bearer {token}
```

**NO enviar:**
- ? Body
- ? Content-Type

**Ejemplo de petición correcta:**
```
POST /api/v1/Pagos/paypal/capture/8VF91827TN047864P
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Respuesta Exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Pago capturado exitosamente",
  "data": {
    "id": "pago-guid",
    "numeroPago": "PAGO-20240115-0001",
    "usuarioId": "usuario-guid",
    "nombreUsuario": "Juan Pérez",
    "monto": 500.00,
    "moneda": "MXN",
    "tipo": 3,
    "tipoNombre": "PagoComplementario",
    "metodo": 1,
    "metodoNombre": "PayPal",
    "estado": 3,
    "estadoNombre": "Completado",
    "payPalOrderId": "8VF91827TN047864P",
    "payPalCaptureId": "3C679366E0199954L",
    "payPalPayerEmail": "buyer@example.com",
    "payPalPayerName": "John Doe",
    "fechaPago": "2024-01-15T14:50:00Z",
    "fechaConfirmacion": "2024-01-15T14:50:00Z",
    "concepto": "Pago restante de cita - $500.00 MXN",
    "citaId": "cita-guid",
    "esAnticipo": false,
    "montoTotal": 1000.00,
    "montoRestante": 0,
    "createdAt": "2024-01-15T14:48:00Z"
  }
}
```

---

## ?? Código Frontend - React (Axios/Fetch)

### **Versión Correcta con Fetch** ?

```javascript
// ===== CAPTURAR PAGO CON FETCH =====
const capturarPagoPayPal = async (orderId) => {
  try {
    console.log('?? Capturando pago de PayPal...', { orderId });
    
    const response = await fetch(
      `https://adopets-bkd.azurewebsites.net/api/v1/Pagos/paypal/capture/${orderId}`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
          // ? NO incluir Content-Type
        }
        // ? NO incluir body
      }
    );

    const data = await response.json();
    
    if (data.success) {
      console.log('? Pago capturado exitosamente:', data.data);
      return data.data;
    } else {
      throw new Error(data.message || 'Error al capturar pago');
    }
  } catch (error) {
    console.error('? Error al capturar pago:', error);
    throw error;
  }
};
```

### **Versión Correcta con Axios** ?

```javascript
import axios from 'axios';

// Configuración de axios
const api = axios.create({
  baseURL: 'https://adopets-bkd.azurewebsites.net/api/v1',
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
});

// ===== CAPTURAR PAGO CON AXIOS =====
const capturarPagoPayPal = async (orderId) => {
  try {
    console.log('?? Capturando pago de PayPal...', { orderId });
    
    // ? orderId en la URL, SIN body, SIN Content-Type
    const response = await api.post(
      `/Pagos/paypal/capture/${orderId}`,
      null, // ? Sin body
      {
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
          // NO incluir Content-Type
        }
      }
    );

    if (response.data.success) {
      console.log('? Pago capturado exitosamente:', response.data.data);
      return response.data.data;
    }
  } catch (error) {
    console.error('? Error al capturar pago:', error);
    throw error;
  }
};
```

---

## ?? Prueba el Flujo Completo

### **Paso 1:** Crear orden PayPal
```javascript
const crearOrdenPayPal = async (citaId) => {
  const response = await api.post('/Pagos/completar-pago/paypal', {
    citaId: citaId,
    returnUrl: `${window.location.origin}/cobranza?success=true`,
    cancelUrl: `${window.location.origin}/cobranza?cancelled=true`
  });

  if (response.data.success) {
    const { orderId, approvalUrl } = response.data.data;
    sessionStorage.setItem('paypal_order_id', orderId);
    window.location.href = approvalUrl;
  }
};
```

### **Paso 2:** Usuario aprueba en PayPal
- PayPal redirige a: `http://localhost:3000/cobranza?success=true&token=8VF91827TN047864P`

### **Paso 3:** Captura automática
```javascript
useEffect(() => {
  const urlParams = new URLSearchParams(window.location.search);
  const success = urlParams.get('success');
  const token = urlParams.get('token'); // Este es el orderId
  
  if (success === 'true' && token) {
    capturarPagoPayPal(token)
      .then((pago) => {
        Swal.fire({
          icon: 'success',
          title: 'Pago Confirmado',
          text: `Pago de $${pago.monto} MXN confirmado`
        });
        
        window.history.replaceState({}, document.title, '/cobranza');
        fetchPagosPendientes();
      })
      .catch((error) => {
        Swal.fire({
          icon: 'error',
          title: 'Error',
          text: error.response?.data?.message || 'No se pudo confirmar el pago'
        });
      });
  }
}, []);
```

---

## ?? Logs Esperados en el Backend

### ? **Flujo Exitoso (después del fix):**
```
[INFO] PagosController.CapturePayPalPayment - INICIO. OrderId recibido: 8VF91827TN047864P
[INFO] PagoService.CapturePayPalPaymentAsync - INICIO. OrderId/Token recibido: 8VF91827TN047864P
[INFO] PagoService.CapturePayPalPaymentAsync - Pago encontrado con búsqueda directa. PagoId: {guid}
[INFO] PayPalClient.CaptureOrderAsync - Iniciando captura para OrderId: 8VF91827TN047864P
[INFO] PayPalClient.CaptureOrderAsync - Token de acceso obtenido exitosamente
[INFO] PayPalClient.CaptureOrderAsync - Content-Type después de set null: NULL (debe ser null o vacío)
[INFO] PayPalClient.CaptureOrderAsync - Headers de la petición a PayPal:
  Request Headers: Authorization=Bearer ..., Accept=application/json
  Content Headers: NONE
[INFO] PayPalClient.CaptureOrderAsync - Enviando petición POST a https://api.sandbox.paypal.com/v2/checkout/orders/8VF91827TN047864P/capture con body vacío (sin Content-Type)
[INFO] PayPalClient.CaptureOrderAsync - Respuesta recibida. StatusCode: 200, ContentLength: 1245
[INFO] PayPalClient.CaptureOrderAsync - Orden capturada exitosamente. OrderId: 8VF91827TN047864P, Status: COMPLETED
[INFO] PagoService.CapturePayPalPaymentAsync - Pago completado exitosamente
[INFO] PagosController.CapturePayPalPayment - Pago capturado exitosamente
```

### ? **Error: UNSUPPORTED_MEDIA_TYPE (antes del fix):**
```
[ERROR] PayPalClient.CaptureOrderAsync - Error en la captura. 
StatusCode: UnsupportedMediaType, 
Content: {"name":"UNSUPPORTED_MEDIA_TYPE","message":"The request payload is not supported"...}
```

**Si sigues viendo este error:** El backend NO tiene la versión actualizada del código. Recompilar y redesplegar.

---

## ? Checklist de Implementación

### **Backend:**
- [ ] Verificar que `PayPalClient.cs` tiene la línea `request.Content.Headers.ContentType = null;`
- [ ] Recompilar el proyecto: `dotnet build`
- [ ] Publicar en Azure o reiniciar el servidor
- [ ] Verificar logs en Application Insights

### **Frontend:**
- [ ] El orderId se pasa en la **URL** como parámetro de ruta
- [ ] La petición **NO tiene body** (es `null` o se omite)
- [ ] La petición **NO tiene Content-Type** en los headers
- [ ] Solo se envía el header `Authorization: Bearer {token}`
- [ ] El frontend detecta correctamente el `token` en la URL de retorno

---

## ?? Troubleshooting

### **Problema:** Sigue apareciendo el error `UNSUPPORTED_MEDIA_TYPE`

**Posibles causas:**
1. ? **Frontend correcto** - Ya verificado
2. ? **Backend desactualizado** - Verificar que Azure tenga la última versión
3. ? **Caché de Azure** - Reiniciar el App Service

**Solución:**
```bash
# 1. Verificar versión del código
git log --oneline -1

# 2. Forzar despliegue
git push origin main --force

# 3. En Azure Portal:
# - Ir a App Service ? AdoPetsBKD
# - Click en "Restart"
# - Esperar 1-2 minutos
# - Probar nuevamente
```

---

### **Problema:** Los logs no muestran "Content-Type después de set null: NULL"

**Causa:** El backend no tiene los logs adicionales agregados.

**Solución:** Verificar que `PayPalClient.cs` tenga el código actualizado con los logs de debugging.

---

## ?? Comando para Verificar en Producción

Usar **Postman** o **curl** para probar directamente:

```bash
curl -X POST \
  'https://adopets-bkd.azurewebsites.net/api/v1/Pagos/paypal/capture/8VF91827TN047864P' \
  -H 'Authorization: Bearer TU_TOKEN_AQUI'
```

**Si funciona:** El problema está en el frontend (pero ya se verificó que está correcto).

**Si NO funciona:** El problema está en el backend (verificar despliegue).

---

**¡Listo! Ahora el sistema de pagos con PayPal debería funcionar correctamente.** ???

**Última actualización:** Enero 2024 - Versión 2.0 con debugging avanzado
