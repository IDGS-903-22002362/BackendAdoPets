# Script de Pruebas - Descuento de Inventario en Tickets

Este documento contiene scripts SQL y llamadas API para probar el flujo completo de descuento de inventario.

## Pre-requisitos

1. Base de datos configurada
2. API en ejecución
3. Token de autenticación válido
4. Datos base: usuarios, cita, mascota, items de inventario con lotes

---

## Paso 1: Preparar Datos de Prueba

### 1.1 Crear Items de Inventario

```sql
-- Insertar items de inventario para pruebas
INSERT INTO ItemsInventario (Id, Nombre, Unidad, Categoria, MinQty, Activo, Descripcion)
VALUES 
  (NEWID(), 'Anestesia Isoflurano', 'ml', 1, 100, 1, 'Anestésico inhalatorio'),
  (NEWID(), 'Sutura Absorbible 3-0', 'sobres', 4, 10, 1, 'Sutura quirúrgica'),
  (NEWID(), 'Vacuna Antirrábica', 'dosis', 2, 20, 1, 'Vacuna contra rabia'),
  (NEWID(), 'Tramadol Inyectable', 'ml', 1, 50, 1, 'Analgésico opioide');

-- Guardar los IDs generados para uso posterior
SELECT Id, Nombre, Unidad FROM ItemsInventario WHERE Nombre IN (
  'Anestesia Isoflurano',
  'Sutura Absorbible 3-0',
  'Vacuna Antirrábica',
  'Tramadol Inyectable'
);
```

### 1.2 Crear Lotes de Inventario

```sql
-- Anestesia: 200 ml disponibles en 2 lotes
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible, CreatedAt)
VALUES
  (NEWID(), 
   (SELECT Id FROM ItemsInventario WHERE Nombre = 'Anestesia Isoflurano'),
   'LOTE-ANE-001',
   DATEADD(MONTH, 6, GETDATE()),
   100,
   100,
   GETDATE()),
  (NEWID(),
   (SELECT Id FROM ItemsInventario WHERE Nombre = 'Anestesia Isoflurano'),
   'LOTE-ANE-002',
   DATEADD(MONTH, 12, GETDATE()),
   100,
   100,
   GETDATE());

-- Sutura: 20 sobres disponibles
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible, CreatedAt)
VALUES
  (NEWID(),
   (SELECT Id FROM ItemsInventario WHERE Nombre = 'Sutura Absorbible 3-0'),
   'LOTE-SUT-001',
   DATEADD(YEAR, 2, GETDATE()),
   20,
   20,
   GETDATE());

-- Vacuna: 30 dosis disponibles
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible, CreatedAt)
VALUES
  (NEWID(),
   (SELECT Id FROM ItemsInventario WHERE Nombre = 'Vacuna Antirrábica'),
   'LOTE-VAC-001',
   DATEADD(MONTH, 3, GETDATE()),
   30,
   30,
   GETDATE());

-- Tramadol: 80 ml disponibles
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible, CreatedAt)
VALUES
  (NEWID(),
   (SELECT Id FROM ItemsInventario WHERE Nombre = 'Tramadol Inyectable'),
   'LOTE-TRA-001',
   DATEADD(MONTH, 9, GETDATE()),
   80,
   80,
   GETDATE());
```

### 1.3 Verificar Stock Inicial

```sql
SELECT 
  i.Nombre AS Item,
  i.Unidad,
  l.Lote,
  l.QtyDisponible AS 'Stock Disponible',
  l.ExpDate AS 'Fecha Vencimiento',
  DATEDIFF(DAY, GETDATE(), l.ExpDate) AS 'Días para Vencer'
FROM ItemsInventario i
INNER JOIN LotesInventario l ON i.Id = l.ItemId
WHERE i.Activo = 1
ORDER BY i.Nombre, l.ExpDate;
```

**Resultado Esperado:**
```
Item                      | Unidad | Lote          | Stock | Fecha Venc. | Días
--------------------------|--------|---------------|-------|-------------|------
Anestesia Isoflurano      | ml     | LOTE-ANE-001  | 100   | 2025-06-03  | 180
Anestesia Isoflurano      | ml     | LOTE-ANE-002  | 100   | 2025-12-03  | 365
Sutura Absorbible 3-0     | sobres | LOTE-SUT-001  | 20    | 2027-12-03  | 730
Tramadol Inyectable       | ml     | LOTE-TRA-001  | 80    | 2026-09-03  | 640
Vacuna Antirrábica        | dosis  | LOTE-VAC-001  | 30    | 2025-03-03  | 90
```

---

## Paso 2: Crear una Cita de Prueba

```sql
-- Verificar IDs necesarios
SELECT Id, Nombre FROM Mascotas WHERE Activo = 1;
SELECT Id, NombreCompleto FROM Usuarios WHERE Rol = 'Cliente';
SELECT Id, NombreCompleto FROM Usuarios WHERE Rol = 'Veterinario';

-- Crear cita
INSERT INTO Citas (Id, MascotaId, VeterinarioId, ClienteId, FechaHora, Motivo, Estado, CreatedAt)
VALUES (
  NEWID(),
  -- Reemplazar con IDs reales
  'GUID-MASCOTA',
  'GUID-VETERINARIO',
  'GUID-CLIENTE',
  GETDATE(),
  'Cirugía de esterilización',
  1, -- Programada
  GETDATE()
);

-- Guardar el ID de la cita
SELECT TOP 1 Id, Motivo FROM Citas ORDER BY CreatedAt DESC;
```

---

## Paso 3: Crear Ticket con Descuento de Inventario

### Request API

**IMPORTANTE:** Reemplazar los GUIDs con los valores reales de tu base de datos.

```bash
curl -X POST https://localhost:5001/api/v1/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -d '{
    "citaId": "GUID-CITA-REAL",
    "mascotaId": "GUID-MASCOTA-REAL",
    "clienteId": "GUID-CLIENTE-REAL",
    "veterinarioId": "GUID-VETERINARIO-REAL",
    "fechaProcedimiento": "2025-12-03T10:00:00Z",
    "nombreProcedimiento": "Cirugía de Esterilización",
    "descripcionProcedimiento": "Ovariohisterectomía completa",
    "costoProcedimiento": 1500.00,
    "costoInsumos": 380.00,
    "costoAdicional": 0,
    "descuento": 0,
    "diagnostico": "Mascota sana, apta para cirugía",
    "tratamiento": "Cirugía realizada exitosamente",
    "medicacionPrescrita": "Tramadol cada 12h por 3 días",
    "detalles": [
      {
        "descripcion": "Procedimiento quirúrgico",
        "cantidad": 1,
        "unidad": "servicio",
        "precioUnitario": 1500.00,
        "tipo": 1
      },
      {
        "descripcion": "Anestesia Isoflurano",
        "cantidad": 15,
        "unidad": "ml",
        "precioUnitario": 10.00,
        "itemInventarioId": "GUID-ANESTESIA-REAL",
        "tipo": 2
      },
      {
        "descripcion": "Sutura Absorbible 3-0",
        "cantidad": 2,
        "unidad": "sobres",
        "precioUnitario": 80.00,
        "itemInventarioId": "GUID-SUTURA-REAL",
        "tipo": 2
      },
      {
        "descripcion": "Tramadol Inyectable",
        "cantidad": 5,
        "unidad": "ml",
        "precioUnitario": 30.00,
        "itemInventarioId": "GUID-TRAMADOL-REAL",
        "tipo": 3
      }
    ]
  }'
```

### Response Esperada (Exitosa)

```json
{
  "success": true,
  "data": {
    "id": "nuevo-ticket-guid",
    "numeroTicket": "TK-20251203-XXXX",
    "subtotal": 1880.00,
    "iva": 300.80,
    "total": 2180.80,
    "estado": 1,
    "estadoNombre": "Generado",
    "detalles": [
      // ... detalles del ticket
    ]
  },
  "message": "Ticket creado exitosamente"
}
```

---

## Paso 4: Verificar Descuentos en Inventario

### 4.1 Verificar Stock Actualizado

```sql
SELECT 
  i.Nombre AS Item,
  l.Lote,
  l.QtyInicial AS 'Stock Inicial',
  l.QtyDisponible AS 'Stock Actual',
  (l.QtyInicial - l.QtyDisponible) AS 'Consumido'
FROM ItemsInventario i
INNER JOIN LotesInventario l ON i.Id = l.ItemId
WHERE i.Nombre IN (
  'Anestesia Isoflurano',
  'Sutura Absorbible 3-0',
  'Tramadol Inyectable'
)
ORDER BY i.Nombre, l.ExpDate;
```

**Resultado Esperado:**
```
Item                      | Lote          | Inicial | Actual | Consumido
--------------------------|---------------|---------|--------|----------
Anestesia Isoflurano      | LOTE-ANE-001  | 100     | 85     | 15
Anestesia Isoflurano      | LOTE-ANE-002  | 100     | 100    | 0
Sutura Absorbible 3-0     | LOTE-SUT-001  | 20      | 18     | 2
Tramadol Inyectable       | LOTE-TRA-001  | 80      | 75     | 5
```

? **FIFO funcionó correctamente:** La anestesia se descontó del lote que vence primero.

### 4.2 Verificar Movimientos de Inventario

```sql
SELECT 
  m.CreatedAt AS 'Fecha',
  i.Nombre AS 'Item',
  l.Lote AS 'Lote',
  m.Tipo AS 'Tipo',
  m.Qty AS 'Cantidad',
  i.Unidad,
  m.Reason AS 'Razón',
  m.Observaciones
FROM MovimientosInventario m
INNER JOIN ItemsInventario i ON m.ItemId = i.Id
LEFT JOIN LotesInventario l ON m.BatchId = l.Id
WHERE m.Tipo = 2 -- Salida
  AND m.CreatedAt >= DATEADD(MINUTE, -10, GETDATE())
ORDER BY m.CreatedAt DESC;
```

**Resultado Esperado:**
```
Fecha                | Item                | Lote         | Tipo | Cantidad | Razón
---------------------|---------------------|--------------|------|----------|---------------------------
2025-12-03 10:45:00  | Tramadol Inyectable | LOTE-TRA-001 | 2    | 5        | Consumo en ticket - Cita...
2025-12-03 10:45:00  | Sutura Absorbible   | LOTE-SUT-001 | 2    | 2        | Consumo en ticket - Cita...
2025-12-03 10:45:00  | Anestesia Isoflurano| LOTE-ANE-001 | 2    | 15       | Consumo en ticket - Cita...
```

### 4.3 Verificar Ticket Creado

```sql
SELECT 
  t.NumeroTicket,
  t.NombreProcedimiento,
  t.Subtotal,
  t.IVA,
  t.Total,
  t.Estado,
  COUNT(d.Id) AS 'Total Detalles'
FROM Tickets t
LEFT JOIN TicketDetalles d ON t.Id = d.TicketId
WHERE t.NumeroTicket LIKE 'TK-20251203-%'
GROUP BY t.NumeroTicket, t.NombreProcedimiento, t.Subtotal, t.IVA, t.Total, t.Estado;
```

---

## Paso 5: Probar Casos de Error

### Test 5.1: Stock Insuficiente

```bash
curl -X POST https://localhost:5001/api/v1/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -d '{
    "citaId": "GUID-CITA-REAL",
    "mascotaId": "GUID-MASCOTA-REAL",
    "clienteId": "GUID-CLIENTE-REAL",
    "veterinarioId": "GUID-VETERINARIO-REAL",
    "fechaProcedimiento": "2025-12-03T11:00:00Z",
    "nombreProcedimiento": "Prueba Stock Insuficiente",
    "costoProcedimiento": 100.00,
    "costoInsumos": 3000.00,
    "detalles": [
      {
        "descripcion": "Anestesia Isoflurano",
        "cantidad": 500,
        "unidad": "ml",
        "precioUnitario": 10.00,
        "itemInventarioId": "GUID-ANESTESIA-REAL",
        "tipo": 2
      }
    ]
  }'
```

**Response Esperada:**
```json
{
  "success": false,
  "data": null,
  "message": "Stock insuficiente para 'Anestesia Isoflurano'. Requerido: 500 ml, Disponible: 185 ml"
}
```

**Verificar que NO se creó el ticket:**
```sql
SELECT COUNT(*) AS 'Tickets con error'
FROM Tickets 
WHERE NombreProcedimiento = 'Prueba Stock Insuficiente';
-- Debe retornar 0
```

### Test 5.2: Lote Vencido

```sql
-- Crear un item con lote vencido
INSERT INTO ItemsInventario (Id, Nombre, Unidad, Categoria, MinQty, Activo)
VALUES (NEWID(), 'Medicamento Vencido Test', 'ml', 1, 10, 1);

DECLARE @ItemVencidoId uniqueidentifier = (
  SELECT Id FROM ItemsInventario WHERE Nombre = 'Medicamento Vencido Test'
);

INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible)
VALUES (NEWID(), @ItemVencidoId, 'LOTE-VENCIDO-001', DATEADD(DAY, -30, GETDATE()), 100, 100);
```

```bash
curl -X POST https://localhost:5001/api/v1/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -d '{
    "citaId": "GUID-CITA-REAL",
    "mascotaId": "GUID-MASCOTA-REAL",
    "clienteId": "GUID-CLIENTE-REAL",
    "veterinarioId": "GUID-VETERINARIO-REAL",
    "fechaProcedimiento": "2025-12-03T12:00:00Z",
    "nombreProcedimiento": "Prueba Lote Vencido",
    "costoProcedimiento": 100.00,
    "costoInsumos": 50.00,
    "detalles": [
      {
        "descripcion": "Medicamento Vencido",
        "cantidad": 10,
        "unidad": "ml",
        "precioUnitario": 5.00,
        "itemInventarioId": "GUID-MEDICAMENTO-VENCIDO-REAL",
        "tipo": 3
      }
    ]
  }'
```

**Response Esperada:**
```json
{
  "success": false,
  "data": null,
  "message": "El lote 'LOTE-VENCIDO-001' del item 'Medicamento Vencido Test' está vencido. Fecha de vencimiento: 03/11/2024"
}
```

---

## Paso 6: Probar Lógica FIFO

### Setup: Crear múltiples lotes con diferentes fechas de vencimiento

```sql
-- Crear item para prueba FIFO
INSERT INTO ItemsInventario (Id, Nombre, Unidad, Categoria, MinQty, Activo)
VALUES (NEWID(), 'Suero FIFO Test', 'ml', 1, 100, 1);

DECLARE @ItemFifoId uniqueidentifier = (
  SELECT Id FROM ItemsInventario WHERE Nombre = 'Suero FIFO Test'
);

-- Lote 1: Vence en 2 meses (debe usarse primero)
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible)
VALUES (NEWID(), @ItemFifoId, 'LOTE-FIFO-001', DATEADD(MONTH, 2, GETDATE()), 50, 50);

-- Lote 2: Vence en 6 meses (debe usarse segundo)
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible)
VALUES (NEWID(), @ItemFifoId, 'LOTE-FIFO-002', DATEADD(MONTH, 6, GETDATE()), 100, 100);

-- Lote 3: Vence en 12 meses (debe usarse último)
INSERT INTO LotesInventario (Id, ItemId, Lote, ExpDate, QtyInicial, QtyDisponible)
VALUES (NEWID(), @ItemFifoId, 'LOTE-FIFO-003', DATEADD(YEAR, 1, GETDATE()), 200, 200);
```

### Crear ticket que consume de múltiples lotes

```bash
curl -X POST https://localhost:5001/api/v1/tickets \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TU_TOKEN}" \
  -d '{
    "citaId": "GUID-CITA-REAL",
    "mascotaId": "GUID-MASCOTA-REAL",
    "clienteId": "GUID-CLIENTE-REAL",
    "veterinarioId": "GUID-VETERINARIO-REAL",
    "fechaProcedimiento": "2025-12-03T13:00:00Z",
    "nombreProcedimiento": "Prueba FIFO",
    "costoProcedimiento": 100.00,
    "costoInsumos": 120.00,
    "detalles": [
      {
        "descripcion": "Suero FIFO Test",
        "cantidad": 120,
        "unidad": "ml",
        "precioUnitario": 1.00,
        "itemInventarioId": "GUID-SUERO-FIFO-REAL",
        "tipo": 2
      }
    ]
  }'
```

### Verificar descuento FIFO

```sql
SELECT 
  l.Lote,
  l.ExpDate AS 'Vencimiento',
  l.QtyInicial AS 'Inicial',
  l.QtyDisponible AS 'Disponible',
  (l.QtyInicial - l.QtyDisponible) AS 'Consumido'
FROM LotesInventario l
INNER JOIN ItemsInventario i ON l.ItemId = i.Id
WHERE i.Nombre = 'Suero FIFO Test'
ORDER BY l.ExpDate;
```

**Resultado Esperado:**
```
Lote          | Vencimiento | Inicial | Disponible | Consumido
--------------|-------------|---------|------------|----------
LOTE-FIFO-001 | 2025-02-03  | 50      | 0          | 50      ? (agotado primero)
LOTE-FIFO-002 | 2025-06-03  | 100     | 30         | 70      ? (consumió el resto)
LOTE-FIFO-003 | 2025-12-03  | 200     | 200        | 0       ? (no se tocó)
```

Total consumido: 50 + 70 = 120 ml ?

---

## Limpieza de Datos de Prueba

```sql
-- Eliminar tickets de prueba
DELETE FROM TicketDetalles WHERE TicketId IN (
  SELECT Id FROM Tickets WHERE NombreProcedimiento LIKE '%Prueba%'
);
DELETE FROM Tickets WHERE NombreProcedimiento LIKE '%Prueba%';

-- Eliminar movimientos de prueba
DELETE FROM MovimientosInventario WHERE Reason LIKE '%Test%';

-- Eliminar lotes de prueba
DELETE FROM LotesInventario WHERE Lote LIKE '%TEST%' OR Lote LIKE '%FIFO%';

-- Eliminar items de prueba
DELETE FROM ItemsInventario WHERE Nombre LIKE '%Test%';
```

---

## Checklist de Validación

- [ ] Stock se descuenta correctamente del inventario
- [ ] Lógica FIFO funciona (consume primero los que vencen antes)
- [ ] Movimientos de inventario se registran correctamente
- [ ] Error de stock insuficiente previene creación de ticket
- [ ] Error de lote vencido previene creación de ticket
- [ ] Transacción hace rollback en caso de error
- [ ] Detalles sin itemInventarioId no intentan descontar inventario
- [ ] Múltiples lotes se consumen correctamente en un solo ticket
- [ ] Ticket se crea solo si todos los descuentos son exitosos
