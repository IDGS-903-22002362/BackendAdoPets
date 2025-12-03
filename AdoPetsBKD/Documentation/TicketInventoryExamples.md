# Ejemplos de Uso - API de Tickets con Descuento de Inventario

## Caso de Uso 1: Cirugía con Múltiples Insumos

### Escenario
Un veterinario realiza una cirugía de esterilización y necesita registrar el consumo de varios insumos.

### Request

```http
POST /api/v1/tickets
Authorization: Bearer {token}
Content-Type: application/json
```

```json
{
  "citaId": "550e8400-e29b-41d4-a716-446655440000",
  "mascotaId": "660e8400-e29b-41d4-a716-446655440001",
  "clienteId": "770e8400-e29b-41d4-a716-446655440002",
  "veterinarioId": "880e8400-e29b-41d4-a716-446655440003",
  "fechaProcedimiento": "2025-12-03T10:00:00Z",
  "nombreProcedimiento": "Esterilización felina",
  "descripcionProcedimiento": "Ovariohisterectomía completa",
  "costoProcedimiento": 1800.00,
  "costoInsumos": 450.00,
  "costoAdicional": 100.00,
  "descuento": 0,
  "diagnostico": "Gata de 8 meses, sana para cirugía",
  "tratamiento": "Cirugía realizada sin complicaciones",
  "medicacionPrescrita": "Tramadol cada 12h por 3 días, Meloxicam cada 24h por 5 días",
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
      "itemInventarioId": "anestesia-iso-guid",
      "tipo": 2
    },
    {
      "descripcion": "Sutura absorbible 3-0",
      "cantidad": 2,
      "unidad": "sobres",
      "precioUnitario": 80.00,
      "itemInventarioId": "sutura-abs-guid",
      "tipo": 2
    },
    {
      "descripcion": "Guantes quirúrgicos",
      "cantidad": 2,
      "unidad": "pares",
      "precioUnitario": 15.00,
      "itemInventarioId": "guantes-guid",
      "tipo": 2
    },
    {
      "descripcion": "Gasas estériles",
      "cantidad": 10,
      "unidad": "piezas",
      "precioUnitario": 5.00,
      "itemInventarioId": "gasas-guid",
      "tipo": 2
    },
    {
      "descripcion": "Tramadol inyectable",
      "cantidad": 3,
      "unidad": "ml",
      "precioUnitario": 25.00,
      "itemInventarioId": "tramadol-guid",
      "tipo": 3
    },
    {
      "descripcion": "Meloxicam inyectable",
      "cantidad": 2,
      "unidad": "ml",
      "precioUnitario": 30.00,
      "itemInventarioId": "meloxicam-guid",
      "tipo": 3
    }
  ]
}
```

### Response Exitosa

```json
{
  "success": true,
  "data": {
    "id": "990e8400-e29b-41d4-a716-446655440004",
    "numeroTicket": "TK-20251203-4521",
    "citaId": "550e8400-e29b-41d4-a716-446655440000",
    "mascotaId": "660e8400-e29b-41d4-a716-446655440001",
    "nombreMascota": "Luna",
    "clienteId": "770e8400-e29b-41d4-a716-446655440002",
    "nombreCliente": "Juan Pérez García",
    "veterinarioId": "880e8400-e29b-41d4-a716-446655440003",
    "nombreVeterinario": "Dra. María López",
    "fechaProcedimiento": "2025-12-03T10:00:00Z",
    "nombreProcedimiento": "Esterilización felina",
    "descripcionProcedimiento": "Ovariohisterectomía completa",
    "costoProcedimiento": 1800.00,
    "costoInsumos": 450.00,
    "costoAdicional": 100.00,
    "subtotal": 2350.00,
    "descuento": 0.00,
    "iva": 376.00,
    "total": 2726.00,
    "diagnostico": "Gata de 8 meses, sana para cirugía",
    "tratamiento": "Cirugía realizada sin complicaciones",
    "medicacionPrescrita": "Tramadol cada 12h por 3 días, Meloxicam cada 24h por 5 días",
    "estado": 1,
    "estadoNombre": "Generado",
    "fechaEntrega": null,
    "pagoId": null,
    "detalles": [
      {
        "id": "det-1",
        "descripcion": "Procedimiento quirúrgico",
        "cantidad": 1,
        "unidad": "servicio",
        "precioUnitario": 1500.00,
        "subtotal": 1500.00,
        "tipo": 1,
        "tipoNombre": "Procedimiento"
      },
      {
        "id": "det-2",
        "descripcion": "Anestesia Isoflurano",
        "cantidad": 15,
        "unidad": "ml",
        "precioUnitario": 10.00,
        "subtotal": 150.00,
        "tipo": 2,
        "tipoNombre": "Insumo"
      }
      // ... más detalles
    ],
    "createdAt": "2025-12-03T10:45:00Z"
  },
  "message": "Ticket creado exitosamente"
}
```

### ¿Qué pasó en el inventario?

El sistema automáticamente:
1. ? Descontó 15 ml de Anestesia Isoflurano
2. ? Descontó 2 sobres de Sutura absorbible
3. ? Descontó 2 pares de Guantes quirúrgicos
4. ? Descontó 10 piezas de Gasas estériles
5. ? Descontó 3 ml de Tramadol
6. ? Descontó 2 ml de Meloxicam
7. ? Creó 6 movimientos de inventario tipo "Salida"
8. ? Aplicó lógica FIFO en cada descuento

---

## Caso de Uso 2: Consulta Simple con Vacunación

### Escenario
Consulta de rutina con aplicación de vacuna anual.

### Request

```json
{
  "citaId": "551e8400-e29b-41d4-a716-446655440010",
  "mascotaId": "661e8400-e29b-41d4-a716-446655440011",
  "clienteId": "771e8400-e29b-41d4-a716-446655440012",
  "veterinarioId": "881e8400-e29b-41d4-a716-446655440013",
  "fechaProcedimiento": "2025-12-03T15:00:00Z",
  "nombreProcedimiento": "Consulta y Vacunación",
  "descripcionProcedimiento": "Revisión general y aplicación de vacuna antirrábica",
  "costoProcedimiento": 300.00,
  "costoInsumos": 150.00,
  "costoAdicional": 0,
  "descuento": 0,
  "diagnostico": "Perro sano, apto para vacunación",
  "tratamiento": "Aplicación de vacuna antirrábica",
  "detalles": [
    {
      "descripcion": "Consulta general",
      "cantidad": 1,
      "unidad": "servicio",
      "precioUnitario": 300.00,
      "tipo": 4
    },
    {
      "descripcion": "Vacuna Antirrábica",
      "cantidad": 1,
      "unidad": "dosis",
      "precioUnitario": 150.00,
      "itemInventarioId": "vacuna-rabia-guid",
      "tipo": 3
    }
  ]
}
```

### Response

```json
{
  "success": true,
  "data": {
    "numeroTicket": "TK-20251203-4522",
    "subtotal": 450.00,
    "descuento": 0.00,
    "iva": 72.00,
    "total": 522.00,
    // ... más campos
  },
  "message": "Ticket creado exitosamente"
}
```

### ¿Qué pasó en el inventario?

1. ? Descontó 1 dosis de Vacuna Antirrábica
2. ? NO descontó nada por la consulta (no tiene itemInventarioId)
3. ? Creó 1 movimiento de inventario

---

## Caso de Uso 3: Error por Stock Insuficiente

### Request

```json
{
  "citaId": "552e8400-e29b-41d4-a716-446655440020",
  "mascotaId": "662e8400-e29b-41d4-a716-446655440021",
  "clienteId": "772e8400-e29b-41d4-a716-446655440022",
  "veterinarioId": "882e8400-e29b-41d4-a716-446655440023",
  "fechaProcedimiento": "2025-12-03T16:00:00Z",
  "nombreProcedimiento": "Aplicación de suero",
  "costoProcedimiento": 200.00,
  "costoInsumos": 100.00,
  "detalles": [
    {
      "descripcion": "Suero Hartmann",
      "cantidad": 1000,
      "unidad": "ml",
      "precioUnitario": 0.10,
      "itemInventarioId": "suero-hartmann-guid",
      "tipo": 2
    }
  ]
}
```

### Response de Error

```json
{
  "success": false,
  "data": null,
  "message": "Stock insuficiente para 'Suero Hartmann'. Requerido: 1000 ml, Disponible: 500 ml"
}
```

**Importante:** El ticket NO se creó y NO se descontó nada del inventario.

---

## Caso de Uso 4: Error por Lote Vencido

### Request

```json
{
  "citaId": "553e8400-e29b-41d4-a716-446655440030",
  "mascotaId": "663e8400-e29b-41d4-a716-446655440031",
  "clienteId": "773e8400-e29b-41d4-a716-446655440032",
  "veterinarioId": "883e8400-e29b-41d4-a716-446655440033",
  "fechaProcedimiento": "2025-12-03T17:00:00Z",
  "nombreProcedimiento": "Aplicación de medicamento",
  "costoProcedimiento": 150.00,
  "costoInsumos": 80.00,
  "detalles": [
    {
      "descripcion": "Antibiótico Amoxicilina",
      "cantidad": 5,
      "unidad": "ml",
      "precioUnitario": 16.00,
      "itemInventarioId": "amoxicilina-guid",
      "tipo": 3
    }
  ]
}
```

### Response de Error

```json
{
  "success": false,
  "data": null,
  "message": "El lote 'LOTE-1234' del item 'Amoxicilina inyectable' está vencido. Fecha de vencimiento: 01/11/2024"
}
```

**Importante:** 
- El sistema detectó que el único lote disponible está vencido
- El ticket NO se creó
- NO se descontó inventario
- Se debe dar de baja el lote vencido antes de poder usar el medicamento

---

## Integración con Frontend

### Hook de React (ejemplo)

```typescript
interface CreateTicketDto {
  citaId: string;
  mascotaId?: string;
  clienteId: string;
  veterinarioId: string;
  fechaProcedimiento: string;
  nombreProcedimiento: string;
  descripcionProcedimiento?: string;
  costoProcedimiento: number;
  costoInsumos: number;
  costoAdicional: number;
  descuento: number;
  diagnostico?: string;
  tratamiento?: string;
  medicacionPrescrita?: string;
  detalles: TicketDetalleDto[];
}

interface TicketDetalleDto {
  descripcion: string;
  cantidad: number;
  unidad?: string;
  precioUnitario: number;
  itemInventarioId?: string; // Opcional: solo si descuenta inventario
  tipo: TipoDetalleTicket;
}

enum TipoDetalleTicket {
  Procedimiento = 1,
  Insumo = 2,
  Medicamento = 3,
  Consulta = 4,
  Otro = 5
}

// Hook
const useCreateTicket = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const createTicket = async (data: CreateTicketDto) => {
    setLoading(true);
    setError(null);

    try {
      const response = await fetch('/api/v1/tickets', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
        body: JSON.stringify(data)
      });

      const result = await response.json();

      if (!result.success) {
        throw new Error(result.message);
      }

      return result.data;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error desconocido';
      setError(errorMessage);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  return { createTicket, loading, error };
};
```

### Formulario de Creación de Ticket

```typescript
const TicketForm = ({ citaId }: { citaId: string }) => {
  const { createTicket, loading, error } = useCreateTicket();
  const [detalles, setDetalles] = useState<TicketDetalleDto[]>([]);
  const [inventarioItems, setInventarioItems] = useState<ItemInventario[]>([]);

  const agregarDetalle = (detalle: TicketDetalleDto) => {
    setDetalles([...detalles, detalle]);
  };

  const handleSubmit = async (formData: CreateTicketDto) => {
    try {
      const ticket = await createTicket({
        ...formData,
        detalles
      });

      toast.success(`Ticket ${ticket.numeroTicket} creado exitosamente`);
      // Navegar a vista de ticket o cerrar modal
    } catch (err) {
      // El error ya está en el estado 'error'
      toast.error(error || 'Error al crear ticket');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Campos del ticket */}
      
      <h3>Detalles e Insumos Utilizados</h3>
      
      {/* Selector de insumos */}
      <div>
        <label>Seleccionar insumo del inventario</label>
        <select onChange={(e) => {
          const item = inventarioItems.find(i => i.id === e.target.value);
          if (item) {
            agregarDetalle({
              descripcion: item.nombre,
              cantidad: 1,
              unidad: item.unidad,
              precioUnitario: 0,
              itemInventarioId: item.id,
              tipo: TipoDetalleTicket.Insumo
            });
          }
        }}>
          <option value="">-- Seleccionar --</option>
          {inventarioItems.map(item => (
            <option key={item.id} value={item.id}>
              {item.nombre} ({item.stockTotal} {item.unidad} disponibles)
            </option>
          ))}
        </select>
      </div>

      {/* Lista de detalles agregados */}
      <table>
        <thead>
          <tr>
            <th>Descripción</th>
            <th>Cantidad</th>
            <th>Unidad</th>
            <th>P. Unitario</th>
            <th>Subtotal</th>
            <th>Tipo</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {detalles.map((det, idx) => (
            <tr key={idx}>
              <td>{det.descripcion}</td>
              <td>{det.cantidad}</td>
              <td>{det.unidad}</td>
              <td>${det.precioUnitario}</td>
              <td>${det.cantidad * det.precioUnitario}</td>
              <td>{TipoDetalleTicket[det.tipo]}</td>
              <td>
                <button onClick={() => eliminarDetalle(idx)}>Eliminar</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {error && (
        <div className="alert alert-danger">
          {error}
        </div>
      )}

      <button type="submit" disabled={loading}>
        {loading ? 'Creando...' : 'Crear Ticket'}
      </button>
    </form>
  );
};
```

---

## Validaciones Recomendadas en Frontend

### Antes de enviar el formulario

```typescript
const validateTicket = (data: CreateTicketDto): string[] => {
  const errors: string[] = [];

  // Validar que hay al menos un detalle
  if (data.detalles.length === 0) {
    errors.push('Debe agregar al menos un detalle al ticket');
  }

  // Validar cantidades positivas
  data.detalles.forEach((det, idx) => {
    if (det.cantidad <= 0) {
      errors.push(`La cantidad del detalle ${idx + 1} debe ser mayor a 0`);
    }
  });

  // Validar que los costos coinciden con los detalles
  const totalDetalles = data.detalles.reduce(
    (sum, det) => sum + (det.cantidad * det.precioUnitario), 
    0
  );

  const totalCostos = data.costoProcedimiento + data.costoInsumos + data.costoAdicional;

  if (Math.abs(totalDetalles - totalCostos) > 0.01) {
    errors.push('La suma de los detalles no coincide con los costos totales');
  }

  return errors;
};
```

---

## Consulta de Stock Disponible

Antes de agregar un insumo al ticket, es recomendable consultar el stock disponible:

```typescript
const checkStockDisponible = async (itemId: string): Promise<StockInfo> => {
  const response = await fetch(`/api/v1/inventario/${itemId}`);
  const data = await response.json();
  
  return {
    itemId: data.itemId,
    nombre: data.nombre,
    stockTotal: data.stockTotal,
    unidad: data.unidad,
    loteMasProximo: data.loteMasProximo
  };
};

// En el formulario
const handleAgregarInsumo = async (itemId: string) => {
  const stock = await checkStockDisponible(itemId);
  
  if (stock.stockTotal <= 0) {
    toast.error(`No hay stock disponible de ${stock.nombre}`);
    return;
  }

  if (stock.loteMasProximo?.expDate) {
    const diasParaVencer = daysDiff(new Date(), stock.loteMasProximo.expDate);
    if (diasParaVencer < 0) {
      toast.error(`El lote de ${stock.nombre} está vencido`);
      return;
    } else if (diasParaVencer < 30) {
      toast.warning(
        `El lote de ${stock.nombre} vence en ${diasParaVencer} días`
      );
    }
  }

  // Agregar al formulario
  agregarDetalle({
    descripcion: stock.nombre,
    cantidad: 1,
    unidad: stock.unidad,
    precioUnitario: 0,
    itemInventarioId: itemId,
    tipo: TipoDetalleTicket.Insumo
  });
};
```
