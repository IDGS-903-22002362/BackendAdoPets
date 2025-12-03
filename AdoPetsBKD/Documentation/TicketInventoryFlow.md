# Flujo de Descuento Automático de Insumos en Tickets

## Descripción General

Cuando un veterinario termina de atender una cita y genera un ticket, el sistema automáticamente descuenta los insumos utilizados del inventario aplicando lógica FIFO (First In, First Out).

## Flujo de Operación

### 1. Creación del Ticket

El veterinario crea un ticket mediante el endpoint:
```
POST /api/v1/tickets
```

### 2. Especificación de Insumos

En el array `Detalles` del ticket, se especifica cada insumo utilizado:

```json
{
  "citaId": "guid",
  "mascotaId": "guid",
  "clienteId": "guid",
  "veterinarioId": "guid",
  "fechaProcedimiento": "2025-12-03T10:00:00Z",
  "nombreProcedimiento": "Cirugía de esterilización",
  "costoProcedimiento": 1500.00,
  "costoInsumos": 300.00,
  "detalles": [
    {
      "descripcion": "Anestesia general",
      "cantidad": 2,
      "unidad": "ml",
      "precioUnitario": 150.00,
      "itemInventarioId": "guid-del-item",
      "tipo": 2
    },
    {
      "descripcion": "Sutura absorbible",
      "cantidad": 1,
      "unidad": "paquete",
      "precioUnitario": 100.00,
      "itemInventarioId": "guid-del-item",
      "tipo": 2
    }
  ]
}
```

### 3. Descuento Automático de Inventario

Por cada detalle que tiene `itemInventarioId`, el sistema:

#### 3.1 Validaciones
- ? Verifica que el item existe
- ? Obtiene lotes disponibles ordenados por FIFO
- ? Valida que hay stock suficiente
- ? Verifica que los lotes no estén vencidos

#### 3.2 Descuento con FIFO
El sistema descuenta siguiendo el orden FIFO:
1. Primero descuenta de lotes que vencen más pronto
2. Si un lote no tiene fecha de vencimiento, lo ordena por fecha de creación
3. Puede dividir la cantidad entre múltiples lotes si es necesario

**Ejemplo:**
```
Requerido: 5 ml de anestesia

Lotes disponibles (ordenados por FIFO):
- Lote A: 3 ml disponibles, vence 2025-01-15
- Lote B: 10 ml disponibles, vence 2025-03-20

Resultado:
- Lote A: Descuenta 3 ml ? Queda 0 ml
- Lote B: Descuenta 2 ml ? Queda 8 ml
```

#### 3.3 Registro de Movimientos
Por cada descuento, se crea un `MovimientoInventario`:
```csharp
{
    Tipo: TipoMovimiento.Salida,
    Qty: cantidadDescontada,
    Reason: "Consumo en ticket - Cita {citaId}",
    ItemId: itemId,
    BatchId: loteId,
    RelatedAppointmentId: citaId,
    PerformedBy: veterinarioId,
    Observaciones: "Ticket: {ticketId}, Lote: {numeroLote}"
}
```

### 4. Manejo Transaccional

Todo el proceso se ejecuta dentro de una transacción de base de datos:
- Si ocurre algún error (falta de stock, lote vencido, etc.), se hace **rollback**
- El ticket **NO** se crea si falla el descuento de inventario
- Garantiza consistencia total de datos

## Tipos de Error

### Stock Insuficiente
```json
{
  "success": false,
  "message": "Stock insuficiente para 'Anestesia General'. Requerido: 10 ml, Disponible: 7 ml"
}
```

### Lote Vencido
```json
{
  "success": false,
  "message": "El lote 'LOTE-001' del item 'Sutura' está vencido. Fecha de vencimiento: 01/12/2024"
}
```

### Item No Existe
```json
{
  "success": false,
  "message": "El item de inventario {guid} no existe"
}
```

## Tipos de Detalle

El campo `tipo` en los detalles puede ser:

```csharp
public enum TipoDetalleTicket
{
    Procedimiento = 1,  // No descuenta inventario
    Insumo = 2,         // Descuenta si tiene itemInventarioId
    Medicamento = 3,    // Descuenta si tiene itemInventarioId
    Consulta = 4,       // No descuenta inventario
    Otro = 5            // Descuenta si tiene itemInventarioId
}
```

## Consulta de Movimientos

Para auditar los movimientos de inventario relacionados con un ticket:

```sql
SELECT 
    m.CreatedAt,
    i.Nombre AS Item,
    m.Qty AS Cantidad,
    i.Unidad,
    l.Lote,
    m.Reason,
    m.Observaciones
FROM MovimientosInventario m
INNER JOIN ItemsInventario i ON m.ItemId = i.Id
LEFT JOIN LotesInventario l ON m.BatchId = l.Id
WHERE m.Tipo = 2 -- Salida
  AND m.Observaciones LIKE '%Ticket: {ticketId}%'
ORDER BY m.CreatedAt
```

## Flujo Visual

```
???????????????????????
?  Veterinario crea   ?
?  ticket con detalles?
???????????????????????
           ?
           ?
???????????????????????
? Validar stock       ?
? disponible          ?
???????????????????????
           ?
           ?
???????????????????????
? Descontar de lotes  ?
? aplicando FIFO      ?
???????????????????????
           ?
           ?
???????????????????????
? Registrar           ?
? movimientos         ?
???????????????????????
           ?
           ?
???????????????????????
? Crear ticket        ?
? y guardar           ?
???????????????????????
```

## Consideraciones Importantes

1. **Detalles Opcionales**: No todos los detalles requieren `itemInventarioId`
   - Solo se descuenta inventario si el campo está presente
   - Útil para cobros de servicios/consultas que no consumen insumos

2. **Cantidad en Unidades del Item**: La cantidad debe estar en las unidades del item
   - Si el item está en "ml", la cantidad debe ser en ml
   - Si el item está en "piezas", la cantidad debe ser en piezas

3. **Alertas de Stock Bajo**: Después del descuento, el sistema puede generar alertas
   - Se verifica si `StockTotal < MinQty`
   - Se pueden configurar notificaciones automáticas

4. **Trazabilidad Completa**: 
   - Cada movimiento queda registrado
   - Se puede rastrear qué veterinario usó qué insumo
   - Se vincula con la cita y el ticket

## Ejemplo Completo

```json
POST /api/v1/tickets
{
  "citaId": "a1b2c3d4-...",
  "mascotaId": "e5f6g7h8-...",
  "clienteId": "i9j0k1l2-...",
  "veterinarioId": "m3n4o5p6-...",
  "fechaProcedimiento": "2025-12-03T14:30:00Z",
  "nombreProcedimiento": "Vacunación múltiple",
  "descripcionProcedimiento": "Vacunas anuales de refuerzo",
  "costoProcedimiento": 500.00,
  "costoInsumos": 200.00,
  "costoAdicional": 0,
  "descuento": 50.00,
  "diagnostico": "Animal sano",
  "tratamiento": "Aplicación de vacunas",
  "detalles": [
    {
      "descripcion": "Consulta general",
      "cantidad": 1,
      "unidad": "servicio",
      "precioUnitario": 300.00,
      "tipo": 4
      // Sin itemInventarioId - no descuenta inventario
    },
    {
      "descripcion": "Vacuna Rabia",
      "cantidad": 1,
      "unidad": "dosis",
      "precioUnitario": 150.00,
      "itemInventarioId": "vacuna-rabia-guid",
      "tipo": 3
      // Con itemInventarioId - descuenta 1 dosis del inventario
    },
    {
      "descripcion": "Vacuna Séxtuple",
      "cantidad": 1,
      "unidad": "dosis",
      "precioUnitario": 180.00,
      "itemInventarioId": "vacuna-sextuple-guid",
      "tipo": 3
      // Con itemInventarioId - descuenta 1 dosis del inventario
    }
  ]
}
```

**Resultado:**
- ? Ticket creado con número TK-20251203-XXXX
- ? Descuenta 1 dosis de Vacuna Rabia
- ? Descuenta 1 dosis de Vacuna Séxtuple
- ? NO descuenta nada por la consulta general
- ? Registra 2 movimientos de tipo Salida
- ? Total calculado: (500 + 200) - 50 + IVA = $754.00
