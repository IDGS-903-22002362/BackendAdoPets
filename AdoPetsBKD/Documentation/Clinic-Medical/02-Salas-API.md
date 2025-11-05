# API de Salas - Documentación

## Descripción General
El módulo de **Salas** gestiona los espacios físicos de la clínica veterinaria donde se realizan las consultas, cirugías y procedimientos médicos.

---

## Endpoints

### 1. Obtener Todas las Salas
**Endpoint:** `GET /api/salas`

**Autorización:** Requerida (Token JWT)

**Descripción:** Retorna todas las salas registradas en el sistema (activas e inactivas).

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Salas obtenidas exitosamente",
  "data": [
    {
      "id": "guid",
      "nombre": "Sala de Consulta 1",
      "tipo": "Consulta",
      "capacidad": 1,
      "equipamiento": "Básico",
      "estado": "Disponible",
      "activo": true
    },
    {
      "id": "guid",
      "nombre": "Quirófano Principal",
      "tipo": "Cirugia",
      "capacidad": 3,
      "equipamiento": "Completo",
      "estado": "Ocupada",
      "activo": true
    }
  ]
}
```

**Códigos de Estado:**
- `200 OK` - Éxito
- `401 Unauthorized` - No autenticado
- `500 Internal Server Error` - Error del servidor

---

### 2. Obtener Salas Activas
**Endpoint:** `GET /api/salas/activas`

**Autorización:** Requerida (Token JWT)

**Descripción:** Retorna únicamente las salas activas y disponibles para uso.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Salas activas obtenidas exitosamente",
  "data": [
    {
      "id": "guid",
      "nombre": "Sala de Consulta 1",
      "tipo": "Consulta",
      "capacidad": 1,
      "estado": "Disponible",
      "activo": true
    }
  ]
}
```

**Uso Típico:** Este endpoint es ideal para mostrar opciones de salas al agendar una cita.

---

### 3. Obtener Sala por ID
**Endpoint:** `GET /api/salas/{id}`

**Autorización:** Requerida (Token JWT)

**Parámetros de Ruta:**
- `id` (Guid) - ID de la sala

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "nombre": "Sala de Consulta 1",
    "tipo": "Consulta",
    "descripcion": "Sala equipada para consultas generales",
    "capacidad": 1,
    "equipamiento": "Básico: camilla, balanza, otoscopio",
    "estado": "Disponible",
    "activo": true,
    "ubicacion": "Primer piso, ala este",
    "creadoPor": "Admin",
    "fechaCreacion": "2024-01-01T10:00:00",
    "modificadoPor": "Admin",
    "fechaModificacion": "2024-01-10T15:30:00"
  }
}
```

**Códigos de Estado:**
- `200 OK` - Sala encontrada
- `404 Not Found` - Sala no encontrada
- `401 Unauthorized` - No autenticado
- `500 Internal Server Error` - Error del servidor

---

### 4. Crear Nueva Sala
**Endpoint:** `POST /api/salas`

**Autorización:** Requerida (Rol: Admin únicamente)

**Cuerpo de la Solicitud:**
```json
{
  "nombre": "Sala de Cirugía 2",
  "tipo": "Cirugia",
  "descripcion": "Quirófano secundario para cirugías menores",
  "capacidad": 3,
  "equipamiento": "Instrumental quirúrgico completo, mesa de cirugía, anestesia",
  "ubicacion": "Segundo piso, ala oeste"
}
```

**Validaciones:**
- `nombre` - Requerido, máximo 100 caracteres, único en el sistema
- `tipo` - Requerido (Consulta, Cirugia, Emergencia, Hospitalizacion, etc.)
- `capacidad` - Requerido, mínimo 1
- `descripcion` - Opcional, máximo 500 caracteres
- `equipamiento` - Opcional, máximo 1000 caracteres
- `ubicacion` - Opcional, máximo 200 caracteres

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Sala creada exitosamente",
  "data": {
    "id": "guid",
    "nombre": "Sala de Cirugía 2",
    "tipo": "Cirugia",
    "capacidad": 3,
    "estado": "Disponible",
    "activo": true
  }
}
```

**Códigos de Estado:**
- `201 Created` - Sala creada exitosamente
- `400 Bad Request` - Datos inválidos
- `401 Unauthorized` - No autenticado
- `403 Forbidden` - Sin permisos de administrador
- `409 Conflict` - Nombre de sala duplicado
- `500 Internal Server Error` - Error del servidor

**Error de Duplicación:**
```json
{
  "success": false,
  "message": "Ya existe una sala con ese nombre",
  "errors": []
}
```

---

### 5. Actualizar Sala
**Endpoint:** `PUT /api/salas/{id}`

**Autorización:** Requerida (Rol: Admin únicamente)

**Parámetros de Ruta:**
- `id` (Guid) - ID de la sala

**Cuerpo de la Solicitud:**
```json
{
  "nombre": "Quirófano Principal - Actualizado",
  "tipo": "Cirugia",
  "descripcion": "Quirófano principal con equipamiento de última generación",
  "capacidad": 4,
  "equipamiento": "Mesa quirúrgica, anestesia digital, monitor de signos vitales",
  "ubicacion": "Segundo piso, centro"
}
```

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Sala actualizada exitosamente",
  "data": {
    "id": "guid",
    "nombre": "Quirófano Principal - Actualizado",
    "tipo": "Cirugia",
    "capacidad": 4,
    "activo": true
  }
}
```

**Códigos de Estado:**
- `200 OK` - Actualización exitosa
- `400 Bad Request` - Datos inválidos
- `401 Unauthorized` - No autenticado
- `403 Forbidden` - Sin permisos
- `404 Not Found` - Sala no encontrada
- `409 Conflict` - Conflicto con nombre duplicado
- `500 Internal Server Error` - Error del servidor

---

### 6. Eliminar Sala (Soft Delete)
**Endpoint:** `DELETE /api/salas/{id}`

**Autorización:** Requerida (Rol: Admin únicamente)

**Parámetros de Ruta:**
- `id` (Guid) - ID de la sala

**Descripción:** Realiza una eliminación lógica (soft delete) de la sala. La sala se marca como inactiva pero permanece en el sistema para preservar el historial de citas asociadas.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Sala eliminada exitosamente",
  "data": null
}
```

**Códigos de Estado:**
- `200 OK` - Eliminación exitosa
- `401 Unauthorized` - No autenticado
- `403 Forbidden` - Sin permisos de administrador
- `404 Not Found` - Sala no encontrada
- `500 Internal Server Error` - Error del servidor

**Nota Importante:** Las salas eliminadas no se muestran en listados de salas activas, pero las citas históricas mantienen la referencia a la sala eliminada.

---

## Tipos de Sala

El sistema soporta los siguientes tipos de salas:

| Tipo | Descripción | Uso Típico |
|------|-------------|------------|
| **Consulta** | Sala básica para consultas | Revisiones generales, vacunaciones |
| **Cirugia** | Quirófano equipado | Cirugías programadas |
| **Emergencia** | Sala de atención urgente | Casos de emergencia |
| **Hospitalizacion** | Área de internamiento | Recuperación post-operatoria |
| **Diagnostico** | Sala de estudios | Rayos X, ecografías |
| **Laboratorio** | Análisis clínicos | Pruebas de sangre, orina |

---

## Estados de Sala

Las salas pueden tener los siguientes estados:

- **Disponible** - La sala está lista para uso
- **Ocupada** - Actualmente en uso por una cita
- **Mantenimiento** - En proceso de limpieza o reparación
- **Reservada** - Apartada para una cita próxima

**Nota:** El estado se actualiza automáticamente según las citas programadas.

---

## Lógica de Negocio

### Gestión de Disponibilidad
```javascript
// Algoritmo de verificación de disponibilidad
function verificarDisponibilidad(salaId, fechaInicio, fechaFin) {
  const citasEnSala = getCitasBySala(salaId);
  
  const conflicto = citasEnSala.some(cita => 
    cita.fechaInicio < fechaFin && 
    cita.fechaFin > fechaInicio &&
    cita.status !== 'Cancelada'
  );
  
  return !conflicto;
}
```

### Validaciones Especiales
1. **Nombre Único:** No pueden existir dos salas con el mismo nombre
2. **Capacidad Mínima:** Al menos 1 persona
3. **Citas Activas:** No se puede eliminar una sala con citas activas o futuras

### Soft Delete vs Hard Delete
- **Soft Delete (DELETE /api/salas/{id}):** Marca `Activo = false`. Preserva historial.
- **Hard Delete:** No disponible via API. Solo por base de datos directamente.

---

## Ejemplos de Uso

### Ejemplo 1: Crear Sala de Consulta
```bash
curl -X POST https://api.adopets.com/api/salas \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Sala de Consulta 3",
    "tipo": "Consulta",
    "descripcion": "Sala para consultas generales",
    "capacidad": 1,
    "equipamiento": "Camilla, balanza digital, otoscopio",
    "ubicacion": "Primer piso, ala oeste"
  }'
```

### Ejemplo 2: Listar Salas Disponibles
```bash
curl -X GET https://api.adopets.com/api/salas/activas \
  -H "Authorization: Bearer {token}"
```

### Ejemplo 3: Actualizar Equipamiento
```bash
curl -X PUT https://api.adopets.com/api/salas/12345678-1234-1234-1234-123456789012 \
  -H "Authorization: Bearer {admin-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Quirófano Principal",
    "tipo": "Cirugia",
    "descripcion": "Quirófano completamente equipado",
    "capacidad": 4,
    "equipamiento": "Mesa quirúrgica eléctrica, anestesia digital, monitor multiparámetros, lámpara cielítica LED",
    "ubicacion": "Segundo piso, centro"
  }'
```

### Ejemplo 4: Desactivar Sala
```bash
curl -X DELETE https://api.adopets.com/api/salas/12345678-1234-1234-1234-123456789012 \
  -H "Authorization: Bearer {admin-token}"
```

---

## Integración con Otros Módulos

### Módulo de Citas
- Al crear una cita, se valida que la sala esté activa y disponible
- Se actualiza automáticamente el estado de la sala según las citas
- Las citas históricas mantienen referencia a salas eliminadas

### Módulo de Reportes
- Estadísticas de uso por sala
- Tiempo promedio de ocupación
- Salas más utilizadas

---

## Casos de Uso Comunes

### Caso 1: Configuración Inicial de Clínica
```
1. Admin crea salas básicas (2-3 consultas, 1 quirófano)
2. Define equipamiento y capacidades
3. Asigna ubicaciones físicas
```

### Caso 2: Expansión de Instalaciones
```
1. Admin crea nuevas salas
2. Configura tipo y equipamiento
3. Las salas quedan disponibles inmediatamente para agendamiento
```

### Caso 3: Mantenimiento de Sala
```
1. Admin desactiva sala temporalmente
2. Sistema previene nuevas citas en esa sala
3. Al completar mantenimiento, se reactiva
```

---

## Notas Técnicas

### Performance
- Listado de salas activas está optimizado con índice en campo `Activo`
- Consultas frecuentes se pueden cachear por 5-10 minutos

### Seguridad
- Solo usuarios con rol **Admin** pueden crear, modificar o eliminar salas
- Todos los cambios son auditados (quién y cuándo)

### Auditoría
Cada sala mantiene:
- `CreadoPor` - Usuario que creó la sala
- `FechaCreacion` - Timestamp de creación
- `ModificadoPor` - Usuario que realizó la última modificación
- `FechaModificacion` - Timestamp de última modificación

---

## Buenas Prácticas

1. **Nomenclatura Consistente:** Usar convención clara (Ej: "Sala de Consulta 1", "Quirófano Principal")
2. **Descripción Detallada:** Incluir información relevante sobre equipamiento y capacidad
3. **Mantenimiento Preventivo:** Programar mantenimientos regulares marcando salas como inactivas
4. **No Eliminar Salas con Historial:** Preferir desactivación a eliminación completa

---

## Contacto y Soporte
**Desarrollador Responsable:** Developer 3 - Beto  
**Módulo:** Clínica & Historial Médico  
**Versión API:** 1.0  
**Última Actualización:** Enero 2024
