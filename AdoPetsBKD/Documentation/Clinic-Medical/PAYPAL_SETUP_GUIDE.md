# Integración de PayPal - Guía de Configuración

## ? Estado de Implementación

### Componentes Implementados:
- ? **SDK de PayPal instalado** (`PayPal v1.9.1`)
- ? **IPayPalClient** - Interfaz del cliente de PayPal
- ? **PayPalClient** - Implementación del cliente con el SDK
- ? **PagoService** - Servicio actualizado con integración real de PayPal
- ? **PagosController** - Controlador con todos los endpoints
- ? **Configuración en DI** - Servicio registrado correctamente
- ? **Modelos de datos** - Entidades y DTOs completos

---

## ?? Configuración de Credenciales de PayPal

### Paso 1: Crear una cuenta de desarrollador en PayPal

1. Ve a [PayPal Developer](https://developer.paypal.com/)
2. Inicia sesión con tu cuenta de PayPal (o crea una nueva)
3. Acepta los términos y condiciones de desarrollador

### Paso 2: Crear una aplicación Sandbox

1. En el dashboard, ve a **"Apps & Credentials"**
2. Asegúrate de estar en el modo **"Sandbox"** (parte superior)
3. Haz clic en **"Create App"**
4. Proporciona los siguientes datos:
   - **App Name**: `AdoPets PayPal Integration`
   - **Sandbox Business Account**: Selecciona la cuenta de negocios sandbox (se crea automáticamente)
5. Haz clic en **"Create App"**

### Paso 3: Obtener las credenciales

Una vez creada la aplicación, verás dos valores importantes:

1. **Client ID** - Identificador público de tu aplicación
2. **Secret** - Clave secreta (haz clic en "Show" para verla)

### Paso 4: Configurar en appsettings.json

Copia las credenciales y actualiza el archivo `appsettings.json`:

```json
{
  "PayPal": {
    "Mode": "sandbox",
    "ClientId": "TU_CLIENT_ID_AQUI",
    "ClientSecret": "TU_CLIENT_SECRET_AQUI"
  }
}
```

?? **IMPORTANTE**: Para producción, debes:
1. Cambiar `"Mode": "live"`
2. Crear una aplicación en modo **"Live"** en PayPal Developer
3. Usar las credenciales de producción
4. **NUNCA** subir las credenciales al repositorio (usa variables de entorno o Azure Key Vault)

---

## ?? Pruebas con Cuentas Sandbox

PayPal proporciona cuentas de prueba automáticamente:

### Cuentas de Prueba

1. Ve a **"Sandbox" > "Accounts"** en PayPal Developer
2. Verás dos cuentas creadas automáticamente:
   - **Business Account** (vendedor) - Para recibir pagos
   - **Personal Account** (comprador) - Para realizar pagos

3. Haz clic en los 3 puntos (**...**) de cada cuenta y selecciona **"View/Edit Account"**
4. Copia las credenciales:
   - **Email**: La dirección de correo sandbox
   - **Password**: La contraseña (generalmente `123456789` o similar)

### Tarjetas de Prueba

PayPal Sandbox también acepta estas tarjetas de prueba:

| Tipo | Número | CVV | Fecha de Expiración |
|------|--------|-----|---------------------|
| Visa | 4032032184654880 | 123 | Cualquier fecha futura |
| MasterCard | 5424180779220433 | 123 | Cualquier fecha futura |
| Discover | 6011111111111117 | 123 | Cualquier fecha futura |
| American Express | 378282246310005 | 1234 | Cualquier fecha futura |

---

## ?? Probar la Integración

### 1. Crear una Orden de Pago

**Endpoint**: `POST /api/pagos/paypal/create-order`

**Headers**:
```
Authorization: Bearer {tu_token_jwt}
Content-Type: application/json
```

**Body**:
```json
{
  "monto": 500.00,
  "concepto": "Consulta veterinaria - Prueba",
  "esAnticipo": false,
  "returnUrl": "http://localhost:5173/payment/success",
  "cancelUrl": "http://localhost:5173/payment/cancel"
}
```

**Respuesta Esperada**:
```json
{
  "success": true,
  "message": "Orden de PayPal creada exitosamente",
  "data": {
    "orderId": "PAYID-XXXXXXXXXXXXX",
    "approvalUrl": "https://www.sandbox.paypal.com/checkoutnow?token=EC-XXXXX",
    "status": "created"
  }
}
```

### 2. Aprobar el Pago

1. Copia el `approvalUrl` de la respuesta
2. Abre esa URL en tu navegador
3. Inicia sesión con las credenciales de la **cuenta Personal (comprador)** sandbox
4. Aprueba el pago
5. Serás redirigido a tu `returnUrl`

### 3. Capturar el Pago

**Endpoint**: `POST /api/pagos/paypal/capture`

**Headers**:
```
Authorization: Bearer {tu_token_jwt}
Content-Type: application/json
```

**Body**:
```json
{
  "orderId": "PAYID-XXXXXXXXXXXXX"
}
```

**Respuesta Esperada**:
```json
{
  "success": true,
  "message": "Pago capturado exitosamente",
  "data": {
    "id": "guid-del-pago",
    "numeroPago": "PAGO-20240115-001",
    "monto": 500.00,
    "metodoPago": "PayPal",
    "estado": "Completado",
    "payPalOrderId": "PAYID-XXXXXXXXXXXXX",
    "payPalCaptureId": "CAPTURE-XXXXXXXXXXXXX",
    "payPalPayerEmail": "sb-buyer@business.example.com",
    "payPalPayerName": "John Doe"
  }
}
```

---

## ?? Flujo Completo desde el Frontend

### Ejemplo con JavaScript/TypeScript

```typescript
// 1. Crear orden de pago
async function crearOrdenPayPal(monto: number, concepto: string) {
  const response = await fetch('http://localhost:5000/api/pagos/paypal/create-order', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      monto: monto,
      concepto: concepto,
      esAnticipo: false,
      returnUrl: `${window.location.origin}/payment/success`,
      cancelUrl: `${window.location.origin}/payment/cancel`
    })
  });

  const data = await response.json();
  return data.data; // { orderId, approvalUrl, status }
}

// 2. Redirigir al usuario a PayPal
function redirigirAPayPal(approvalUrl: string) {
  window.location.href = approvalUrl;
}

// 3. Cuando el usuario regrese (en /payment/success)
async function capturarPago(orderId: string) {
  const response = await fetch('http://localhost:5000/api/pagos/paypal/capture', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ orderId: orderId })
  });

  const data = await response.json();
  return data.data; // Pago completado
}

// Flujo completo
async function procesarPagoConPayPal() {
  try {
    // Paso 1: Crear orden
    const orden = await crearOrdenPayPal(500, "Consulta veterinaria");
    
    // Guardar orderId en localStorage para recuperarlo después
    localStorage.setItem('paypal_order_id', orden.orderId);
    
    // Paso 2: Redirigir a PayPal
    redirigirAPayPal(orden.approvalUrl);
    
    // El usuario será redirigido de vuelta después de aprobar
  } catch (error) {
    console.error('Error al crear orden de PayPal:', error);
  }
}

// En la página de retorno (/payment/success)
async function procesarRetornoPayPal() {
  const orderId = localStorage.getItem('paypal_order_id');
  
  if (orderId) {
    try {
      // Paso 3: Capturar el pago
      const pago = await capturarPago(orderId);
      console.log('Pago completado:', pago);
      
      // Limpiar localStorage
      localStorage.removeItem('paypal_order_id');
      
      // Mostrar mensaje de éxito al usuario
      alert(`Pago completado exitosamente. Folio: ${pago.numeroPago}`);
    } catch (error) {
      console.error('Error al capturar pago:', error);
    }
  }
}
```

---

## ?? Monitoreo y Logs

### Ver transacciones en PayPal Sandbox

1. Inicia sesión en [PayPal Sandbox](https://www.sandbox.paypal.com/)
2. Usa las credenciales de la **cuenta Business** para ver los pagos recibidos
3. Verás todas las transacciones de prueba

### Logs en la Aplicación

La aplicación registra todos los eventos importantes:

```csharp
_logger.LogInformation("Pago de PayPal creado: {PaymentId}", createdPayment.id);
_logger.LogInformation("Pago de PayPal ejecutado: {PaymentId}", executedPayment.id);
_logger.LogError(ex, "Error al crear pago en PayPal: {Message}", ex.Message);
```

---

## ?? Errores Comunes y Soluciones

### Error: "Authentication failed"
- **Causa**: Credenciales incorrectas o expiradas
- **Solución**: Verifica que ClientId y ClientSecret sean correctos y estés en modo "sandbox"

### Error: "Payment not approved"
- **Causa**: El usuario no aprobó el pago en PayPal
- **Solución**: Asegúrate de que el usuario complete el flujo en PayPal antes de capturar

### Error: "Invalid currency"
- **Causa**: PayPal no soporta la moneda configurada
- **Solución**: Usa "USD", "MXN", "EUR" u otra moneda soportada

### Error: "Amount too low"
- **Causa**: PayPal requiere un monto mínimo (generalmente $1.00)
- **Solución**: Asegúrate de que el monto sea mayor a 1.00

---

## ?? Seguridad en Producción

### Mejores Prácticas:

1. **Variables de Entorno**: Nunca hardcodees las credenciales
   ```csharp
   "ClientId": Environment.GetEnvironmentVariable("PAYPAL_CLIENT_ID")
   ```

2. **Azure Key Vault**: Almacena secretos en la nube
   ```csharp
   var keyVaultClient = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
   var secret = await keyVaultClient.GetSecretAsync("PayPal-ClientSecret");
   ```

3. **Validación de Webhooks**: Implementa verificación de firma HMAC-SHA256

4. **HTTPS Obligatorio**: En producción, usa solo HTTPS

5. **Rate Limiting**: Implementa límites de tasa en los endpoints de pago

---

## ?? Soporte

- **Documentación de PayPal**: https://developer.paypal.com/docs/
- **SDK .NET de PayPal**: https://github.com/paypal/PayPal-NET-SDK
- **Foro de Desarrolladores**: https://www.paypal-community.com/

---

## ? Checklist de Producción

Antes de lanzar a producción:

- [ ] Obtener credenciales de PayPal en modo **"Live"**
- [ ] Cambiar `"Mode": "live"` en configuración
- [ ] Mover credenciales a variables de entorno o Key Vault
- [ ] Configurar URLs de retorno de producción
- [ ] Implementar webhooks de PayPal para notificaciones automáticas
- [ ] Configurar monitoreo y alertas
- [ ] Realizar pruebas end-to-end en producción
- [ ] Revisar comisiones y fees de PayPal
- [ ] Configurar reconciliación de pagos
- [ ] Documentar proceso de reembolsos

---

**Desarrollado por**: Developer 3 - Beto  
**Módulo**: Clinic & Medical Records - Sistema de Pagos  
**Versión**: 1.0  
**Última Actualización**: Enero 2024
