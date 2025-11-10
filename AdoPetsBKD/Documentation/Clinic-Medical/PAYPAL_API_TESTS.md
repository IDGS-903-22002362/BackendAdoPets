# Colección de Pruebas de PayPal para AdoPets API

## Variables de Entorno
```
base_url = http://localhost:5000
token = {tu_token_jwt_aqui}
```

---

## 1?? Crear Orden de PayPal (Pago Completo)

**POST** `{{base_url}}/api/pagos/paypal/create-order`

### Headers
```
Authorization: Bearer {{token}}
Content-Type: application/json
```

### Body
```json
{
  "monto": 500.00,
  "concepto": "Consulta veterinaria general - Prueba",
  "esAnticipo": false,
  "montoTotal": null,
  "citaId": null,
  "solicitudCitaId": null,
  "returnUrl": "http://localhost:5173/payment/success",
  "cancelUrl": "http://localhost:5173/payment/cancel"
}
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": "Orden de PayPal creada exitosamente",
  "data": {
    "orderId": "PAYID-MZ7ABCD1234567890",
    "approvalUrl": "https://www.sandbox.paypal.com/checkoutnow?token=EC-12345ABCDE",
    "status": "created"
  },
  "errors": null,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Siguiente paso**: Copia el `approvalUrl` y ábrelo en tu navegador para aprobar el pago.

---

## 2?? Crear Orden de PayPal (Anticipo 50%)

**POST** `{{base_url}}/api/pagos/paypal/create-order`

### Headers
```
Authorization: Bearer {{token}}
Content-Type: application/json
```

### Body
```json
{
  "monto": 609.00,
  "concepto": "Anticipo 50% para cirugía de esterilización",
  "esAnticipo": true,
  "montoTotal": 1218.00,
  "citaId": "guid-de-la-cita",
  "solicitudCitaId": null,
  "returnUrl": "http://localhost:5173/payment/success?type=anticipo",
  "cancelUrl": "http://localhost:5173/payment/cancel"
}
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": "Orden de PayPal creada exitosamente",
  "data": {
    "orderId": "PAYID-MZ7ABCD9876543210",
    "approvalUrl": "https://www.sandbox.paypal.com/checkoutnow?token=EC-98765EDCBA",
    "status": "created"
  },
  "errors": null,
  "timestamp": "2024-01-15T10:35:00Z"
}
```

---

## 3?? Capturar Pago de PayPal

**POST** `{{base_url}}/api/pagos/paypal/capture`

### Headers
```
Authorization: Bearer {{token}}
Content-Type: application/json
```

### Body
```json
{
  "orderId": "PAYID-MZ7ABCD1234567890"
}
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": "Pago capturado exitosamente",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAGO-20240115-001",
    "usuarioId": "12345678-1234-1234-1234-123456789012",
    "nombreUsuario": "Juan Pérez López",
    "monto": 500.00,
    "moneda": "MXN",
    "tipo": 2,
    "tipoNombre": "PagoCompleto",
    "metodo": 1,
    "metodoNombre": "PayPal",
    "estado": 2,
    "estadoNombre": "Completado",
    "payPalOrderId": "PAYID-MZ7ABCD1234567890",
    "payPalCaptureId": "8XV12345AB6789012",
    "payPalPayerEmail": "sb-buyer@business.example.com",
    "payPalPayerName": "John Doe",
    "fechaPago": "2024-01-15T10:40:00Z",
    "fechaConfirmacion": "2024-01-15T10:40:00Z",
    "concepto": "Consulta veterinaria general - Prueba",
    "referencia": null,
    "citaId": null,
    "ticketId": null,
    "esAnticipo": false,
    "montoTotal": null,
    "montoRestante": null,
    "createdAt": "2024-01-15T10:30:00Z"
  },
  "errors": null,
  "timestamp": "2024-01-15T10:40:00Z"
}
```

---

## 4?? Obtener Pago por ID

**GET** `{{base_url}}/api/pagos/{id}`

### Headers
```
Authorization: Bearer {{token}}
```

### Parámetros de URL
```
id = 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAGO-20240115-001",
    "monto": 500.00,
    "estado": 2,
    "estadoNombre": "Completado",
    "payPalOrderId": "PAYID-MZ7ABCD1234567890"
  },
  "errors": null,
  "timestamp": "2024-01-15T10:45:00Z"
}
```

---

## 5?? Obtener Pago por PayPal Order ID

**GET** `{{base_url}}/api/pagos/paypal/{orderId}`

### Headers
```
Authorization: Bearer {{token}}
```

### Parámetros de URL
```
orderId = PAYID-MZ7ABCD1234567890
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": null,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAGO-20240115-001",
    "payPalOrderId": "PAYID-MZ7ABCD1234567890",
    "estado": 2,
    "estadoNombre": "Completado"
  },
  "errors": null,
  "timestamp": "2024-01-15T10:50:00Z"
}
```

---

## 6?? Obtener Historial de Pagos por Usuario

**GET** `{{base_url}}/api/pagos/usuario/{usuarioId}`

### Headers
```
Authorization: Bearer {{token}}
```

### Parámetros de URL
```
usuarioId = 12345678-1234-1234-1234-123456789012
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": null,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "numeroPago": "PAGO-20240115-001",
      "monto": 500.00,
      "metodo": 1,
      "metodoNombre": "PayPal",
      "estado": 2,
      "estadoNombre": "Completado",
      "concepto": "Consulta veterinaria general",
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": "4gb96f75-6828-5673-c4gd-3d074g77bgb7",
      "numeroPago": "PAGO-20240110-015",
      "monto": 609.00,
      "metodo": 1,
      "metodoNombre": "PayPal",
      "estado": 2,
      "estadoNombre": "Completado",
      "concepto": "Anticipo 50% para cirugía",
      "esAnticipo": true,
      "montoTotal": 1218.00,
      "montoRestante": 609.00,
      "createdAt": "2024-01-10T14:20:00Z"
    }
  ],
  "errors": null,
  "timestamp": "2024-01-15T10:55:00Z"
}
```

---

## 7?? Crear Pago Manual (Efectivo)

**POST** `{{base_url}}/api/pagos`

### Headers
```
Authorization: Bearer {{token}}
Content-Type: application/json
```

### Body
```json
{
  "monto": 500.00,
  "moneda": "MXN",
  "tipo": 2,
  "metodo": 2,
  "concepto": "Pago de consulta en efectivo",
  "referencia": "REC-001",
  "citaId": null,
  "ticketId": null,
  "esAnticipo": false,
  "montoTotal": null
}
```

**Nota sobre los enums**:
- **Tipo**: 1 = Anticipo, 2 = PagoCompleto
- **Metodo**: 1 = PayPal, 2 = Efectivo, 3 = TarjetaCredito, 4 = TarjetaDebito, 5 = Transferencia

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": "Pago creado exitosamente",
  "data": {
    "id": "5hc07g86-7939-6784-d5he-4e185h88chc8",
    "numeroPago": "PAGO-20240115-002",
    "monto": 500.00,
    "metodo": 2,
    "metodoNombre": "Efectivo",
    "estado": 2,
    "estadoNombre": "Completado",
    "concepto": "Pago de consulta en efectivo",
    "referencia": "REC-001",
    "createdAt": "2024-01-15T11:00:00Z"
  },
  "errors": null,
  "timestamp": "2024-01-15T11:00:00Z"
}
```

---

## 8?? Cancelar Pago (Solo Admin)

**PUT** `{{base_url}}/api/pagos/{id}/cancelar`

### Headers
```
Authorization: Bearer {{admin_token}}
Content-Type: application/json
```

### Parámetros de URL
```
id = 3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Body
```json
"Error en el monto capturado, se requiere corrección"
```

### Respuesta Esperada (200 OK)
```json
{
  "success": true,
  "message": "Pago cancelado",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAGO-20240115-001",
    "estado": 4,
    "estadoNombre": "Cancelado"
  },
  "errors": null,
  "timestamp": "2024-01-15T11:10:00Z"
}
```

---

## 9?? Webhook de PayPal (Endpoint Público)

**POST** `{{base_url}}/api/pagos/webhook/paypal`

?? **Este endpoint es llamado automáticamente por PayPal**, no necesitas probarlo manualmente.

### Headers
```
Content-Type: application/json
```

### Body (Ejemplo de evento PAYMENT.CAPTURE.COMPLETED)
```json
{
  "eventType": "PAYMENT.CAPTURE.COMPLETED",
  "eventId": "WH-EVENT-12345ABCDE",
  "resource": {
    "id": "8XV12345AB6789012",
    "status": "COMPLETED",
    "amount": {
      "currency_code": "MXN",
      "value": "500.00"
    },
    "parent_payment": "PAYID-MZ7ABCD1234567890"
  }
}
```

### Respuesta Esperada (200 OK)
```
OK
```

---

## ?? Configuración para Postman

### Importar como Collection

1. Copia todo el contenido de este archivo
2. En Postman, ve a **File > Import**
3. Selecciona **Raw text** y pega el contenido
4. Postman creará automáticamente la colección

### Configurar Variables de Entorno

1. En Postman, crea un nuevo **Environment** llamado "AdoPets Dev"
2. Agrega estas variables:
   ```
   base_url: http://localhost:5000
   token: {obtener-de-login}
   admin_token: {obtener-de-login-admin}
   ```

---

## ?? Escenarios de Prueba

### Escenario 1: Pago Completo de Consulta
```
1. POST /api/pagos/paypal/create-order (monto: 500, esAnticipo: false)
2. Abrir approvalUrl en navegador
3. Aprobar con cuenta sandbox
4. POST /api/pagos/paypal/capture (orderId del paso 1)
5. GET /api/pagos/{id} (verificar estado = Completado)
```

### Escenario 2: Anticipo del 50%
```
1. POST /api/pagos/paypal/create-order (monto: 609, esAnticipo: true, montoTotal: 1218)
2. Aprobar en PayPal
3. POST /api/pagos/paypal/capture
4. Verificar montoRestante = 609.00
```

### Escenario 3: Cancelación de Pago
```
1. Crear pago (cualquier método)
2. PUT /api/pagos/{id}/cancelar (con token de admin)
3. Verificar estado = Cancelado
```

---

## ?? Estados de Pago

| Estado | Código | Descripción |
|--------|--------|-------------|
| Pendiente | 1 | Pago iniciado, esperando confirmación |
| Completado | 2 | Pago exitoso y confirmado |
| Fallido | 3 | Pago rechazado o error |
| Cancelado | 4 | Pago anulado por admin |
| Reembolsado | 5 | Pago devuelto al cliente |

---

## ?? Tips de Prueba

1. **Siempre usa montos mayores a $1.00**
2. **Para PayPal, usa la cuenta Personal sandbox como comprador**
3. **Guarda los OrderId para pruebas de captura**
4. **Verifica los logs en la consola del backend**
5. **Revisa las transacciones en el dashboard de PayPal Sandbox**

---

**Última Actualización**: Enero 2024  
**Versión**: 1.0
