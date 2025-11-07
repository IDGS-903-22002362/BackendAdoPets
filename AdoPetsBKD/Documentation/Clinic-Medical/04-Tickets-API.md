# API de Tickets - Documentación

## Descripción General
El módulo de **Tickets** gestiona los comprobantes de procedimientos médicos y servicios realizados en la clínica. Los tickets incluyen detalles de servicios prestados, costos, y pueden generar PDFs para impresión usando **QuestPDF**.

---

## Características Principales
- ? Generación automática de número de ticket único
- ? Detalles itemizados de servicios y productos
- ? Cálculo automático de subtotales, IVA y totales
- ? Generación de PDF descargable
- ? Control de estado de entrega
- ? Vinculación con citas y clientes

---

## Endpoints

### 1. Crear Nuevo Ticket
**Endpoint:** `POST /api/tickets`

**Autorización:** Requerida (Token JWT)

**Descripción:** Crea un nuevo ticket de servicios/procedimientos para un cliente.

**Cuerpo de la Solicitud:**
```json
{
  "clienteId": "guid",
  "mascotaId": "guid",
  "citaId": "guid",
  "detalles": [
    {
      "tipoServicioId": "guid",
      "descripcion": "Consulta general",
      "cantidad": 1,
      "precioUnitario": 500.00
    },
    {
      "tipoServicioId": "guid",
      "descripcion": "Vacuna triple felina",
      "cantidad": 1,
      "precioUnitario": 350.00
    },
    {
      "tipoServicioId": "guid",
      "descripcion": "Desparasitante interno",
      "cantidad": 1,
      "precioUnitario": 200.00
    }
  ],
  "notas": "Cliente solicita factura",
  "aplicarDescuento": false,
  "porcentajeDescuento": 0
}
```

**Validaciones:**
- `clienteId` - Requerido
- `detalles` - Requerido, al menos 1 detalle
- `descripcion` en cada detalle - Requerido
- `cantidad` - Requerido, mayor a 0
- `precioUnitario` - Requerido, mayor o igual a 0

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Ticket creado exitosamente",
  "data": {
    "id": "guid",
    "numeroTicket": "TICKET-20240115-001",
    "fecha": "2024-01-15T14:30:00",
    "clienteId": "guid",
    "clienteNombre": "Juan Pérez",
    "mascotaId": "guid",
    "mascotaNombre": "Max",
    "citaId": "guid",
    "detalles": [
      {
        "id": "guid",
        "descripcion": "Consulta general",
        "cantidad": 1,
        "precioUnitario": 500.00,
        "subtotal": 500.00
      },
      {
        "id": "guid",
        "descripcion": "Vacuna triple felina",
        "cantidad": 1,
        "precioUnitario": 350.00,
        "subtotal": 350.00
      },
      {
        "id": "guid",
        "descripcion": "Desparasitante interno",
        "cantidad": 1,
        "precioUnitario": 200.00,
        "subtotal": 200.00
      }
    ],
    "subtotal": 1050.00,
    "iva": 168.00,
    "descuento": 0.00,
    "total": 1218.00,
    "estado": "Pendiente",
    "entregado": false,
    "notas": "Cliente solicita factura"
  }
}
```

**Códigos de Estado:**
- `200 OK` - Ticket creado exitosamente
- `400 Bad Request` - Datos inválidos
- `401 Unauthorized` - No autenticado
- `500 Internal Server Error` - Error del servidor

**Cálculos Automáticos:**
```javascript
subtotal = sum(cantidad * precioUnitario)
descuento = subtotal * (porcentajeDescuento / 100)
baseImponible = subtotal - descuento
iva = baseImponible * 0.16
total = baseImponible + iva
```

---

### 2. Obtener Ticket por ID
**Endpoint:** `GET /api/tickets/{id}`

**Autorización:** Requerida (Token JWT)

**Parámetros de Ruta:**
- `id` (Guid) - ID del ticket

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "numeroTicket": "TICKET-20240115-001",
    "fecha": "2024-01-15T14:30:00",
    "cliente": {
      "id": "guid",
      "nombre": "Juan Pérez",
      "email": "juan@email.com",
      "telefono": "+52-555-123-4567"
    },
    "mascota": {
      "id": "guid",
      "nombre": "Max",
      "especie": "Perro",
      "raza": "Labrador"
    },
    "cita": {
      "id": "guid",
      "fechaHora": "2024-01-15T10:00:00",
      "veterinario": "Dr. González"
    },
    "detalles": [
      {
        "id": "guid",
        "tipoServicio": "Consulta",
        "descripcion": "Consulta general",
        "cantidad": 1,
        "precioUnitario": 500.00,
        "subtotal": 500.00
      }
    ],
    "subtotal": 1050.00,
    "porcentajeDescuento": 0,
    "descuento": 0.00,
    "iva": 168.00,
    "total": 1218.00,
    "estado": "Pendiente",
    "entregado": false,
    "fechaEntrega": null,
    "notas": "Cliente solicita factura",
    "creadoPor": "Admin",
    "fechaCreacion": "2024-01-15T14:30:00"
  }
}
```

**Códigos de Estado:**
- `200 OK` - Ticket encontrado
- `404 Not Found` - Ticket no encontrado
- `401 Unauthorized` - No autenticado

---

### 3. Obtener Ticket por Número
**Endpoint:** `GET /api/tickets/numero/{numeroTicket}`

**Autorización:** Requerida (Token JWT)

**Parámetros de Ruta:**
- `numeroTicket` (string) - Número de ticket (Ej: "TICKET-20240115-001")

**Descripción:** Busca un ticket por su número único. Útil para búsquedas rápidas desde recepción.

**Ejemplo de Uso:**
```
GET /api/tickets/numero/TICKET-20240115-001
```

**Respuesta:** Igual estructura que GET /api/tickets/{id}

---

### 4. Obtener Tickets por Cliente
**Endpoint:** `GET /api/tickets/cliente/{clienteId}`

**Autorización:** Requerida (Token JWT)

**Parámetros de Ruta:**
- `clienteId` (Guid) - ID del cliente

**Descripción:** Retorna el historial completo de tickets de un cliente.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "numeroTicket": "TICKET-20240115-001",
      "fecha": "2024-01-15T14:30:00",
      "mascotaNombre": "Max",
      "total": 1218.00,
      "estado": "Entregado",
      "entregado": true
    },
    {
      "id": "guid",
      "numeroTicket": "TICKET-20240110-015",
      "fecha": "2024-01-10T11:00:00",
      "mascotaNombre": "Luna",
      "total": 850.00,
      "estado": "Pendiente",
      "entregado": false
    }
  ]
}
```

**Ordenamiento:** Por fecha descendente (más reciente primero)

---

### 5. Obtener Tickets por Cita
**Endpoint:** `GET /api/tickets/cita/{citaId}`

**Autorización:** Requerida (Token JWT)

**Parámetros de Ruta:**
- `citaId` (Guid) - ID de la cita

**Descripción:** Retorna todos los tickets asociados a una cita específica. Una cita puede tener múltiples tickets si se realizaron varios procedimientos.

---

### 6. Marcar como Entregado
**Endpoint:** `PUT /api/tickets/{id}/entregar`

**Autorización:** Requerida (Roles: Admin, Recepcionista)

**Parámetros de Ruta:**
- `id` (Guid) - ID del ticket

**Descripción:** Marca el ticket como entregado al cliente.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Ticket marcado como entregado",
  "data": {
    "id": "guid",
    "numeroTicket": "TICKET-20240115-001",
    "entregado": true,
    "fechaEntrega": "2024-01-15T16:00:00",
    "entregadoPor": "Recepcionista María"
  }
}
```

**Códigos de Estado:**
- `200 OK` - Actualización exitosa
- `404 Not Found` - Ticket no encontrado
- `400 Bad Request` - Ticket ya entregado

---

### 7. Generar PDF del Ticket
**Endpoint:** `GET /api/tickets/{id}/pdf`

**Autorización:** Requerida (Token JWT)

**Parámetros de Ruta:**
- `id` (Guid) - ID del ticket

**Descripción:** Genera y descarga un PDF profesional del ticket usando **QuestPDF**.

**Respuesta Exitosa (200):**
- **Content-Type:** `application/pdf`
- **Content-Disposition:** `attachment; filename="ticket-{id}.pdf"`

**Ejemplo de Uso:**
```bash
curl -X GET https://api.adopets.com/api/tickets/12345678-1234-1234-1234-123456789012/pdf \
  -H "Authorization: Bearer {token}" \
  --output ticket.pdf
```

**Estructura del PDF:**
```
???????????????????????????????????????????
?        CLÍNICA VETERINARIA ADOPETS      ?
?                                         ?
?  Ticket: TICKET-20240115-001            ?
?  Fecha: 15/01/2024 14:30                ?
?                                         ?
?  Cliente: Juan Pérez                    ?
?  Mascota: Max (Perro - Labrador)        ?
?                                         ?
?  SERVICIOS PRESTADOS                    ?
?  ?????????????????????????????????????  ?
?  Consulta general        $500.00        ?
?  Vacuna triple felina    $350.00        ?
?  Desparasitante          $200.00        ?
?                                         ?
?  Subtotal:              $1,050.00       ?
?  Descuento (0%):            $0.00       ?
?  IVA (16%):               $168.00       ?
?  ?????????????????????????????????????  ?
?  TOTAL:                 $1,218.00       ?
?                                         ?
?  Notas: Cliente solicita factura        ?
?                                         ?
?  Gracias por su visita                  ?
???????????????????????????????????????????
```

**Códigos de Estado:**
- `200 OK` - PDF generado exitosamente
- `404 Not Found` - Ticket no encontrado
- `500 Internal Server Error` - Error al generar PDF

---

## Estados de Ticket

| Estado | Descripción |
|--------|-------------|
| **Pendiente** | Ticket creado, pendiente de entrega |
| **Entregado** | Ticket entregado al cliente |
| **Cancelado** | Ticket anulado (error en captura) |

---

## Lógica de Negocio

### Generación de Número de Ticket
```csharp
// Formato: TICKET-YYYYMMDD-XXX
// Ejemplo: TICKET-20240115-001

string fechaParte = DateTime.Now.ToString("yyyyMMdd");
int consecutivo = ObtenerConsecutivoDelDia();
string numeroTicket = $"TICKET-{fechaParte}-{consecutivo:D3}";
```

### Cálculo de Totales
```csharp
// 1. Subtotal
decimal subtotal = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

// 2. Descuento
decimal descuento = subtotal * (porcentajeDescuento / 100);

// 3. Base imponible
decimal baseImponible = subtotal - descuento;

// 4. IVA (16% en México)
decimal iva = baseImponible * 0.16m;

// 5. Total
decimal total = baseImponible + iva;
```

### Validaciones Especiales
- Un ticket debe tener al menos 1 detalle
- Los precios no pueden ser negativos
- El porcentaje de descuento debe estar entre 0 y 100
- Solo se puede marcar como entregado una vez

---

## Integración con QuestPDF

### Instalación
```bash
dotnet add package QuestPDF
```

### Implementación Básica
```csharp
public byte[] GenerarPdfTicket(Ticket ticket)
{
    var document = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            
            // Encabezado
            page.Header()
                .Text("CLÍNICA VETERINARIA ADOPETS")
                .FontSize(20)
                .Bold();
            
            // Contenido
            page.Content()
                .Column(column =>
                {
                    // Número de ticket
                    column.Item().Text($"Ticket: {ticket.NumeroTicket}");
                    
                    // Cliente
                    column.Item().Text($"Cliente: {ticket.ClienteNombre}");
                    
                    // Detalles
                    foreach (var detalle in ticket.Detalles)
                    {
                        column.Item().Text($"{detalle.Descripcion} - ${detalle.Subtotal}");
                    }
                    
                    // Total
                    column.Item().Text($"TOTAL: ${ticket.Total}").Bold();
                });
            
            // Pie de página
            page.Footer()
                .AlignCenter()
                .Text("Gracias por su visita");
        });
    });
    
    return document.GeneratePdf();
}
```

---

## Ejemplos de Uso

### Ejemplo 1: Crear Ticket de Consulta Simple
```bash
curl -X POST https://api.adopets.com/api/tickets \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": "12345678-1234-1234-1234-123456789012",
    "mascotaId": "87654321-4321-4321-4321-210987654321",
    "citaId": "11111111-1111-1111-1111-111111111111",
    "detalles": [
      {
        "tipoServicioId": "service-guid",
        "descripcion": "Consulta general",
        "cantidad": 1,
        "precioUnitario": 500.00
      }
    ],
    "notas": "Primera visita",
    "aplicarDescuento": false,
    "porcentajeDescuento": 0
  }'
```

### Ejemplo 2: Crear Ticket con Descuento
```bash
curl -X POST https://api.adopets.com/api/tickets \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": "cliente-guid",
    "mascotaId": "mascota-guid",
    "detalles": [
      {
        "descripcion": "Cirugía de esterilización",
        "cantidad": 1,
        "precioUnitario": 2500.00
      },
      {
        "descripcion": "Medicamentos post-operatorios",
        "cantidad": 1,
        "precioUnitario": 350.00
      }
    ],
    "aplicarDescuento": true,
    "porcentajeDescuento": 10,
    "notas": "Descuento por campaña de esterilización"
  }'
```

### Ejemplo 3: Marcar Ticket como Entregado
```bash
curl -X PUT https://api.adopets.com/api/tickets/12345678-1234-1234-1234-123456789012/entregar \
  -H "Authorization: Bearer {token}"
```

### Ejemplo 4: Descargar PDF
```bash
curl -X GET https://api.adopets.com/api/tickets/12345678-1234-1234-1234-123456789012/pdf \
  -H "Authorization: Bearer {token}" \
  --output ticket.pdf
```

### Ejemplo 5: Buscar por Número de Ticket
```bash
curl -X GET https://api.adopets.com/api/tickets/numero/TICKET-20240115-001 \
  -H "Authorization: Bearer {token}"
```

---

## Integración con Otros Módulos

### Módulo de Citas
- Los tickets se pueden vincular a citas específicas
- Al completar una cita, se puede generar automáticamente un ticket

### Módulo de Pagos
- Los tickets sirven como comprobante antes del pago
- Un ticket puede tener múltiples pagos asociados (anticipos)

### Módulo de Clientes/Usuarios
- Histórico de tickets por cliente
- Estadísticas de consumo

### Módulo de Servicios
- Catálogo de servicios con precios predefinidos
- Actualización automática de precios

---

## Casos de Uso Comunes

### Caso 1: Consulta Veterinaria Completa
```
1. Veterinario atiende cita
2. Realiza procedimientos (consulta, vacunas, medicamentos)
3. Recepcionista crea ticket con todos los servicios
4. Se genera PDF del ticket
5. Cliente paga y recibe ticket
6. Ticket se marca como entregado
```

### Caso 2: Ticket de Emergencia
```
1. Mascota llega a emergencia sin cita previa
2. Se atiende de inmediato
3. Al finalizar, se crea ticket sin CitaId
4. Se generan detalles de servicios de emergencia
5. Cliente paga y recibe comprobante
```

### Caso 3: Ticket con Descuento Promocional
```
1. Cliente trae cupón de descuento del 15%
2. Se crea ticket normal con servicios
3. Se aplica porcentajeDescuento = 15
4. Sistema calcula automáticamente el nuevo total
5. PDF muestra el descuento aplicado
```

---

## Notas Técnicas

### Performance
- Generación de PDF es síncrona, puede tomar 1-3 segundos
- Se recomienda cachear PDFs frecuentemente solicitados
- Índice en `NumeroTicket` para búsquedas rápidas

### Seguridad
- Los tickets contienen información sensible (precios, servicios)
- Solo el cliente propietario y personal autorizado pueden ver tickets
- Los PDFs no contienen información de pago

### Auditoría
- Cada ticket registra quién lo creó y cuándo
- Se registra quién marcó el ticket como entregado
- No se permite eliminar tickets, solo cancelar

### Impresión
- Los PDFs están optimizados para impresión en papel tamaño carta
- Se recomienda impresión térmica para tickets físicos en recepción

---

## Buenas Prácticas

1. **Descripción Detallada:** Incluir descripciones claras de servicios
2. **Verificar Precios:** Validar precios antes de crear ticket
3. **Notas Importantes:** Usar campo de notas para información relevante
4. **PDF Inmediato:** Generar PDF inmediatamente al crear ticket
5. **Control de Entrega:** Marcar como entregado solo cuando el cliente reciba físicamente el ticket

---

## Contacto y Soporte
**Desarrollador Responsable:** Developer 3 - Beto  
**Módulo:** Clínica & Historial Médico  
**Versión API:** 1.0  
**Última Actualización:** Enero 2024  
**Librería PDF:** QuestPDF v2024.x
