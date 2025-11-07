# API de Especialidades para Empleados - Documentación

## Resumen

Se ha implementado una solución completa para asignar y gestionar especialidades de empleados (veterinarios) en el sistema AdoPets.

## Enfoque Implementado: **Híbrido (Recomendado)**

### ¿Por qué este enfoque?

1. **Endpoint separado para asignar especialidades** - Más flexible y RESTful
2. **Especialidades incluidas en el DTO de consulta** - Fácil acceso a la información
3. **Operaciones CRUD independientes** - Mejor separación de responsabilidades

## Archivos Creados

### DTOs
- `AdoPetsBKD/Application/DTOs/Empleados/AsignarEspecialidadesDto.cs` - DTO para asignar especialidades
- `AdoPetsBKD/Application/DTOs/Empleados/EspecialidadEmpleadoDto.cs` - DTO para mostrar especialidades del empleado

### Archivos Modificados

1. **EmpleadoDetailDto.cs** - Agregada propiedad `Especialidades`
2. **IEmpleadoService.cs** - Agregados métodos:
   - `AsignarEspecialidadesAsync`
   - `RemoverEspecialidadAsync`

3. **IEmpleadoRepository.cs** - Agregado método:
   - `GetByIdWithEspecialidadesAsync`

4. **EmpleadoRepository.cs** - Implementado:
   - `GetByIdWithEspecialidadesAsync` con Include de especialidades
   - Modificado `GetAllAsync` para incluir especialidades

5. **EmpleadoService.cs** - Implementados:
   - `AsignarEspecialidadesAsync` - Asigna múltiples especialidades (reemplaza las anteriores)
   - `RemoverEspecialidadAsync` - Remueve una especialidad específica
   - Modificado `GetByIdAsync` para incluir especialidades

6. **EmpleadosController.cs** - Agregados endpoints:
   - `POST /api/empleados/{id}/especialidades`
   - `DELETE /api/empleados/{id}/especialidades/{especialidadId}`

## Endpoints Disponibles

### 1. Asignar Especialidades a un Empleado
```http
POST /api/empleados/{id}/especialidades
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "especialidades": [
    {
      "especialidadId": "guid-here",
      "certificacion": "Diplomado en Cirugía Veterinaria"
    },
    {
      "especialidadId": "guid-here",
      "certificacion": "Certificación en Dermatología"
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "message": "Especialidades asignadas exitosamente",
  "data": {
    "id": "guid",
    "nombre": "Juan",
    "apellidoPaterno": "Pérez",
    "apellidoMaterno": "García",
    "nombreCompleto": "Juan Pérez García",
    "emailLaboral": "juan.perez@adopets.com",
    "telefonoLaboral": "5551234567",
    "cedula": "12345678",
    "especialidades": [
      {
        "especialidadId": "guid",
        "descripcion": "Cirugía Veterinaria",
        "codigo": "CIR",
        "certificacion": "Diplomado en Cirugía Veterinaria",
        "obtainedAt": "2024-01-15T10:30:00Z"
      },
      {
        "especialidadId": "guid",
        "descripcion": "Dermatología Veterinaria",
        "codigo": "DER",
        "certificacion": "Certificación en Dermatología",
        "obtainedAt": "2024-01-15T10:30:00Z"
      }
    ],
    ...
  }
}
```

### 2. Consultar Empleado (incluye especialidades)
```http
GET /api/empleados/{id}
Authorization: Bearer {token}
```

**Response:** Mismo formato que arriba, incluye el array de especialidades.

### 3. Remover una Especialidad
```http
DELETE /api/empleados/{id}/especialidades/{especialidadId}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "message": "Especialidad removida exitosamente",
  "data": {
    // ... empleado actualizado sin esa especialidad
  }
}
```

### 4. Listar Todos los Empleados (incluye especialidades)
```http
GET /api/empleados?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

Los empleados listados incluyen un array de nombres de especialidades.

## Características Implementadas

### ? Funcionalidades
- Asignar múltiples especialidades a un empleado de una vez
- Remover una especialidad específica
- Consultar especialidades de un empleado
- Incluir certificaciones personalizadas por especialidad
- Registro de fecha de obtención (`obtainedAt`)

### ? Validaciones
- Validar que el empleado exista
- Validar que todas las especialidades a asignar existan
- Validar que la especialidad a remover esté asignada al empleado
- Validación de permisos (solo Admins)

### ? Seguridad
- Requiere autenticación JWT
- Política de autorización `AdminOnly`
- Auditoría de cambios (`UpdatedBy`, `UpdatedAt`)

### ? Logging
- Log de asignación de especialidades
- Log de remoción de especialidades
- Log de errores de validación

## Modelo de Datos

### EmpleadoEspecialidad (Tabla Intermedia)
```csharp
public class EmpleadoEspecialidad
{
    public Guid EmpleadoId { get; set; }
    public Guid EspecialidadId { get; set; }
    public DateTime ObtainedAt { get; set; }      // Fecha de obtención
    public string? Certificacion { get; set; }     // Información de certificación
    
    // Navigation properties
    public Empleado Empleado { get; set; }
    public Especialidad Especialidad { get; set; }
}
```

## Flujo de Uso Recomendado

### Escenario 1: Asignar especialidades al crear un empleado

1. **Crear el empleado** usando `POST /api/empleados`
2. **Asignar especialidades** usando `POST /api/empleados/{id}/especialidades`
3. **Consultar el empleado** para verificar: `GET /api/empleados/{id}`

### Escenario 2: Actualizar especialidades de un empleado existente

1. **Asignar nuevas especialidades** (reemplaza las anteriores):
   ```http
   POST /api/empleados/{id}/especialidades
   ```
   
2. **O remover una específica**:
   ```http
   DELETE /api/empleados/{id}/especialidades/{especialidadId}
   ```

## Ventajas de este Enfoque

### ? Separación de Responsabilidades
- CRUD de empleados independiente de especialidades
- Endpoints específicos para cada operación
- Fácil de mantener y extender

### ? Flexibilidad
- Puedes asignar especialidades después de crear el empleado
- Puedes actualizar especialidades sin modificar otros datos
- Puedes remover especialidades individuales

### ? RESTful
- Sigue las mejores prácticas de diseño de APIs
- Recursos anidados (`/empleados/{id}/especialidades`)
- Verbos HTTP apropiados (POST, DELETE)

### ? Información Rica
- Incluye certificaciones personalizadas
- Fecha de obtención de cada especialidad
- Código y descripción completa

## Alternativas Consideradas

### Opción A: Incluir especialidades en Create/Update Empleado
**Ventaja:** Un solo request para todo
**Desventaja:** Endpoint muy grande, difícil de mantener

### Opción B: Endpoint separado (IMPLEMENTADA)
**Ventaja:** Flexible, RESTful, fácil de mantener
**Desventaja:** Requiere múltiples requests

### Opción C: Híbrida (IMPLEMENTADA)
**Ventaja:** Lo mejor de ambos mundos
**Desventaja:** Ninguna significativa

## Notas Importantes

1. **Asignación reemplaza las anteriores**: Cuando asignas especialidades con POST, se eliminan las anteriores y se crean las nuevas.

2. **Soft delete**: Si eliminas una especialidad del catálogo, los empleados mantienen su relación histórica.

3. **Validación de duplicados**: Entity Framework maneja automáticamente la clave compuesta (EmpleadoId + EspecialidadId).

4. **Performance**: Los queries incluyen `.Include()` para evitar N+1 queries.

## Testing

Usa el archivo `AdoPetsBKD/Tests/EmpleadoEspecialidades.http` para probar los endpoints.

## Próximos Pasos Sugeridos

1. ? Implementado: API completa
2. ? Pendiente: Agregar filtro para buscar empleados por especialidad
3. ? Pendiente: Reportes de empleados por especialidad
4. ? Pendiente: Validación de vigencia de certificaciones

---

**Desarrollado para**: AdoPets Backend  
**Módulo**: Servicios/Empleados  
**Responsable**: Developer 5 (Cielo) según AsignacionModulos-EquipoDev.md  
**Fecha**: 2024
