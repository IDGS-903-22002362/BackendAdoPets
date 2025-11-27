# Guía de API - Gestión Administrativa de Citas, Solicitudes y Pagos

## Introducción

Esta guía documenta los endpoints administrativos del sistema AdoPets para la gestión de **citas**, **solicitudes de citas digitales** y **pagos**. Estos endpoints están diseñados para el frontend web y son utilizados por los roles administrativos del sistema:

- **Admin**: Acceso completo a todas las funcionalidades
- **Veterinario**: Gestión de citas y atención médica
- **Recepcionista**: Gestión de citas y pagos
- **Asistente**: Soporte en operaciones generales

> **Nota**: Los endpoints relacionados con el rol "Adoptante" no están incluidos en esta documentación, ya que son utilizados exclusivamente desde la aplicación móvil.

---

## Tabla de Contenidos

1. [Autenticación](#autenticación)
2. [Gestión de Citas](#gestión-de-citas)
3. [Gestión de Solicitudes de Citas Digitales](#gestión-de-solicitudes-de-citas-digitales)
4. [Gestión de Pagos](#gestión-de-pagos)
5. [Códigos de Estado HTTP](#códigos-de-estado-http)
6. [Modelos de Datos](#modelos-de-datos)

---

## Autenticación

Todos los endpoints requieren autenticación mediante JWT (JSON Web Token). El token debe incluirse en el encabezado `Authorization` de cada petición:

```
Authorization: Bearer {token}
```

### Roles y Permisos

| Rol | Citas | Solicitudes Citas | Pagos |
|-----|-------|-------------------|-------|
| Admin | ? Completo | ? Completo | ? Completo |
| Veterinario | ? Crear, Actualizar, Cancelar, Completar | ? Ver, Confirmar, Rechazar | ? Ver |
| Recepcionista | ? Crear, Actualizar, Cancelar | ? Ver, Confirmar, Rechazar | ? Crear, Ver, Cancelar |
| Asistente | ? Ver | ? Ver | ? |

---

## Gestión de Citas

Base URL: `/api/citas`

### 1. Listar Todas las Citas

Obtiene un listado de todas las citas registradas en el sistema.

**Endpoint:**
```
GET /api/citas
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Citas obtenidas exitosamente",
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "mascotaId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
      "mascotaNombre": "Max",
      "propietarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
      "propietarioNombre": "Juan Pérez",
      "veterinarioId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
      "veterinarioNombre": "Dra. María López",
      "salaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
      "salaNombre": "Consulta 1",
      "tipo": 1,
      "status": 1,
      "startAt": "2024-01-15T10:00:00Z",
      "endAt": "2024-01-15T10:30:00Z",
      "duracionMin": 30
    }
  ]
}
```

---

### 2. Obtener Cita por ID

Obtiene los detalles completos de una cita específica.

**Endpoint:**
```
GET /api/citas/{id}
```

**Parámetros de ruta:**
- `id` (UUID): ID de la cita

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "mascotaId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
    "mascotaNombre": "Max",
    "propietarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
    "propietarioNombre": "Juan Pérez",
    "propietarioEmail": "juan.perez@email.com",
    "propietarioTelefono": "+52 123 456 7890",
    "veterinarioId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
    "veterinarioNombre": "Dra. María López",
    "veterinarioEmail": "maria.lopez@veterinaria.com",
    "salaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
    "salaNombre": "Consulta 1",
    "tipo": 1,
    "status": 1,
    "startAt": "2024-01-15T10:00:00Z",
    "endAt": "2024-01-15T10:30:00Z",
    "duracionMin": 30,
    "notas": "Primera consulta de rutina",
    "motivoConsulta": "Chequeo general",
    "pagoId": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAt": "2024-01-10T08:00:00Z",
    "recordatorios": [],
    "historial": []
  }
}
```

---

### 3. Crear Nueva Cita

Crea una nueva cita en el sistema.

**Endpoint:**
```
POST /api/citas
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Cuerpo de la petición:**
```json
{
  "mascotaId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "propietarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veterinarioId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "salaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipo": 1,
  "startAt": "2024-01-15T10:00:00Z",
  "duracionMin": 30,
  "notas": "Primera consulta de rutina",
  "motivoConsulta": "Chequeo general"
}
```

**Campos obligatorios:**
- `veterinarioId`: ID del veterinario asignado
- `tipo`: Tipo de cita (1: Consulta, 2: Cirugía, 3: Vacunación, 4: Urgencia)
- `startAt`: Fecha y hora de inicio
- `duracionMin`: Duración en minutos (entre 15 y 480)

**Respuesta exitosa (201 Created):**
```json
{
  "success": true,
  "message": "Cita creada exitosamente",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "veterinarioNombre": "Dra. María López",
    "startAt": "2024-01-15T10:00:00Z",
    "status": 0
  }
}
```

---

### 4. Actualizar Cita

Actualiza los datos de una cita existente.

**Endpoint:**
```
PUT /api/citas/{id}
```

**Parámetros de ruta:**
- `id` (UUID): ID de la cita

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Cuerpo de la petición:**
```json
{
  "mascotaId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
  "propietarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veterinarioId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "salaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tipo": 1,
  "startAt": "2024-01-15T11:00:00Z",
  "duracionMin": 45,
  "notas": "Consulta reagendada",
  "motivoConsulta": "Seguimiento"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Cita actualizada exitosamente",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": 1
  }
}
```

---

### 5. Cancelar Cita

Cancela una cita programada.

**Endpoint:**
```
PUT /api/citas/{id}/cancelar
```

**Parámetros de ruta:**
- `id` (UUID): ID de la cita

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Cuerpo de la petición:**
```json
{
  "motivoRechazo": "Cliente solicitó cancelación por viaje"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Cita cancelada exitosamente",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": 4
  }
}
```

---

### 6. Completar Cita

Marca una cita como completada (solo veterinarios).

**Endpoint:**
```
PUT /api/citas/{id}/completar
```

**Parámetros de ruta:**
- `id` (UUID): ID de la cita

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario

**Cuerpo de la petición:**
```json
{
  "notas": "Consulta completada satisfactoriamente. Se recetó antibiótico."
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Cita completada exitosamente",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": 3
  }
}
```

---

### 7. Obtener Citas por Veterinario

Obtiene todas las citas asignadas a un veterinario específico.

**Endpoint:**
```
GET /api/citas/veterinario/{veterinarioId}
```

**Parámetros de ruta:**
- `veterinarioId` (UUID): ID del veterinario

**Parámetros de consulta (opcionales):**
- `startDate` (datetime): Fecha de inicio del rango
- `endDate` (datetime): Fecha de fin del rango

**Ejemplo:**
```
GET /api/citas/veterinario/9fa85f64-5717-4562-b3fc-2c963f66afa6?startDate=2024-01-01&endDate=2024-01-31
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "mascotaNombre": "Max",
      "propietarioNombre": "Juan Pérez",
      "startAt": "2024-01-15T10:00:00Z",
      "status": 1
    }
  ]
}
```

---

### 8. Obtener Citas por Mascota

Obtiene el historial de citas de una mascota.

**Endpoint:**
```
GET /api/citas/mascota/{mascotaId}
```

**Parámetros de ruta:**
- `mascotaId` (UUID): ID de la mascota

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

---

### 9. Obtener Citas por Propietario

Obtiene todas las citas de un propietario.

**Endpoint:**
```
GET /api/citas/propietario/{propietarioId}
```

**Parámetros de ruta:**
- `propietarioId` (UUID): ID del propietario

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

---

### 10. Obtener Citas por Rango de Fechas

Filtra citas dentro de un rango de fechas específico.

**Endpoint:**
```
GET /api/citas/rango
```

**Parámetros de consulta:**
- `startDate` (datetime): Fecha de inicio
- `endDate` (datetime): Fecha de fin

**Ejemplo:**
```
GET /api/citas/rango?startDate=2024-01-01T00:00:00Z&endDate=2024-01-31T23:59:59Z
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

---

### 11. Obtener Citas por Estado

Filtra citas por su estado actual.

**Endpoint:**
```
GET /api/citas/estado/{status}
```

**Parámetros de ruta:**
- `status` (int): Estado de la cita
  - `0`: Pendiente
  - `1`: Confirmada
  - `2`: En Curso
  - `3`: Completada
  - `4`: Cancelada
  - `5`: No Asistió

**Ejemplo:**
```
GET /api/citas/estado/1
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

---

### 12. Verificar Disponibilidad

Verifica la disponibilidad de un veterinario y/o sala en una fecha específica.

**Endpoint:**
```
GET /api/citas/disponibilidad
```

**Parámetros de consulta:**
- `veterinarioId` (UUID): ID del veterinario (requerido)
- `fecha` (date): Fecha a consultar (requerido)
- `salaId` (UUID): ID de la sala (opcional)

**Ejemplo:**
```
GET /api/citas/disponibilidad?veterinarioId=9fa85f64-5717-4562-b3fc-2c963f66afa6&fecha=2024-01-15&salaId=1fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": {
    "fecha": "2024-01-15",
    "horariosDisponibles": [
      {
        "horaInicio": "09:00:00",
        "horaFin": "10:00:00",
        "disponible": true
      },
      {
        "horaInicio": "10:00:00",
        "horaFin": "11:00:00",
        "disponible": false,
        "motivo": "Cita ya programada"
      }
    ]
  }
}
```

---

### 13. Eliminar Cita

Elimina permanentemente una cita del sistema (solo Admin).

**Endpoint:**
```
DELETE /api/citas/{id}
```

**Parámetros de ruta:**
- `id` (UUID): ID de la cita

**Autenticación:** Requerida  
**Roles permitidos:** Admin

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Cita eliminada exitosamente"
}
```

---

## Gestión de Solicitudes de Citas Digitales

Base URL: `/api/v1/solicitudescitasdigitales`

Las solicitudes de citas digitales son peticiones que hacen los usuarios antes de que se confirme una cita. El personal administrativo debe revisar, confirmar o rechazar estas solicitudes.

### 1. Obtener Solicitud por ID

Obtiene los detalles de una solicitud específica.

**Endpoint:**
```
GET /api/v1/solicitudescitasdigitales/{id}
```

**Parámetros de ruta:**
- `id` (UUID): ID de la solicitud

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroSolicitud": "SOL-2024-0001",
    "solicitanteId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombreSolicitante": "Juan Pérez",
    "emailSolicitante": "juan.perez@email.com",
    "mascotaId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombreMascota": "Max",
    "especieMascota": "Perro",
    "razaMascota": "Labrador",
    "servicioId": "5fa85f64-5717-4562-b3fc-2c963f66afa6",
    "descripcionServicio": "Consulta general",
    "motivoConsulta": "Chequeo de rutina",
    "fechaHoraSolicitada": "2024-01-15T10:00:00Z",
    "duracionEstimadaMin": 60,
    "veterinarioPreferidoId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombreVeterinarioPreferido": "Dra. María López",
    "costoEstimado": 500.00,
    "montoAnticipo": 0.00,
    "estado": 0,
    "estadoNombre": "Pendiente",
    "fechaSolicitud": "2024-01-10T08:00:00Z",
    "disponibilidadVerificada": false
  }
}
```

---

### 2. Obtener Solicitudes por Usuario

Obtiene todas las solicitudes de un usuario específico.

**Endpoint:**
```
GET /api/v1/solicitudescitasdigitales/usuario/{usuarioId}
```

**Parámetros de ruta:**
- `usuarioId` (UUID): ID del usuario

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

---

### 3. Obtener Solicitudes Pendientes

Lista todas las solicitudes que están pendientes de revisión.

**Endpoint:**
```
GET /api/v1/solicitudescitasdigitales/pendientes
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
      "numeroSolicitud": "SOL-2024-0001",
      "nombreSolicitante": "Juan Pérez",
      "nombreMascota": "Max",
      "fechaHoraSolicitada": "2024-01-15T10:00:00Z",
      "estado": 0,
      "estadoNombre": "Pendiente"
    }
  ]
}
```

---

### 4. Verificar Disponibilidad

Verifica si la fecha y hora solicitadas están disponibles.

**Endpoint:**
```
POST /api/v1/solicitudescitasdigitales/verificar-disponibilidad
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista, Asistente

**Cuerpo de la petición:**
```json
{
  "fechaHoraInicio": "2024-01-15T10:00:00Z",
  "duracionMin": 60,
  "veterinarioId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "salaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": {
    "disponible": true,
    "mensaje": "Horario disponible",
    "conflictos": []
  }
}
```

**Respuesta con conflictos:**
```json
{
  "success": true,
  "data": {
    "disponible": false,
    "mensaje": "Existen conflictos en el horario solicitado",
    "conflictos": [
      {
        "tipo": "Veterinario",
        "horaInicio": "2024-01-15T10:00:00Z",
        "horaFin": "2024-01-15T11:00:00Z",
        "descripcion": "Cita ya programada"
      }
    ]
  }
}
```

---

### 5. Marcar Solicitud en Revisión

Marca una solicitud como "en revisión" para que otros usuarios sepan que está siendo procesada.

**Endpoint:**
```
PUT /api/v1/solicitudescitasdigitales/{id}/en-revision
```

**Parámetros de ruta:**
- `id` (UUID): ID de la solicitud

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Solicitud en revisión",
  "data": {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
    "estado": 1,
    "estadoNombre": "En Revisión",
    "fechaRevision": "2024-01-11T09:00:00Z"
  }
}
```

---

### 6. Confirmar Solicitud

Confirma una solicitud y crea automáticamente la cita correspondiente.

**Endpoint:**
```
POST /api/v1/solicitudescitasdigitales/confirmar
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Cuerpo de la petición:**
```json
{
  "solicitudId": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
  "confirmadoPorId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "veterinarioId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "salaId": "1fa85f64-5717-4562-b3fc-2c963f66afa6",
  "fechaHoraConfirmada": "2024-01-15T10:00:00Z",
  "duracionMin": 60
}
```

**Campos obligatorios:**
- `solicitudId`: ID de la solicitud a confirmar
- `confirmadoPorId`: ID del usuario que confirma (automático desde el token)
- `veterinarioId`: ID del veterinario asignado
- `fechaHoraConfirmada`: Fecha y hora confirmada (puede ser diferente a la solicitada)
- `duracionMin`: Duración en minutos

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Solicitud confirmada y cita creada",
  "data": {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
    "estado": 2,
    "estadoNombre": "Confirmada",
    "citaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fechaConfirmacion": "2024-01-11T09:30:00Z"
  }
}
```

---

### 7. Rechazar Solicitud

Rechaza una solicitud de cita.

**Endpoint:**
```
POST /api/v1/solicitudescitasdigitales/rechazar
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Cuerpo de la petición:**
```json
{
  "solicitudId": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
  "rechazadoPorId": "9fa85f64-5717-4562-b3fc-2c963f66afa6",
  "motivo": "No hay disponibilidad para la fecha solicitada. Por favor, solicite otra fecha."
}
```

**Campos obligatorios:**
- `solicitudId`: ID de la solicitud
- `rechazadoPorId`: ID del usuario que rechaza
- `motivo`: Razón del rechazo

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Solicitud rechazada",
  "data": {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
    "estado": 3,
    "estadoNombre": "Rechazada",
    "motivoRechazo": "No hay disponibilidad para la fecha solicitada. Por favor, solicite otra fecha."
  }
}
```

---

## Gestión de Pagos

Base URL: `/api/v1/pagos`

### 1. Crear Pago

Crea un nuevo registro de pago en el sistema.

**Endpoint:**
```
POST /api/v1/pagos
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Recepcionista

**Cuerpo de la petición:**
```json
{
  "usuarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "monto": 500.00,
  "moneda": "MXN",
  "tipo": 1,
  "metodo": 1,
  "concepto": "Pago de consulta veterinaria",
  "referencia": "REF-2024-0001",
  "citaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "esAnticipo": false,
  "montoTotal": 500.00
}
```

**Campos obligatorios:**
- `monto`: Cantidad a pagar
- `tipo`: Tipo de pago (1: Consulta, 2: Cirugía, 3: Medicamento, 4: Otro)
- `metodo`: Método de pago (1: Efectivo, 2: Tarjeta, 3: Transferencia, 4: PayPal)

**Tipos de Pago:**
- `1`: Consulta
- `2`: Cirugía
- `3`: Medicamento
- `4`: Otro

**Métodos de Pago:**
- `1`: Efectivo
- `2`: Tarjeta
- `3`: Transferencia
- `4`: PayPal

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Pago creado exitosamente",
  "data": {
    "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAG-2024-0001",
    "monto": 500.00,
    "moneda": "MXN",
    "estado": 1,
    "estadoNombre": "Completado",
    "fechaPago": "2024-01-11T10:00:00Z"
  }
}
```

---

### 2. Crear Orden de Pago con PayPal

Crea una orden de pago para procesar con PayPal.

**Endpoint:**
```
POST /api/v1/pagos/paypal/create-order
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Recepcionista

**Cuerpo de la petición:**
```json
{
  "usuarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
  "monto": 500.00,
  "concepto": "Pago de consulta veterinaria",
  "solicitudCitaId": "4fa85f64-5717-4562-b3fc-2c963f66afa6",
  "citaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "esAnticipo": false,
  "montoTotal": 500.00,
  "returnUrl": "https://frontend.com/pagos/exito",
  "cancelUrl": "https://frontend.com/pagos/cancelado"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Orden de PayPal creada exitosamente",
  "data": {
    "orderId": "PAYPAL-ORDER-123456",
    "approvalUrl": "https://www.paypal.com/checkoutnow?token=XXXXX",
    "status": "CREATED"
  }
}
```

---

### 3. Capturar Pago de PayPal

Captura un pago previamente autorizado en PayPal.

**Endpoint:**
```
POST /api/v1/pagos/paypal/capture
```

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Recepcionista

**Cuerpo de la petición:**
```json
{
  "orderId": "PAYPAL-ORDER-123456"
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Pago capturado exitosamente",
  "data": {
    "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAG-2024-0001",
    "payPalOrderId": "PAYPAL-ORDER-123456",
    "payPalCaptureId": "CAPTURE-123456",
    "estado": 2,
    "estadoNombre": "Completado",
    "fechaConfirmacion": "2024-01-11T10:05:00Z"
  }
}
```

---

### 4. Obtener Pago por ID

Consulta los detalles de un pago específico.

**Endpoint:**
```
GET /api/v1/pagos/{id}
```

**Parámetros de ruta:**
- `id` (UUID): ID del pago

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "numeroPago": "PAG-2024-0001",
    "usuarioId": "8fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombreUsuario": "Juan Pérez",
    "monto": 500.00,
    "moneda": "MXN",
    "tipo": 1,
    "tipoNombre": "Consulta",
    "metodo": 2,
    "metodoNombre": "Tarjeta",
    "estado": 2,
    "estadoNombre": "Completado",
    "fechaPago": "2024-01-11T10:00:00Z",
    "fechaConfirmacion": "2024-01-11T10:05:00Z",
    "concepto": "Pago de consulta veterinaria",
    "referencia": "REF-2024-0001",
    "citaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "esAnticipo": false,
    "montoTotal": 500.00,
    "createdAt": "2024-01-11T10:00:00Z"
  }
}
```

---

### 5. Obtener Pago por Order ID de PayPal

Busca un pago mediante el Order ID de PayPal.

**Endpoint:**
```
GET /api/v1/pagos/paypal/{orderId}
```

**Parámetros de ruta:**
- `orderId` (string): Order ID de PayPal

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

---

### 6. Obtener Pagos por Usuario

Lista todos los pagos realizados por un usuario.

**Endpoint:**
```
GET /api/v1/pagos/usuario/{usuarioId}
```

**Parámetros de ruta:**
- `usuarioId` (UUID): ID del usuario

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Veterinario, Recepcionista

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
      "numeroPago": "PAG-2024-0001",
      "monto": 500.00,
      "moneda": "MXN",
      "estado": 2,
      "estadoNombre": "Completado",
      "fechaPago": "2024-01-11T10:00:00Z"
    }
  ]
}
```

---

### 7. Cancelar Pago

Cancela un pago registrado en el sistema.

**Endpoint:**
```
PUT /api/v1/pagos/{id}/cancelar
```

**Parámetros de ruta:**
- `id` (UUID): ID del pago

**Autenticación:** Requerida  
**Roles permitidos:** Admin, Recepcionista

**Cuerpo de la petición:**
```json
"Cliente solicitó reembolso"
```

**Respuesta exitosa (200 OK):**
```json
{
  "success": true,
  "message": "Pago cancelado",
  "data": {
    "id": "2fa85f64-5717-4562-b3fc-2c963f66afa6",
    "estado": 3,
    "estadoNombre": "Cancelado"
  }
}
```

---

## Códigos de Estado HTTP

| Código | Descripción |
|--------|-------------|
| 200 | OK - Solicitud exitosa |
| 201 | Created - Recurso creado exitosamente |
| 400 | Bad Request - Datos inválidos o faltantes |
| 401 | Unauthorized - Falta autenticación |
| 403 | Forbidden - Sin permisos para esta acción |
| 404 | Not Found - Recurso no encontrado |
| 409 | Conflict - Conflicto con el estado actual (ej: horario ocupado) |
| 500 | Internal Server Error - Error del servidor |

---

## Modelos de Datos

### Estados de Cita (StatusCita)

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Pendiente | Cita creada pero no confirmada |
| 1 | Confirmada | Cita confirmada por el cliente |
| 2 | En Curso | Cita en proceso |
| 3 | Completada | Cita finalizada |
| 4 | Cancelada | Cita cancelada |
| 5 | No Asistió | Cliente no se presentó |

### Tipos de Cita (TipoCita)

| Valor | Nombre |
|-------|--------|
| 1 | Consulta |
| 2 | Cirugía |
| 3 | Vacunación |
| 4 | Urgencia |

### Estados de Solicitud de Cita

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Pendiente | En espera de revisión |
| 1 | En Revisión | Siendo revisada por personal |
| 2 | Confirmada | Confirmada y cita creada |
| 3 | Rechazada | Rechazada con motivo |
| 4 | Cancelada | Cancelada por el solicitante |

### Estados de Pago

| Valor | Nombre | Descripción |
|-------|--------|-------------|
| 0 | Pendiente | Esperando pago |
| 1 | En Proceso | Procesando pago |
| 2 | Completado | Pago confirmado |
| 3 | Cancelado | Pago cancelado |
| 4 | Reembolsado | Pago reembolsado |

### Tipos de Pago

| Valor | Nombre |
|-------|--------|
| 1 | Consulta |
| 2 | Cirugía |
| 3 | Medicamento |
| 4 | Otro |

### Métodos de Pago

| Valor | Nombre |
|-------|--------|
| 1 | Efectivo |
| 2 | Tarjeta |
| 3 | Transferencia |
| 4 | PayPal |

---

## Ejemplos de Flujo de Trabajo

### Flujo 1: Creación de Cita desde el Frontend Administrativo

1. **Verificar disponibilidad**
   ```
   GET /api/citas/disponibilidad?veterinarioId={id}&fecha=2024-01-15
   ```

2. **Crear la cita**
   ```
   POST /api/citas
   ```

3. **Confirmar la cita** (si es necesario)
   ```
   PUT /api/citas/{id}
   ```

### Flujo 2: Gestión de Solicitud de Cita Digital

1. **Ver solicitudes pendientes**
   ```
   GET /api/v1/solicitudescitasdigitales/pendientes
   ```

2. **Marcar como en revisión**
   ```
   PUT /api/v1/solicitudescitasdigitales/{id}/en-revision
   ```

3. **Verificar disponibilidad**
   ```
   POST /api/v1/solicitudescitasdigitales/verificar-disponibilidad
   ```

4. **Confirmar solicitud** (crea automáticamente la cita)
   ```
   POST /api/v1/solicitudescitasdigitales/confirmar
   ```

   **O rechazar solicitud:**
   ```
   POST /api/v1/solicitudescitasdigitales/rechazar
   ```

### Flujo 3: Procesamiento de Pago

1. **Crear registro de pago**
   ```
   POST /api/v1/pagos
   ```

   **O para PayPal:**
   ```
   POST /api/v1/pagos/paypal/create-order
   ```

2. **Capturar pago (solo PayPal)**
   ```
   POST /api/v1/pagos/paypal/capture
   ```

3. **Consultar pago**
   ```
   GET /api/v1/pagos/{id}
   ```

---

## Notas Importantes

### Seguridad
- Todos los endpoints requieren autenticación JWT
- Los roles se verifican en cada petición
- Las acciones sensibles (eliminar, cancelar) requieren roles específicos

### Validaciones
- Las fechas de citas deben estar en el futuro
- La duración de citas debe estar entre 15 y 480 minutos
- No se pueden crear citas con conflictos de horario

### Notificaciones
- El sistema envía notificaciones automáticas cuando:
  - Se crea una cita
  - Se confirma una solicitud
  - Se cancela una cita
  - Se completa un pago

### Zona Horaria
- Todas las fechas y horas se manejan en UTC
- El frontend debe convertir a la zona horaria local

---

## Soporte

Para preguntas o problemas relacionados con la API, contactar al equipo de desarrollo.

**Versión de la API:** 1.0  
**Última actualización:** Enero 2024
