# ?? Documentación API - Sistema Completo de Citas para Usuarios

## ?? Índice
1. [Introducción](#introducción)
2. [Autenticación](#autenticación)
3. [Flujo Completo del Usuario](#flujo-completo-del-usuario)
4. [Endpoints de Mascotas de Usuario](#endpoints-de-mascotas-de-usuario)
5. [Endpoints de Solicitudes de Citas](#endpoints-de-solicitudes-de-citas)
6. [Endpoints de Pagos](#endpoints-de-pagos)
7. [Modelos de Datos](#modelos-de-datos)
8. [Estados y Flujos](#estados-y-flujos)
9. [Ejemplos Completos](#ejemplos-completos)
10. [Errores Comunes](#errores-comunes)

---

## ?? Introducción

Este documento describe el **sistema completo end-to-end** para que los usuarios puedan:

? **Registrar sus propias mascotas** (no del refugio)  
? **Solicitar citas veterinarias** para sus mascotas  
? **Verificar disponibilidad** de veterinarios y salas  
? **Pagar anticipo del 50%** mediante PayPal  
? **Seguimiento en tiempo real** del estado de solicitudes  
? **Gestionar historial** de citas y pagos

**URL Base:** `https://api.adopets.com`  
**Versión:** 1.0  
**Última Actualización:** Diciembre 2024

---

## ?? Autenticación

Todos los endpoints requieren **JWT Bearer Token**.

### Login

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "contraseña123"
}
```

**Respuesta (200 OK):**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nombreCompleto": "Juan Pérez López",
      "email": "usuario@ejemplo.com",
      "roles": ["Adoptante"]
    }
  }
}
```

### Usar Token

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? Flujo Completo del Usuario

```
1. Usuario se registra/inicia sesión
   ?
2. ¿Tiene mascota registrada?
   No ? Registrar mascota propia (/api/MisMascotas)
   Sí ? Continuar
   ?
3. Consultar servicios disponibles (/api/Servicios)
   ?
4. Verificar disponibilidad (/api/SolicitudesCitasDigitales/verificar-disponibilidad)
   ?
5. Crear solicitud de cita (/api/SolicitudesCitasDigitales)
   ?
6. Sistema calcula anticipo 50%
   ?
7. Crear orden de pago PayPal (/api/Pagos/paypal/create-order)
   ?
8. Usuario paga en PayPal
   ?
9. Webhook confirma pago (automático)
   ?
10. Solicitud ? Estado: PagadaPendienteConfirmacion
   ?
11. Personal revisa y confirma (/api/SolicitudesCitasDigitales/confirmar)
   ?
12. Cita creada y programada ?
   ?
13. Usuario asiste a cita
   ?
14. Pago del 50% restante (en clínica)
```

---

## ?? Endpoints de Mascotas de Usuario

**Base Path:** `/api/MisMascotas`

### 1. Obtener Mis Mascotas

```http
GET /api/MisMascotas
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "nombre": "Firulais",
      "especie": "Perro",
      "raza": "Golden Retriever",
      "edadEnAnios": 4,
      "fotos": [...]
    }
  ]
}
```

---

### 2. Registrar Nueva Mascota

```http
POST /api/MisMascotas
Authorization: Bearer {token}
Content-Type: application/json

{
  "nombre": "Luna",
  "especie": "Gato",
  "raza": "Persa",
  "fechaNacimiento": "2021-03-20",
  "sexo": 2,
  "estadoSalud": "Saludable"
}
```

**Campos:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| nombre | string(100) | ? | Nombre de la mascota |
| especie | string(50) | ? | Perro, Gato, etc. |
| raza | string(100) | ? | Raza específica |
| fechaNacimiento | date | ? | Fecha de nacimiento |
| sexo | int | ? | 1=Macho, 2=Hembra, 3=Desconocido |
| estadoSalud | string(500) | ? | Estado de salud |

---

### 3. Agregar Fotos

```http
POST /api/MisMascotas/{id}/fotos
Authorization: Bearer {token}
Content-Type: application/json

[
  {
    "storageKey": "data:image/jpeg;base64,/9j/4AAQSkZJRg...",
    "mimeType": "image/jpeg",
    "esPrincipal": true
  }
]
```

**Características:**
- ? Acepta Base64 o URL
- ? Redimensionamiento automático (max 1600px)
- ? Compresión JPEG 75%
- ? Primera foto = principal

---

## ?? Endpoints de Solicitudes de Citas

**Base Path:** `/api/SolicitudesCitasDigitales`

### 1. Verificar Disponibilidad

```http
POST /api/SolicitudesCitasDigitales/verificar-disponibilidad
Authorization: Bearer {token}
Content-Type: application/json

{
  "fechaHoraInicio": "2025-01-15T10:00:00Z",
  "duracionMin": 60,
  "veterinarioId": "a1b2c3d4-...",
  "salaId": "f1e2d3c4-..."
}
```

**Respuesta - Disponible:**
```json
{
  "success": true,
  "data": {
    "disponible": true,
    "conflictos": []
  }
}
```

**Respuesta - NO Disponible:**
```json
{
  "success": true,
  "data": {
    "disponible": false,
    "mensaje": "Existen conflictos de horario",
    "conflictos": [
      {
        "tipo": "Veterinario",
        "horaInicio": "2025-01-15T09:30:00Z",
        "horaFin": "2025-01-15T10:30:00Z",
        "descripcion": "Cita programada de 09:30 a 10:30"
      }
    ]
  }
}
```

---

### 2. Crear Solicitud de Cita

```http
POST /api/SolicitudesCitasDigitales
Authorization: Bearer {token}
Content-Type: application/json

{
  "solicitanteId": "3fa85f64-...",
  "mascotaId": "7c9e6679-...",
  "nombreMascota": "Firulais",
  "especieMascota": "Perro",
  "servicioId": "1a2b3c4d-...",
  "descripcionServicio": "Vacunación anual",
  "motivoConsulta": "Control anual",
  "fechaHoraSolicitada": "2025-01-15T10:00:00Z",
  "duracionEstimadaMin": 60,
  "veterinarioPreferidoId": "a1b2c3d4-...",
  "costoEstimado": 800.00
}
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Solicitud creada exitosamente",
  "data": {
    "id": "9f8e7d6c-...",
    "numeroSolicitud": "SC-20250115-4523",
    "costoEstimado": 800.00,
    "montoAnticipo": 400.00,  // ? 50% automático
    "estado": 3,
    "estadoNombre": "PendientePago",
    "disponibilidadVerificada": true
  }
}
```

---

### 3. Obtener Mis Solicitudes

```http
GET /api/SolicitudesCitasDigitales/usuario/{usuarioId}
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "id": "9f8e7d6c-...",
      "numeroSolicitud": "SC-20250115-4523",
      "nombreMascota": "Firulais",
      "descripcionServicio": "Vacunación anual",
      "fechaHoraSolicitada": "2025-01-15T10:00:00Z",
      "costoEstimado": 800.00,
      "montoAnticipo": 400.00,
      "estado": 4,
      "estadoNombre": "PagadaPendienteConfirmacion"
    }
  ]
}
```

---

### 4. Cancelar Solicitud

```http
PUT /api/SolicitudesCitasDigitales/{id}/cancelar
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "message": "Solicitud cancelada",
  "data": {
    "estado": 7,
    "estadoNombre": "Cancelada"
  }
}
```

---

## ?? Endpoints de Pagos

**Base Path:** `/api/Pagos`

### 1. Crear Orden PayPal

```http
POST /api/Pagos/paypal/create-order
Authorization: Bearer {token}
Content-Type: application/json

{
  "solicitudCitaId": "9f8e7d6c-...",
  "monto": 400.00,
  "concepto": "Anticipo 50% - SC-20250115-4523",
  "returnUrl": "https://app.adopets.com/citas/pago-exitoso",
  "cancelUrl": "https://app.adopets.com/citas/pago-cancelado"
}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "orderId": "8AB12345CD67890E",
    "approvalUrl": "https://www.paypal.com/checkoutnow?token=8AB...",
    "pagoId": "d4e5f6a7-..."
  }
}
```

**Flujo:**
1. Usuario recibe `approvalUrl`
2. Redirige a PayPal
3. Inicia sesión y aprueba
4. PayPal redirige a `returnUrl`
5. Frontend captura el pago

---

### 2. Capturar Pago

```http
POST /api/Pagos/paypal/capture-order
Authorization: Bearer {token}
Content-Type: application/json

{
  "orderId": "8AB12345CD67890E",
  "pagoId": "d4e5f6a7-..."
}
```

**Respuesta:**
```json
{
  "success": true,
  "data": {
    "pagoId": "d4e5f6a7-...",
    "estado": 2,
    "estadoNombre": "Completado",
    "monto": 400.00,
    "payPalCaptureId": "9BC23456...",
    "payPalPayerEmail": "usuario@ejemplo.com"
  }
}
```

**Automático tras captura:**
- ? Estado pago ? "Completado"
- ? Vincula pago a solicitud
- ? Solicitud ? "PagadaPendienteConfirmacion"

---

### 3. Historial de Pagos

```http
GET /api/Pagos/usuario/{usuarioId}
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "success": true,
  "data": [
    {
      "id": "d4e5f6a7-...",
      "numeroPago": "PAG-20241215-7890",
      "concepto": "Anticipo 50% - SC-20250115-4523",
      "monto": 400.00,
      "estado": 2,
      "estadoNombre": "Completado",
      "fechaPago": "2024-12-15T10:15:00Z",
      "esAnticipo": true,
      "montoTotal": 800.00,
      "montoRestante": 400.00
    }
  ]
}
```

---

## ?? Modelos de Datos

### Mascota de Usuario

```typescript
interface MascotaUsuario {
  id: string;                 // UUID
  nombre: string;             // max 100
  especie: string;            // max 50
  raza?: string;              // max 100
  fechaNacimiento?: Date;
  sexo: 1 | 2 | 3;           // Macho | Hembra | Desconocido
  personalidad?: string;      // max 500
  estadoSalud?: string;       // max 500
  notas?: string;             // max 2000
  propietarioId: string;      // UUID del usuario
  edadEnAnios?: number;       // Calculado
  fotos: MascotaFoto[];
  createdAt: Date;
  updatedAt?: Date;
}
```

### Solicitud de Cita

```typescript
interface SolicitudCitaDigital {
  id: string;
  numeroSolicitud: string;           // SC-YYYYMMDD-XXXX
  solicitanteId: string;
  nombreSolicitante: string;
  mascotaId?: string;
  nombreMascota: string;
  servicioId?: string;
  descripcionServicio: string;
  fechaHoraSolicitada: Date;
  costoEstimado: number;
  montoAnticipo: number;             // 50% automático
  estado: 1-8;                       // Ver tabla estados
  estadoNombre: string;
  pagoAnticipoId?: string;
  citaId?: string;
  disponibilidadVerificada: boolean;
}
```

### Pago

```typescript
interface Pago {
  id: string;
  numeroPago: string;                // PAG-YYYYMMDD-XXXX
  usuarioId: string;
  monto: number;
  moneda: string;                    // "MXN"
  tipo: 1;                           // CitaVeterinaria
  metodo: 2;                         // PayPal
  estado: 1-5;                       // Ver tabla estados
  payPalOrderId?: string;
  payPalCaptureId?: string;
  esAnticipo: boolean;               // true para 50%
  montoTotal?: number;               // Total servicio
  montoRestante?: number;            // 50% pendiente
}
```

---

## ?? Estados y Flujos

### Estados de Solicitud

| Código | Nombre | Usuario puede | Personal puede |
|--------|--------|---------------|----------------|
| 1 | Pendiente | Cancelar | Revisar, Rechazar |
| 2 | EnRevision | Cancelar | Confirmar, Rechazar |
| 3 | PendientePago | Pagar, Cancelar | Ver |
| 4 | PagadaPendienteConfirmacion | Ver | Confirmar |
| 5 | Confirmada | Ver cita | Gestionar |
| 6 | Rechazada | Ver motivo | - |
| 7 | Cancelada | - | - |
| 8 | Expirada | - | - |

### Flujo de Estados

```
Pendiente (1)
    ?
EnRevision (2)
    ?
PendientePago (3)
    ? [Usuario paga]
PagadaPendienteConfirmacion (4)
    ? [Personal confirma]
Confirmada (5) ?
```

---

## ?? Ejemplo Completo

```javascript
// 1. Login
const loginRes = await fetch('/api/Auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'juan@mail.com', password: 'pass123' })
});
const { data: { token, usuario } } = await loginRes.json();

// 2. Registrar mascota
const mascotaRes = await fetch('/api/MisMascotas', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    nombre: 'Max',
    especie: 'Perro',
    sexo: 1
  })
});
const { data: mascota } = await mascotaRes.json();

// 3. Verificar disponibilidad
const disponibilidadRes = await fetch('/api/SolicitudesCitasDigitales/verificar-disponibilidad', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    fechaHoraInicio: '2025-01-20T10:00:00Z',
    duracionMin: 60,
    veterinarioId: 'vet-id'
  })
});
const { data: disponibilidad } = await disponibilidadRes.json();

if (!disponibilidad.disponible) {
  console.error('No disponible:', disponibilidad.conflictos);
  return;
}

// 4. Crear solicitud
const solicitudRes = await fetch('/api/SolicitudesCitasDigitales', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    solicitanteId: usuario.id,
    mascotaId: mascota.id,
    nombreMascota: mascota.nombre,
    servicioId: 'servicio-id',
    descripcionServicio: 'Vacunación anual',
    fechaHoraSolicitada: '2025-01-20T10:00:00Z',
    duracionEstimadaMin: 60,
    costoEstimado: 600
  })
});
const { data: solicitud } = await solicitudRes.json();

// 5. Crear orden PayPal
const pagoRes = await fetch('/api/Pagos/paypal/create-order', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    solicitudCitaId: solicitud.id,
    monto: solicitud.montoAnticipo,
    concepto: `Anticipo - ${solicitud.numeroSolicitud}`,
    returnUrl: 'https://app.adopets.com/pago-exitoso',
    cancelUrl: 'https://app.adopets.com/pago-cancelado'
  })
});
const { data: orden } = await pagoRes.json();

// 6. Redirigir a PayPal
window.location.href = orden.approvalUrl;

// 7. Después del pago (en returnUrl)
const captureRes = await fetch('/api/Pagos/paypal/capture-order', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    orderId: orden.orderId,
    pagoId: orden.pagoId
  })
});
const { data: pago } = await captureRes.json();

console.log('Pago completado:', pago.estadoNombre);
console.log('Esperando confirmación del personal...');
```

---

## ?? Errores Comunes

### 401 Unauthorized
```json
{ "success": false, "message": "Usuario no autenticado" }
```
**Solución:** Verificar token JWT

### 400 Pago Pendiente
```json
{ "success": false, "message": "Debe completar el pago del anticipo del 50%..." }
```
**Solución:** Completar pago antes de confirmar

### 400 Conflicto Disponibilidad
```json
{
  "success": false,
  "message": "Existen conflictos de horario",
  "data": { "disponible": false, "conflictos": [...] }
}
```
**Solución:** Seleccionar otra fecha/hora

---

## ?? Notas Importantes

### Seguridad
- ? Validación `PropietarioId == UsuarioId` en mascotas
- ? Validación `SolicitanteId == UsuarioId` en solicitudes
- ? Firma PayPal en webhooks
- ? Auditoría de pagos

### Límites
- **Fotos por mascota:** Max 10
- **Tamaño imagen:** Max 5MB
- **Solicitudes activas:** Max 5 por usuario
- **Anticipo:** Siempre 50%

### Mejores Prácticas
1. Verificar disponibilidad ANTES de crear solicitud
2. Guardar `orderId` de PayPal para tracking
3. Implementar polling para estados (cada 5-10s)
4. Manejar timeout de pago (30min)
5. Notificar por email en cambios de estado

---

**Versión:** 1.0  
**Actualizado:** Diciembre 2024  
**Contacto:** soporte@adopets.com
