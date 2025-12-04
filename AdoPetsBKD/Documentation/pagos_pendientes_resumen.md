# ?? Resumen Ejecutivo - Implementación de Pagos Pendientes

## ? Lo que se implementó

Se agregaron **5 nuevos endpoints** al sistema de pagos para gestionar pagos pendientes de citas:

### 1. **GET** `/api/v1/Pagos/cita/{citaId}`
- **Propósito:** Ver todos los pagos de una cita específica
- **Uso:** Verificar cuánto se ha pagado y cuánto falta
- **Respuesta:** Lista de pagos con detalles (monto, método, estado)

### 2. **GET** `/api/v1/Pagos/pendientes`
- **Propósito:** Ver TODAS las citas con pagos pendientes
- **Roles:** Admin, Veterinario, Recepcionista
- **Uso:** Dashboard de recepción para ver qué citas tienen saldo pendiente

### 3. **GET** `/api/v1/Pagos/pendientes/usuario/{usuarioId}`
- **Propósito:** Ver pagos pendientes de un usuario específico
- **Uso:** Mostrar al usuario sus propias citas con saldo pendiente
- **Seguridad:** El usuario solo puede ver sus propios pagos

### 4. **POST** `/api/v1/Pagos/completar-pago`
- **Propósito:** Registrar el pago del saldo restante (efectivo, tarjeta)
- **Roles:** Admin, Veterinario, Recepcionista
- **Uso:** Cuando el cliente paga en recepción el día de la cita

### 5. **POST** `/api/v1/Pagos/completar-pago/paypal`
- **Propósito:** Crear orden PayPal para pagar el saldo restante
- **Uso:** Cuando el cliente quiere pagar el resto por PayPal desde la app

---

## ?? DTOs Agregados

### `PagoPendienteDto`
Muestra información resumida de una cita con pago pendiente:
```csharp
{
    CitaId,
    NumeroCita,
    FechaCita,
    NombreMascota,
    NombrePropietario,
    ServicioDescripcion,
    
    // Info del anticipo (si existe)
    PagoAnticipoId,
    MontoAnticipoPagado,
    FechaPagoAnticipo,
    
    // Info de saldo
    MontoTotal,
    MontoPendiente,
    PorcentajePagado,
    
    // Estado
    TieneAnticipoPagado,
    EstadoPago  // "Anticipo Pagado (50%)" o "Pago Pendiente (100%)"
}
```

### `CompletarPagoRestanteDto`
Para registrar pago en efectivo/tarjeta:
```csharp
{
    CitaId,
    PagoAnticipoId,
    MetodoPago,  // 1=PayPal, 2=Efectivo, 3=TarjetaDebito, 4=TarjetaCredito, 5=Transferencia
    Referencia,
    Notas
}
```

### `CompletarPagoRestantePayPalDto`
Para crear orden PayPal del saldo:
```csharp
{
    CitaId,
    PagoAnticipoId,
    ReturnUrl,
    CancelUrl
}
```

---

## ?? Flujo de Pagos

### Cita Digital (Online)
```
1. Usuario solicita cita ? Paga 50% anticipo con PayPal
2. Solicitud confirmada ? Cita creada con 50% pagado
3. Día de la cita ? Pagar saldo 50% (efectivo o PayPal)
4. Pago completo ? Cita finalizada
```

### Cita Presencial (Walk-in)
```
1. Recepcionista crea cita ? Sin anticipo
2. Día de la cita ? Pagar 100% (efectivo, tarjeta o PayPal)
3. Pago completo ? Cita finalizada
```

---

## ?? Información Calculada

El sistema ahora calcula automáticamente:

- **MontoRestante:** Cuánto falta por pagar
- **PorcentajePagado:** Qué % del total se ha pagado
- **EstadoPago:** 
  - `"Anticipo Pagado (50%)"` ? Cita digital con anticipo
  - `"Pago Pendiente (100%)"` ? Cita sin pago
  - `"Pagado Completo"` ? Ya se pagó todo

---

## ?? Seguridad

- ? Usuarios solo pueden ver sus propios pagos pendientes
- ? Admin/Veterinario/Recepcionista pueden ver todos
- ? Solo personal autorizado puede completar pagos
- ? Validación de que el pago anticipo exista antes de completar

---

## ?? Pendiente de Implementar en el Servicio

**IMPORTANTE:** Los endpoints están creados en el controlador, pero **falta implementar la lógica en el servicio** `IPagoService`:

```csharp
// Estos métodos están declarados pero NO implementados:
Task<List<PagoDto>> GetPagosByCitaIdAsync(Guid citaId);
Task<List<PagoPendienteDto>> GetPagosPendientesAsync();
Task<List<PagoPendienteDto>> GetPagosPendientesByUsuarioAsync(Guid usuarioId);
Task<PagoDto> CompletarPagoRestanteAsync(CompletarPagoRestanteDto dto, Guid userId);
```

### Próximos pasos:

1. **Crear `PagoRepository`** con métodos para consultar pagos
2. **Implementar la lógica en `PagoService`**:
   - Consultar pagos por cita
   - Calcular montos pendientes
   - Validar que no se dupliquen pagos
   - Crear pago complementario
3. **Agregar tests unitarios**

---

## ?? Casos de Uso del Frontend

### 1. Usuario ve sus pagos pendientes
```javascript
GET /api/v1/Pagos/pendientes/usuario/{usuarioId}

// Respuesta: Lista de citas con saldo pendiente
// UI: Mostrar badge "Tienes 2 pagos pendientes"
```

### 2. Dashboard de recepción
```javascript
GET /api/v1/Pagos/pendientes

// Respuesta: Todas las citas con pagos pendientes
// UI: Tabla con próximas citas y sus saldos
```

### 3. Cobrar en recepción
```javascript
// 1. Buscar cita por ID
GET /api/v1/Citas/{citaId}

// 2. Ver pagos de esa cita
GET /api/v1/Pagos/cita/{citaId}

// 3. Registrar pago
POST /api/v1/Pagos/completar-pago
{
  citaId: "...",
  metodoPago: 2, // Efectivo
  referencia: "RECIBO-001234"
}
```

### 4. Usuario paga desde app
```javascript
// 1. Ver citas con saldo pendiente
GET /api/v1/Pagos/pendientes/usuario/{userId}

// 2. Click "Pagar con PayPal"
POST /api/v1/Pagos/completar-pago/paypal
{
  citaId: "...",
  returnUrl: "adopets://payment/success"
}

// 3. Abrir WebView con approvalUrl
// 4. Capturar pago al regresar
POST /api/v1/Pagos/paypal/capture
```

---

## ?? Archivos Modificados

1. **`IPagoService.cs`** - Agregados 4 métodos nuevos
2. **`PagoDtos.cs`** - Agregados 3 DTOs nuevos
3. **`PagosController.cs`** - Agregados 5 endpoints nuevos

---

## ?? Documentación

Se creó la guía completa para frontend en:
```
AdoPetsBKD/Documentation/pagos_pendientes_api.md
```

Incluye:
- ? Descripción de cada endpoint
- ? Ejemplos de request/response
- ? Casos de uso con código
- ? Manejo de errores
- ? Ejemplos en JavaScript/TypeScript
- ? Ejemplos en React/React Native

---

## ?? Conclusión

**SÍ es posible ver los pagos pendientes** de las citas. Se han creado los endpoints necesarios para:

1. ? Ver pagos de una cita específica
2. ? Ver todas las citas con pagos pendientes
3. ? Filtrar por usuario
4. ? Completar el pago restante (efectivo/tarjeta/PayPal)

**Lo único que falta es implementar la lógica de negocio en el servicio.** Los endpoints están listos y documentados.

---

## ?? Para Continuar

1. Implementar los métodos en `PagoService`
2. Crear `IPagoRepository` con métodos de consulta
3. Agregar tests unitarios
4. El frontend puede empezar a consumir los endpoints

---

*Documento generado: Enero 2025*  
*Versión: 1.0*
