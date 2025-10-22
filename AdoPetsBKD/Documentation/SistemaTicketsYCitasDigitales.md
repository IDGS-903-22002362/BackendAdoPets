# Sistema de Tickets y Citas Digitales - AdoPets

## Descripci�n General

Se han agregado tres m�dulos principales al sistema AdoPets:

1. **Sistema de Tickets** - Para generar recibos de procedimientos m�dicos
2. **Citas Digitales** - Para que adoptantes soliciten citas online
3. **Sistema de Pagos** - Integraci�n con PayPal para pagos y anticipos

---

## ?? 1. Sistema de Tickets

### Funcionalidad
Permite generar tickets/recibos detallados de procedimientos m�dicos realizados en la cl�nica veterinaria.

### Caracter�sticas
- ? Generaci�n autom�tica de n�mero de ticket (TK-YYYYMMDD-XXXX)
- ? Desglose detallado de costos (procedimiento, insumos, adicionales)
- ? C�lculo autom�tico de IVA (16% M�xico)
- ? Registro de diagn�stico y tratamiento
- ? Medicaci�n prescrita
- ? Relaci�n con cita y pago
- ? Estado del ticket (Generado, Entregado, Cancelado, Reimpreso)
- ? Detalles itemizados del ticket

### Endpoints API

#### Crear Ticket
```http
POST /api/tickets
Authorization: Bearer {token}
Content-Type: application/json

{
  "citaId": "guid",
  "mascotaId": "guid",
  "clienteId": "guid",
  "veterinarioId": "guid",
  "fechaProcedimiento": "2024-01-15T10:00:00Z",
  "nombreProcedimiento": "Cirug�a de Esterilizaci�n",
  "descripcionProcedimiento": "Ovariohisterectom�a completa",
  "costoProcedimiento": 1500.00,
  "costoInsumos": 300.00,
  "costoAdicional": 200.00,
  "descuento": 100.00,
  "diagnostico": "Paciente sano para cirug�a",
  "tratamiento": "Cuidados postoperatorios est�ndar",
  "medicacionPrescrita": "Meloxicam 0.1mg/kg cada 24h por 5 d�as",
  "detalles": [
    {
      "descripcion": "Anestesia general",
      "cantidad": 1,
      "unidad": "aplicaci�n",
      "precioUnitario": 500.00,
      "tipo": 1
    },
    {
      "descripcion": "Antibi�tico profil�ctico",
      "cantidad": 2,
      "unidad": "dosis",
      "precioUnitario": 150.00,
      "itemInventarioId": "guid",
      "tipo": 3
    }
  ]
}
```

#### Obtener Ticket por ID
```http
GET /api/tickets/{id}
Authorization: Bearer {token}
```

#### Obtener Tickets de un Cliente
```http
GET /api/tickets/cliente/{clienteId}
Authorization: Bearer {token}
```

#### Marcar Ticket como Entregado
```http
PUT /api/tickets/{id}/entregar
Authorization: Bearer {token}
```

#### Generar PDF del Ticket
```http
GET /api/tickets/{id}/pdf
Authorization: Bearer {token}
```
*Nota: La generaci�n de PDF est� pendiente de implementar con QuestPDF o similar*

---

## ?? 2. Sistema de Citas Digitales

### Funcionalidad
Permite a los adoptantes solicitar citas de forma digital, con verificaci�n de disponibilidad autom�tica y confirmaci�n del personal.

### Flujo del Proceso

1. **Solicitante crea solicitud** ? Estado: `Pendiente` o `PendientePago`
2. **Verificaci�n de disponibilidad autom�tica**
3. **Personal revisa solicitud** ? Estado: `EnRevision`
4. **Si se aprueba** ? Solicitante debe pagar 50% de anticipo
5. **Pago recibido** ? Estado: `PagadaPendienteConfirmacion`
6. **Personal confirma y crea cita** ? Estado: `Confirmada`

### Caracter�sticas
- ? Verificaci�n autom�tica de disponibilidad (horarios del veterinario y sala)
- ? Detecci�n de conflictos de citas
- ? C�lculo autom�tico de anticipo (50% del costo total)
- ? Vinculaci�n con sistema de pagos
- ? M�ltiples estados de seguimiento
- ? Preferencias de veterinario y sala
- ? Generaci�n autom�tica de n�mero de solicitud (SC-YYYYMMDD-XXXX)

### Endpoints API

#### Crear Solicitud de Cita
```http
POST /api/solicitudescitasdigitales
Authorization: Bearer {token}
Content-Type: application/json

{
  "solicitanteId": "guid",
  "mascotaId": "guid",
  "nombreMascota": "Firulais",
  "especieMascota": "Perro",
  "razaMascota": "Labrador",
  "servicioId": "guid",
  "descripcionServicio": "Consulta general",
  "motivoConsulta": "Revisi�n de rutina",
  "fechaHoraSolicitada": "2024-01-20T14:00:00Z",
  "duracionEstimadaMin": 60,
  "veterinarioPreferidoId": "guid",
  "costoEstimado": 500.00
}
```

#### Verificar Disponibilidad
```http
POST /api/solicitudescitasdigitales/verificar-disponibilidad
Authorization: Bearer {token}
Content-Type: application/json

{
  "fechaHoraInicio": "2024-01-20T14:00:00Z",
  "duracionMin": 60,
  "veterinarioId": "guid",
  "salaId": "guid"
}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "disponible": true,
    "mensaje": null,
    "conflictos": []
  }
}
```

#### Obtener Solicitudes Pendientes (Personal)
```http
GET /api/solicitudescitasdigitales/pendientes
Authorization: Bearer {token}
```

#### Confirmar Solicitud
```http
POST /api/solicitudescitasdigitales/confirmar
Authorization: Bearer {token}
Content-Type: application/json

{
  "solicitudId": "guid",
  "confirmadoPorId": "guid",
  "veterinarioId": "guid",
  "salaId": "guid",
  "fechaHoraConfirmada": "2024-01-20T14:00:00Z",
  "duracionMin": 60
}
```

#### Rechazar Solicitud
```http
POST /api/solicitudescitasdigitales/rechazar
Authorization: Bearer {token}
Content-Type: application/json

{
  "solicitudId": "guid",
  "rechazadoPorId": "guid",
  "motivo": "No hay disponibilidad en esa fecha"
}
```

---

## ?? 3. Sistema de Pagos (PayPal)

### Funcionalidad
Gesti�n de pagos con integraci�n a PayPal para anticipos de citas y pagos completos de procedimientos.

### Caracter�sticas
- ? Soporte para pagos completos y anticipos
- ? Integraci�n con PayPal (crear orden, capturar pago)
- ? Webhooks de PayPal para actualizaci�n autom�tica
- ? Soporte para m�ltiples m�todos de pago (PayPal, Efectivo, Tarjeta, Transferencia)
- ? C�lculo autom�tico de monto restante en anticipos
- ? Generaci�n de n�mero de pago (PAY-YYYYMMDD-XXXX)
- ? Estados: Pendiente, Procesando, Completado, Fallido, Cancelado, Reembolsado

### Tipos de Pago
- **PagoCompleto**: Pago total del servicio
- **Anticipo**: Pago parcial (t�picamente 50%)
- **PagoComplementario**: Pago del monto restante
- **Reembolso**: Devoluci�n de pago

### Endpoints API

#### Crear Orden de PayPal
```http
POST /api/pagos/paypal/create-order
Authorization: Bearer {token}
Content-Type: application/json

{
  "usuarioId": "guid",
  "monto": 250.00,
  "concepto": "Anticipo para cita - Consulta General",
  "solicitudCitaId": "guid",
  "esAnticipo": true,
  "montoTotal": 500.00,
  "returnUrl": "https://miapp.com/pago/success",
  "cancelUrl": "https://miapp.com/pago/cancel"
}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "orderId": "PAYPAL-ORDER-12345",
    "approvalUrl": "https://www.paypal.com/checkoutnow?token=PAYPAL-ORDER-12345",
    "status": "CREATED"
  }
}
```

#### Capturar Pago de PayPal
```http
POST /api/pagos/paypal/capture
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderId": "PAYPAL-ORDER-12345"
}
```

#### Obtener Pagos de un Usuario
```http
GET /api/pagos/usuario/{usuarioId}
Authorization: Bearer {token}
```

#### Webhook de PayPal
```http
POST /api/pagos/webhook/paypal
Content-Type: application/json

{
  "eventType": "PAYMENT.CAPTURE.COMPLETED",
  "eventId": "WH-EVENT-123",
  "resource": {
    // Datos del evento de PayPal
  }
}
```

---

## ??? Estructura de Base de Datos

### Nuevas Tablas

#### Tickets
- `Id` (PK)
- `NumeroTicket` (Unique, indexed)
- `CitaId` (FK ? Citas)
- `MascotaId` (FK ? Mascotas, nullable)
- `ClienteId` (FK ? Usuarios)
- `VeterinarioId` (FK ? Usuarios)
- `FechaProcedimiento`
- `NombreProcedimiento`
- `DescripcionProcedimiento`
- `CostoProcedimiento`, `CostoInsumos`, `CostoAdicional`
- `Subtotal`, `Descuento`, `IVA`, `Total`
- `Observaciones`, `Diagnostico`, `Tratamiento`, `MedicacionPrescrita`
- `Estado` (Generado, Entregado, Cancelado, Reimpreso)
- `FechaEntrega`, `EntregadoPorId`
- `PagoId` (FK ? Pagos, nullable)
- Campos de auditor�a

#### TicketDetalles
- `Id` (PK)
- `TicketId` (FK ? Tickets)
- `Descripcion`
- `Cantidad`, `Unidad`
- `PrecioUnitario`, `Subtotal`
- `ItemInventarioId` (FK ? ItemsInventario, nullable)
- `Tipo` (Procedimiento, Insumo, Medicamento, Consulta, Otro)

#### Pagos
- `Id` (PK)
- `NumeroPago` (Unique, indexed)
- `UsuarioId` (FK ? Usuarios, nullable)
- `Monto`, `Moneda`
- `Tipo` (PagoCompleto, Anticipo, PagoComplementario, Reembolso)
- `Metodo` (PayPal, Efectivo, TarjetaDebito, TarjetaCredito, Transferencia)
- `Estado` (Pendiente, Procesando, Completado, Fallido, Cancelado, Reembolsado)
- `PayPalOrderId`, `PayPalCaptureId`, `PayPalPayerId`, `PayPalPayerEmail`, `PayPalPayerName`
- `FechaPago`, `FechaConfirmacion`, `FechaCancelacion`
- `Concepto`, `Referencia`, `Notas`
- `CitaId` (FK ? Citas, nullable)
- `TicketId` (FK ? Tickets, nullable)
- `EsAnticipo`, `MontoTotal`, `MontoRestante`
- `PagoPrincipalId` (FK ? Pagos, self-reference para pagos complementarios)
- Campos de auditor�a

#### SolicitudesCitasDigitales
- `Id` (PK)
- `NumeroSolicitud` (Unique, indexed)
- `SolicitanteId` (FK ? Usuarios)
- `MascotaId` (FK ? Mascotas, nullable)
- `NombreMascota`, `EspecieMascota`, `RazaMascota`
- `ServicioId` (FK ? Servicios, nullable)
- `DescripcionServicio`, `MotivoConsulta`
- `FechaHoraSolicitada`, `DuracionEstimadaMin`
- `VeterinarioPreferidoId` (FK ? Usuarios, nullable)
- `SalaPreferidaId` (FK ? Salas, nullable)
- `CostoEstimado`, `MontoAnticipo`
- `Estado` (Pendiente, EnRevision, PendientePago, PagadaPendienteConfirmacion, Confirmada, Rechazada, Cancelada, Expirada)
- `FechaSolicitud`, `FechaRevision`, `FechaConfirmacion`, `FechaRechazo`
- `RevisadoPorId` (FK ? Usuarios, nullable)
- `MotivoRechazo`, `NotasInternas`
- `PagoAnticipoId` (FK ? Pagos, nullable)
- `CitaId` (FK ? Citas, nullable)
- `DisponibilidadVerificada`, `FechaVerificacionDisponibilidad`
- Campos de auditor�a

---

## ?? Flujo Completo de una Cita Digital

### 1. Solicitante crea solicitud
```javascript
// Frontend env�a solicitud
POST /api/solicitudescitasdigitales
{
  "solicitanteId": "abc123",
  "nombreMascota": "Firulais",
  "especieMascota": "Perro",
  "servicioId": "def456",
  "descripcionServicio": "Esterilizaci�n",
  "fechaHoraSolicitada": "2024-01-20T10:00:00Z",
  "duracionEstimadaMin": 120,
  "costoEstimado": 1500.00
}
```

### 2. Sistema verifica disponibilidad autom�ticamente
- Verifica conflictos con otras citas
- Verifica horario del veterinario (si se especific�)
- Verifica disponibilidad de sala (si se especific�)

### 3. Si hay disponibilidad ? Estado: PendientePago
```javascript
// Solicitante recibe respuesta
{
  "numeroSolicitud": "SC-20240115-1234",
  "estado": 3, // PendientePago
  "montoAnticipo": 750.00 // 50% de 1500
}
```

### 4. Solicitante realiza pago de anticipo
```javascript
// 1. Crear orden de PayPal
POST /api/pagos/paypal/create-order
{
  "usuarioId": "abc123",
  "monto": 750.00,
  "concepto": "Anticipo - Esterilizaci�n",
  "solicitudCitaId": "guid-solicitud",
  "esAnticipo": true,
  "montoTotal": 1500.00,
  "returnUrl": "https://miapp.com/success",
  "cancelUrl": "https://miapp.com/cancel"
}

// 2. Redirigir a approvalUrl de PayPal
// 3. Usuario aprueba en PayPal
// 4. Capturar pago
POST /api/pagos/paypal/capture
{
  "orderId": "PAYPAL-ORDER-123"
}
```

### 5. Estado cambia a: PagadaPendienteConfirmacion

### 6. Personal confirma la solicitud
```javascript
POST /api/solicitudescitasdigitales/confirmar
{
  "solicitudId": "guid-solicitud",
  "confirmadoPorId": "guid-personal",
  "veterinarioId": "guid-veterinario",
  "salaId": "guid-sala",
  "fechaHoraConfirmada": "2024-01-20T10:00:00Z",
  "duracionMin": 120
}
```

### 7. Sistema crea la cita autom�ticamente
- Crea registro en tabla `Citas`
- Vincula con la solicitud
- Estado de solicitud ? Confirmada

### 8. D�a de la cita
- Veterinario realiza el procedimiento
- Se genera ticket con costos reales

```javascript
POST /api/tickets
{
  "citaId": "guid-cita",
  "clienteId": "abc123",
  "veterinarioId": "guid-veterinario",
  "nombreProcedimiento": "Esterilizaci�n",
  "costoProcedimiento": 1200.00,
  "costoInsumos": 250.00,
  "costoAdicional": 50.00,
  // Total: 1740.00 (con IVA)
}
```

### 9. Cliente paga monto restante
```javascript
// Monto restante = 1740.00 - 750.00 (anticipo) = 990.00
POST /api/pagos
{
  "usuarioId": "abc123",
  "monto": 990.00,
  "tipo": 3, // PagoComplementario
  "ticketId": "guid-ticket",
  "metodo": 2 // Efectivo
}
```

---

## ?? Configuraci�n Pendiente

### PayPal SDK
Para usar la integraci�n real de PayPal, es necesario:

1. **Instalar paquete NuGet:**
```bash
dotnet add package PayPalCheckoutSdk
```

2. **Configurar credenciales en `appsettings.json`:**
```json
{
  "PayPal": {
    "Mode": "sandbox", // o "live"
    "ClientId": "tu-client-id",
    "ClientSecret": "tu-client-secret"
  }
}
```

3. **Descomentar c�digo en `PagoService.cs`** (l�neas marcadas con `// TODO`)

### Generaci�n de PDF
Para generar PDFs de tickets:

1. **Instalar QuestPDF:**
```bash
dotnet add package QuestPDF
```

2. **Implementar generaci�n en `TicketService.GenerarPdfTicketAsync()`**

---

## ?? Testing

### Datos de Prueba

```sql
-- Crear servicio de ejemplo
INSERT INTO Servicios (Id, Descripcion, DuracionMinDefault, PrecioSugerido, Categoria, Activo)
VALUES (NEWID(), 'Esterilizaci�n Canina', 120, 1500.00, 2, 1);

-- Crear sala
INSERT INTO Salas (Id, Nombre, Descripcion, Capacidad, Activa)
VALUES (NEWID(), 'Quir�fano 1', 'Sala de cirug�as', 1, 1);
```

### Ejemplos de Uso con cURL

```bash
# Crear solicitud de cita
curl -X POST "https://localhost:5001/api/solicitudescitasdigitales" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "solicitanteId": "guid",
    "nombreMascota": "Firulais",
    "especieMascota": "Perro",
    "descripcionServicio": "Esterilizaci�n",
    "fechaHoraSolicitada": "2024-01-20T10:00:00Z",
    "duracionEstimadaMin": 120,
    "costoEstimado": 1500.00
  }'
```

---

## ?? Notas Importantes

1. **Autenticaci�n JWT**: Los controladores tienen `[Authorize]`, necesitas configurar JWT y pasar el token en el header `Authorization: Bearer {token}`

2. **IDs de Usuario**: Actualmente se usa un GUID temporal. Debes reemplazar con el ID del usuario autenticado obtenido del token JWT:
```csharp
// En los controladores
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

3. **Webhooks de PayPal**: El endpoint `/api/pagos/webhook/paypal` debe ser configurado en el dashboard de PayPal con la URL p�blica del servidor.

4. **Idempotencia**: Los webhooks de PayPal usan `EventId` para prevenir procesamiento duplicado.

5. **Moneda**: El sistema est� configurado para MXN (pesos mexicanos) por defecto.

---

## ?? Pr�ximos Pasos

1. ? Migraci�n de base de datos creada
2. ? Implementar autenticaci�n JWT completa
3. ? Configurar integraci�n real con PayPal
4. ? Implementar generaci�n de PDF para tickets
5. ? Agregar notificaciones por email/SMS
6. ? Dashboard para el personal
7. ? Tests unitarios e integraci�n

---

## ?? Soporte

Para dudas o problemas contactar al equipo de desarrollo.

**Versi�n:** 1.0.0  
**Fecha:** 2024  
**Proyecto:** AdoPets Backend  
