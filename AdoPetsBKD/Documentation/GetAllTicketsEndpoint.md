# Endpoint para Obtener Todos los Tickets

## ?? Descripción

Se agregó un nuevo endpoint `GET /api/v1/Tickets` para obtener **todos los tickets** del sistema con **todos sus detalles** (información completa del ticket y sus ítems).

---

## ?? Endpoint

```
GET /api/v1/Tickets
```

### **Headers Requeridos**

```
Authorization: Bearer {token}
```

---

## ?? Respuesta

### **Estructura de Respuesta**

```json
{
  "success": true,
  "message": "Se encontraron 15 tickets",
  "data": [
    {
      "id": "guid-ticket-1",
      "numeroTicket": "TK-20251203-0001",
      "citaId": "guid-cita",
      "mascotaId": "guid-mascota",
      "nombreMascota": "Max",
      "clienteId": "guid-cliente",
      "nombreCliente": "Juan Pérez García",
      "veterinarioId": "guid-vet",
      "nombreVeterinario": "Dra. María López",
      "fechaProcedimiento": "2025-12-03T10:30:00Z",
      "nombreProcedimiento": "Consulta General",
      "descripcionProcedimiento": "Revisión general de salud",
      "costoProcedimiento": 500.00,
      "costoInsumos": 150.00,
      "costoAdicional": 50.00,
      "subtotal": 700.00,
      "descuento": 0.00,
      "iva": 112.00,
      "total": 812.00,
      "observaciones": "Mascota en buen estado",
      "diagnostico": "Saludable, requiere vacunas de refuerzo",
      "tratamiento": "Aplicación de vacuna antirrábica",
      "medicacionPrescrita": "Ninguna",
      "estado": 1,
      "estadoNombre": "Pendiente",
      "fechaEntrega": null,
      "pagoId": null,
      "createdAt": "2025-12-03T10:45:00Z",
      "detalles": [
        {
          "id": "guid-detalle-1",
          "descripcion": "Vacuna Antirrábica",
          "cantidad": 1,
          "unidad": "dosis",
          "precioUnitario": 120.00,
          "subtotal": 120.00,
          "tipo": 3,
          "tipoNombre": "Vacuna"
        },
        {
          "id": "guid-detalle-2",
          "descripcion": "Suplemento Vitamínico",
          "cantidad": 1,
          "unidad": "sobre",
          "precioUnitario": 30.00,
          "subtotal": 30.00,
          "tipo": 2,
          "tipoNombre": "Insumo"
        }
      ]
    },
    {
      "id": "guid-ticket-2",
      "numeroTicket": "TK-20251203-0002",
      // ... más tickets
    }
  ],
  "errors": []
}
```

---

## ?? Campos de Cada Ticket

### **Información General**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `Guid` | ID único del ticket |
| `numeroTicket` | `string` | Número de ticket (formato: TK-YYYYMMDD-NNNN) |
| `citaId` | `Guid` | ID de la cita asociada |
| `fechaProcedimiento` | `DateTime` | Fecha en que se realizó el procedimiento |
| `createdAt` | `DateTime` | Fecha de creación del ticket |

### **Información de Procedimiento**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `nombreProcedimiento` | `string` | Nombre del procedimiento realizado |
| `descripcionProcedimiento` | `string` | Descripción detallada |
| `diagnostico` | `string` | Diagnóstico del veterinario |
| `tratamiento` | `string` | Tratamiento aplicado |
| `medicacionPrescrita` | `string` | Medicación recetada |
| `observaciones` | `string` | Observaciones adicionales |

### **Información de Participantes**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `mascotaId` | `Guid?` | ID de la mascota (opcional) |
| `nombreMascota` | `string` | Nombre de la mascota |
| `clienteId` | `Guid` | ID del cliente/propietario |
| `nombreCliente` | `string` | Nombre completo del cliente |
| `veterinarioId` | `Guid` | ID del veterinario |
| `nombreVeterinario` | `string` | Nombre completo del veterinario |

### **Información Financiera**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `costoProcedimiento` | `decimal` | Costo del procedimiento principal |
| `costoInsumos` | `decimal` | Costo total de insumos usados |
| `costoAdicional` | `decimal` | Costos adicionales |
| `subtotal` | `decimal` | Subtotal (suma de costos) |
| `descuento` | `decimal` | Descuento aplicado |
| `iva` | `decimal` | IVA calculado (16%) |
| `total` | `decimal` | Total final a pagar |

### **Estado y Pago**

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `estado` | `int` | Código del estado (1=Pendiente, 2=Entregado, 3=Cancelado) |
| `estadoNombre` | `string` | Nombre del estado |
| `fechaEntrega` | `DateTime?` | Fecha de entrega (si aplica) |
| `pagoId` | `Guid?` | ID del pago asociado (si existe) |

### **Detalles (Ítems del Ticket)**

Cada ticket incluye un array `detalles` con los ítems consumidos:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `id` | `Guid` | ID del detalle |
| `descripcion` | `string` | Descripción del ítem |
| `cantidad` | `decimal` | Cantidad consumida |
| `unidad` | `string` | Unidad de medida (ml, dosis, sobre, etc.) |
| `precioUnitario` | `decimal` | Precio por unidad |
| `subtotal` | `decimal` | Subtotal del ítem (cantidad × precio) |
| `tipo` | `int` | Tipo de ítem (0=Servicio, 1=Producto, 2=Insumo, 3=Vacuna, 4=Medicamento) |
| `tipoNombre` | `string` | Nombre del tipo |

---

## ?? Tipos de Detalle

```csharp
public enum TipoDetalleTicket
{
    Servicio = 0,        // Servicios veterinarios
    Producto = 1,        // Productos vendidos
    Insumo = 2,          // Insumos consumidos (gauza, jeringas, etc.)
    Vacuna = 3,          // Vacunas aplicadas
    Medicamento = 4,     // Medicamentos administrados
    Otro = 99            // Otros conceptos
}
```

---

## ?? Características del Endpoint

### ? **Ordenamiento**
Los tickets se retornan **ordenados por fecha de creación descendente** (más recientes primero).

### ? **Includes Completos**
Cada ticket incluye:
- ? Información de la mascota
- ? Información del cliente
- ? Información del veterinario
- ? **Todos los detalles** (ítems del ticket)

### ? **Cálculos Automáticos**
Todos los totales están calculados:
- Subtotal
- IVA (16%)
- Total final

---

## ?? Ejemplos de Uso

### **Ejemplo 1: Obtener Todos los Tickets**

```bash
curl -X GET "https://api.adopets.com/api/v1/Tickets" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Respuesta:**

```json
{
  "success": true,
  "message": "Se encontraron 3 tickets",
  "data": [
    {
      "id": "a1b2c3d4-...",
      "numeroTicket": "TK-20251203-0003",
      "nombreProcedimiento": "Cirugía de Esterilización",
      "total": 2500.00,
      "estado": 1,
      "estadoNombre": "Pendiente",
      "detalles": [
        {
          "descripcion": "Anestesia Isoflurano",
          "cantidad": 50,
          "unidad": "ml",
          "precioUnitario": 10.50,
          "subtotal": 525.00,
          "tipo": 2,
          "tipoNombre": "Insumo"
        }
      ]
    },
    // ... más tickets
  ]
}
```

### **Ejemplo 2: Tickets Vacíos**

Si no hay tickets en el sistema:

```json
{
  "success": true,
  "message": "Se encontraron 0 tickets",
  "data": [],
  "errors": []
}
```

---

## ?? Casos de Uso

### **1. Dashboard Administrativo**
Mostrar todos los tickets del día/semana/mes con sus totales.

```typescript
const response = await fetch('/api/v1/Tickets', {
  headers: { 'Authorization': `Bearer ${token}` }
});
const { data: tickets } = await response.json();

// Calcular total de ventas
const totalVentas = tickets.reduce((sum, t) => sum + t.total, 0);

// Filtrar por fecha
const ticketsHoy = tickets.filter(t => 
  new Date(t.createdAt).toDateString() === new Date().toDateString()
);
```

### **2. Lista de Tickets Pendientes**

```typescript
const ticketsPendientes = tickets.filter(t => t.estado === 1);
```

### **3. Reporte de Insumos Consumidos**

```typescript
const insumosConsumidos = [];
tickets.forEach(ticket => {
  ticket.detalles
    .filter(d => d.tipo === 2) // Solo insumos
    .forEach(detalle => {
      insumosConsumidos.push({
        ticket: ticket.numeroTicket,
        insumo: detalle.descripcion,
        cantidad: detalle.cantidad,
        costo: detalle.subtotal
      });
    });
});
```

### **4. Análisis de Veterinarios**

```typescript
const ticketsPorVeterinario = tickets.reduce((acc, ticket) => {
  const vet = ticket.nombreVeterinario;
  if (!acc[vet]) {
    acc[vet] = { cantidad: 0, total: 0 };
  }
  acc[vet].cantidad++;
  acc[vet].total += ticket.total;
  return acc;
}, {});
```

---

## ?? Seguridad

### **Autorización**
- ? Requiere autenticación (token JWT)
- ? Solo usuarios autorizados pueden acceder
- ?? **Nota:** Considera agregar filtros por rol (ej: solo admin y recepcionistas)

### **Recomendación de Mejora**

Agregar filtros opcionales por query parameters:

```csharp
[HttpGet]
public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetAllTickets(
    [FromQuery] DateTime? fechaDesde,
    [FromQuery] DateTime? fechaHasta,
    [FromQuery] int? estado,
    [FromQuery] Guid? veterinarioId)
{
    // Implementar filtros opcionales
}
```

---

## ?? Ejemplo Completo de Respuesta

```json
{
  "success": true,
  "message": "Se encontraron 2 tickets",
  "data": [
    {
      "id": "979708c0-019d-4ea8-be0a-d1b36c2f1e47",
      "numeroTicket": "TK-20251203-0001",
      "citaId": "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6",
      "mascotaId": "94dd9bb8-bd32-4c1d-a945-d1b5d2009410",
      "nombreMascota": "Max",
      "clienteId": "53ba8e2c-cc1d-489a-a29f-0828961e84be",
      "nombreCliente": "Juan Pérez García",
      "veterinarioId": "d67ee6cd-8e00-40ff-965d-9611eef11475",
      "nombreVeterinario": "Dra. María López",
      "fechaProcedimiento": "2025-12-03T10:30:00Z",
      "nombreProcedimiento": "Consulta General",
      "descripcionProcedimiento": "Revisión general de salud",
      "costoProcedimiento": 500.00,
      "costoInsumos": 150.00,
      "costoAdicional": 0.00,
      "subtotal": 650.00,
      "descuento": 0.00,
      "iva": 104.00,
      "total": 754.00,
      "observaciones": "Mascota en buen estado general",
      "diagnostico": "Saludable, requiere vacuna antirrábica de refuerzo",
      "tratamiento": "Aplicación de vacuna antirrábica + suplemento vitamínico",
      "medicacionPrescrita": "Ninguna",
      "estado": 1,
      "estadoNombre": "Pendiente",
      "fechaEntrega": null,
      "pagoId": null,
      "createdAt": "2025-12-03T10:45:00Z",
      "detalles": [
        {
          "id": "det-001",
          "descripcion": "Vacuna Antirrábica",
          "cantidad": 1.0,
          "unidad": "dosis",
          "precioUnitario": 120.00,
          "subtotal": 120.00,
          "tipo": 3,
          "tipoNombre": "Vacuna"
        },
        {
          "id": "det-002",
          "descripcion": "Suplemento Vitamínico",
          "cantidad": 1.0,
          "unidad": "sobre",
          "precioUnitario": 30.00,
          "subtotal": 30.00,
          "tipo": 2,
          "tipoNombre": "Insumo"
        }
      ]
    },
    {
      "id": "ticket-002",
      "numeroTicket": "TK-20251203-0002",
      "citaId": "cita-002",
      "mascotaId": "mascota-002",
      "nombreMascota": "Luna",
      "clienteId": "cliente-002",
      "nombreCliente": "Ana Martínez",
      "veterinarioId": "vet-002",
      "nombreVeterinario": "Dr. Carlos Ruiz",
      "fechaProcedimiento": "2025-12-03T14:00:00Z",
      "nombreProcedimiento": "Cirugía de Esterilización",
      "descripcionProcedimiento": "Esterilización quirúrgica",
      "costoProcedimiento": 2000.00,
      "costoInsumos": 800.00,
      "costoAdicional": 200.00,
      "subtotal": 3000.00,
      "descuento": 300.00,
      "iva": 432.00,
      "total": 3132.00,
      "observaciones": "Cirugía exitosa, requiere revisión en 7 días",
      "diagnostico": "Post-operatorio normal",
      "tratamiento": "Cirugía + antibiótico + analgésico",
      "medicacionPrescrita": "Amoxicilina 500mg cada 8 horas por 7 días",
      "estado": 1,
      "estadoNombre": "Pendiente",
      "fechaEntrega": null,
      "pagoId": null,
      "createdAt": "2025-12-03T15:30:00Z",
      "detalles": [
        {
          "id": "det-003",
          "descripcion": "Anestesia Isoflurano",
          "cantidad": 50.0,
          "unidad": "ml",
          "precioUnitario": 10.50,
          "subtotal": 525.00,
          "tipo": 2,
          "tipoNombre": "Insumo"
        },
        {
          "id": "det-004",
          "descripcion": "Sutura Absorbible 3-0",
          "cantidad": 3.0,
          "unidad": "sobres",
          "precioUnitario": 80.00,
          "subtotal": 240.00,
          "tipo": 2,
          "tipoNombre": "Insumo"
        },
        {
          "id": "det-005",
          "descripcion": "Amoxicilina 500mg",
          "cantidad": 21.0,
          "unidad": "tabletas",
          "precioUnitario": 5.00,
          "subtotal": 105.00,
          "tipo": 4,
          "tipoNombre": "Medicamento"
        }
      ]
    }
  ],
  "errors": []
}
```

---

## ? Resumen

| Aspecto | Detalle |
|---------|---------|
| **Endpoint** | `GET /api/v1/Tickets` |
| **Autenticación** | Requerida (Bearer Token) |
| **Respuesta** | Lista de tickets con todos sus detalles |
| **Ordenamiento** | Por fecha de creación (más recientes primero) |
| **Includes** | Mascota, Cliente, Veterinario, Detalles |
| **HTTP 200** | Lista de tickets (puede estar vacía) |
| **HTTP 400** | Error en la petición |
| **HTTP 401** | No autenticado |

**El endpoint está listo para ser consumido por el frontend y mostrar todos los tickets del sistema con información completa.**
