# API de Historial Clínico Complementario - Documentación

## Descripción General
Este documento cubre los módulos complementarios del Historial Clínico:
- **Vacunaciones** - Registro de vacunas con plan de refuerzos
- **Desparasitaciones** - Control de desparasitación periódica
- **Cirugías** - Registro de procedimientos quirúrgicos
- **Valoraciones** - Registro de signos vitales

Todos estos módulos se integran con los Expedientes Médicos para crear un historial clínico completo.

---

# 1. API de Vacunaciones

## Descripción
Gestiona el registro de vacunaciones aplicadas a las mascotas, incluyendo el control de refuerzos y alertas de próximas vacunas.

---

## Endpoints

### 1.1 Obtener Vacunación por ID
**Endpoint:** `GET /api/vacunaciones/{id}`

**Autorización:** Requerida (Token JWT)

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "mascotaId": "guid",
    "mascotaNombre": "Max",
    "nombreVacuna": "Vacuna Séxtuple Canina",
    "laboratorio": "Zoetis",
    "lote": "ABC123456",
    "fechaAplicacion": "2024-01-15T10:00:00",
    "proximoRefuerzo": "2025-01-15T10:00:00",
    "veterinarioId": "guid",
    "veterinarioNombre": "Dr. González",
    "notas": "Primera dosis anual. Sin reacciones adversas.",
    "creadoPor": "Dr. González",
    "fechaCreacion": "2024-01-15T10:30:00"
  }
}
```

---

### 1.2 Obtener Vacunaciones por Mascota
**Endpoint:** `GET /api/vacunaciones/mascota/{mascotaId}`

**Autorización:** Requerida (Token JWT)

**Descripción:** Retorna el historial completo de vacunación de una mascota.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "nombreVacuna": "Vacuna Séxtuple Canina",
      "fechaAplicacion": "2024-01-15T10:00:00",
      "proximoRefuerzo": "2025-01-15T10:00:00",
      "veterinario": "Dr. González",
      "diasParaRefuerzo": 365
    },
    {
      "id": "guid",
      "nombreVacuna": "Vacuna Antirrábica",
      "fechaAplicacion": "2023-07-20T11:00:00",
      "proximoRefuerzo": "2024-07-20T11:00:00",
      "veterinario": "Dr. Martínez",
      "diasParaRefuerzo": 186
    }
  ]
}
```

---

### 1.3 Obtener Vacunaciones Próximas a Vencer
**Endpoint:** `GET /api/vacunaciones/proximas?days=30`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Parámetros de Query:**
- `days` (int, opcional) - Días de anticipación (default: 30)

**Descripción:** Retorna vacunas que vencen en los próximos N días. Útil para recordatorios automáticos.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "mascotaNombre": "Max",
      "propietarioNombre": "Juan Pérez",
      "propietarioEmail": "juan@email.com",
      "propietarioTelefono": "+52-555-123-4567",
      "nombreVacuna": "Vacuna Antirrábica",
      "proximoRefuerzo": "2024-02-10T10:00:00",
      "diasRestantes": 26
    }
  ]
}
```

---

### 1.4 Registrar Nueva Vacunación
**Endpoint:** `POST /api/vacunaciones`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Cuerpo de la Solicitud:**
```json
{
  "mascotaId": "guid",
  "nombreVacuna": "Vacuna Séxtuple Canina",
  "laboratorio": "Zoetis",
  "lote": "ABC123456",
  "fechaAplicacion": "2024-01-15T10:00:00",
  "proximoRefuerzo": "2025-01-15T10:00:00",
  "veterinarioId": "guid",
  "notas": "Primera dosis anual. Mascota toleró bien la vacuna."
}
```

**Validaciones:**
- `mascotaId` - Requerido
- `nombreVacuna` - Requerido, máximo 200 caracteres
- `fechaAplicacion` - Requerido
- `proximoRefuerzo` - Opcional (se calcula automáticamente si no se proporciona)
- `veterinarioId` - Requerido

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Vacunación registrada exitosamente",
  "data": {
    "id": "guid",
    "mascotaNombre": "Max",
    "nombreVacuna": "Vacuna Séxtuple Canina",
    "fechaAplicacion": "2024-01-15T10:00:00",
    "proximoRefuerzo": "2025-01-15T10:00:00"
  }
}
```

---

### 1.5 Eliminar Vacunación
**Endpoint:** `DELETE /api/vacunaciones/{id}`

**Autorización:** Requerida (Rol: Admin únicamente)

**Descripción:** Elimina un registro de vacunación. Usar con precaución.

---

## Plan de Vacunación Típico

### Perros (Cachorros)
| Vacuna | Primera Dosis | Refuerzos | Frecuencia Anual |
|--------|---------------|-----------|------------------|
| Séxtuple | 6-8 semanas | 10, 14 semanas | Anual |
| Antirrábica | 12 semanas | - | Anual |
| Bordetella | 8 semanas | 12 semanas | Anual |
| Leptospirosis | 12 semanas | 16 semanas | Anual |

### Gatos (Cachorros)
| Vacuna | Primera Dosis | Refuerzos | Frecuencia Anual |
|--------|---------------|-----------|------------------|
| Triple Felina | 6-8 semanas | 12, 16 semanas | Anual |
| Leucemia Felina | 8 semanas | 12 semanas | Anual |
| Antirrábica | 12 semanas | - | Anual |

---

## Lógica de Negocio

### Cálculo Automático de Refuerzos
```csharp
// Si no se especifica próximo refuerzo, se calcula automáticamente
if (dto.ProximoRefuerzo == null)
{
    // Vacunas anuales
    if (nombreVacuna.Contains("Antirrábica") || nombreVacuna.Contains("Séxtuple"))
    {
        dto.ProximoRefuerzo = dto.FechaAplicacion.AddYears(1);
    }
    // Vacunas de cachorros (cada 4 semanas)
    else if (nombreVacuna.Contains("Cachorro"))
    {
        dto.ProximoRefuerzo = dto.FechaAplicacion.AddDays(28);
    }
}
```

### Alertas de Refuerzo
- **30 días antes:** Email al propietario
- **15 días antes:** Email + SMS
- **Día del vencimiento:** Notificación urgente
- **Después de vencer:** Marcar vacuna como vencida

---

# 2. API de Desparasitaciones

## Descripción
Gestiona el control de desparasitaciones periódicas, tanto internas como externas.

---

## Endpoints

### 2.1 Obtener Desparasitación por ID
**Endpoint:** `GET /api/desparasitaciones/{id}`

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "mascotaId": "guid",
    "mascotaNombre": "Max",
    "tipo": "Interna",
    "producto": "Fenbendazol",
    "dosis": "50mg/kg",
    "fechaAplicacion": "2024-01-15T10:00:00",
    "proximaAplicacion": "2024-04-15T10:00:00",
    "veterinarioId": "guid",
    "veterinarioNombre": "Dr. González",
    "notas": "Desparasitación preventiva trimestral",
    "efectosAdversos": false
  }
}
```

---

### 2.2 Obtener Desparasitaciones por Mascota
**Endpoint:** `GET /api/desparasitaciones/mascota/{mascotaId}`

**Descripción:** Retorna el historial de desparasitaciones de una mascota.

---

### 2.3 Obtener Desparasitaciones Próximas
**Endpoint:** `GET /api/desparasitaciones/proximas?days=30`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Descripción:** Mascotas que requieren desparasitación en los próximos N días.

---

### 2.4 Registrar Nueva Desparasitación
**Endpoint:** `POST /api/desparasitaciones`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Cuerpo de la Solicitud:**
```json
{
  "mascotaId": "guid",
  "tipo": "Interna",
  "producto": "Fenbendazol",
  "dosis": "50mg/kg",
  "fechaAplicacion": "2024-01-15T10:00:00",
  "proximaAplicacion": "2024-04-15T10:00:00",
  "veterinarioId": "guid",
  "notas": "Desparasitación preventiva. Sin efectos adversos.",
  "efectosAdversos": false
}
```

**Tipos de Desparasitación:**
- **Interna** - Parásitos intestinales (lombrices, giardia, etc.)
- **Externa** - Pulgas, garrapatas, ácaros

**Productos Comunes:**
- Internos: Fenbendazol, Ivermectina, Praziquantel
- Externos: Fipronil, Selamectina, Fluralaner

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Desparasitación registrada exitosamente",
  "data": {
    "id": "guid",
    "mascotaNombre": "Max",
    "tipo": "Interna",
    "producto": "Fenbendazol",
    "fechaAplicacion": "2024-01-15T10:00:00",
    "proximaAplicacion": "2024-04-15T10:00:00"
  }
}
```

---

### 2.5 Eliminar Desparasitación
**Endpoint:** `DELETE /api/desparasitaciones/{id}`

**Autorización:** Requerida (Rol: Admin)

---

## Plan de Desparasitación Típico

### Cachorros (hasta 6 meses)
- **Frecuencia:** Cada 2 semanas hasta 12 semanas, luego mensual
- **Producto:** Fenbendazol o Pamoato de pirantel

### Adultos
- **Interna:** Cada 3 meses (trimestral)
- **Externa:** Cada 1-3 meses según producto

---

# 3. API de Cirugías

## Descripción
Registra procedimientos quirúrgicos realizados con todos sus detalles.

---

## Endpoints

### 3.1 Obtener Cirugía por ID
**Endpoint:** `GET /api/cirugias/{id}`

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "mascotaId": "guid",
    "mascotaNombre": "Max",
    "tipoCirugia": "Esterilización (Castración)",
    "descripcion": "Orquiectomía bilateral",
    "fechaCirugia": "2024-01-15T09:00:00",
    "duracionMinutos": 45,
    "veterinarioId": "guid",
    "veterinarioCirujano": "Dr. González",
    "anestesiologo": "Dr. Martínez",
    "tipoAnestesia": "General inhalada (Isoflurano)",
    "complicaciones": false,
    "detallesComplicaciones": null,
    "estadoPostOperatorio": "Estable",
    "instruccionesPostOperatorias": "1. Reposo absoluto x 7 días\n2. Antibiótico: Cefalexina 500mg c/12h x 7 días\n3. Analgésico: Carprofeno 75mg c/24h x 5 días\n4. Collar isabelino permanente\n5. Revisión de puntos en 7 días\n6. Retiro de puntos en 10-12 días",
    "fechaRevisionPuntos": "2024-01-22T10:00:00",
    "notas": "Cirugía sin complicaciones. Mascota despertó bien de anestesia.",
    "creadoPor": "Dr. González",
    "fechaCreacion": "2024-01-15T11:00:00"
  }
}
```

---

### 3.2 Obtener Cirugías por Mascota
**Endpoint:** `GET /api/cirugias/mascota/{mascotaId}`

**Descripción:** Historial quirúrgico completo de una mascota.

---

### 3.3 Obtener Cirugías por Veterinario
**Endpoint:** `GET /api/cirugias/veterinario/{veterinarioId}`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Descripción:** Todas las cirugías realizadas por un veterinario específico.

---

### 3.4 Registrar Nueva Cirugía
**Endpoint:** `POST /api/cirugias`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Cuerpo de la Solicitud:**
```json
{
  "mascotaId": "guid",
  "tipoCirugia": "Esterilización (Ovariohisterectomía)",
  "descripcion": "Remoción de ovarios y útero vía ventral",
  "fechaCirugia": "2024-01-15T09:00:00",
  "duracionMinutos": 60,
  "veterinarioId": "guid",
  "anestesiologoId": "guid",
  "tipoAnestesia": "General inhalada (Isoflurano)",
  "complicaciones": false,
  "detallesComplicaciones": null,
  "estadoPostOperatorio": "Estable",
  "instruccionesPostOperatorias": "Reposo x 10 días, antibiótico, analgésico, collar isabelino",
  "fechaRevisionPuntos": "2024-01-22T10:00:00",
  "notas": "Procedimiento estándar sin incidentes"
}
```

**Tipos de Cirugía Comunes:**
- Esterilización (Castración/Ovariohisterectomía)
- Extracción dental
- Remoción de masas/tumores
- Reparación de fracturas
- Cesárea
- Gastropexia
- Limpieza dental con anestesia

**Validaciones:**
- `mascotaId` - Requerido
- `tipoCirugia` - Requerido
- `fechaCirugia` - Requerido
- `duracionMinutos` - Requerido, mínimo 5
- `veterinarioId` - Requerido

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Cirugía registrada exitosamente",
  "data": {
    "id": "guid",
    "mascotaNombre": "Max",
    "tipoCirugia": "Esterilización",
    "fechaCirugia": "2024-01-15T09:00:00",
    "estadoPostOperatorio": "Estable"
  }
}
```

---

### 3.5 Eliminar Cirugía
**Endpoint:** `DELETE /api/cirugias/{id}`

**Autorización:** Requerida (Rol: Admin)

---

## Protocolo Quirúrgico Estándar

### Pre-Operatorio
1. Ayuno de 8-12 horas
2. Análisis pre-quirúrgicos (sangre, rayos X según caso)
3. Valoración cardiaca y pulmonar
4. Consentimiento informado firmado

### Trans-Operatorio
1. Monitoreo de signos vitales
2. Oxigenación adecuada
3. Control de temperatura
4. Registro de complicaciones

### Post-Operatorio
1. Monitoreo de recuperación anestésica
2. Control de dolor
3. Prevención de infecciones
4. Instrucciones detalladas al propietario

---

# 4. API de Valoraciones

## Descripción
Registra signos vitales y mediciones físicas durante las consultas.

---

## Endpoints

### 4.1 Obtener Valoración por ID
**Endpoint:** `GET /api/valoraciones/{id}`

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "mascotaId": "guid",
    "mascotaNombre": "Max",
    "fecha": "2024-01-15T10:00:00",
    "peso": 28.5,
    "temperatura": 38.2,
    "frecuenciaCardiaca": 95,
    "frecuenciaRespiratoria": 22,
    "presionArterial": "120/80",
    "condicionCorporal": 5,
    "hidratacion": "Adecuada",
    "mucosas": "Rosadas y húmedas",
    "tiempoLlenadoCapilar": "< 2 segundos",
    "estadoMental": "Alerta y responsivo",
    "notas": "Signos vitales dentro de parámetros normales",
    "veterinarioId": "guid",
    "veterinarioNombre": "Dr. González"
  }
}
```

---

### 4.2 Obtener Valoraciones por Mascota
**Endpoint:** `GET /api/valoraciones/mascota/{mascotaId}`

**Descripción:** Historial de valoraciones de una mascota. Útil para tracking de peso, temperatura, etc.

---

### 4.3 Obtener Última Valoración de Mascota
**Endpoint:** `GET /api/valoraciones/mascota/{mascotaId}/ultima`

**Descripción:** Retorna la valoración más reciente, útil para comparaciones rápidas.

---

### 4.4 Registrar Nueva Valoración
**Endpoint:** `POST /api/valoraciones`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Cuerpo de la Solicitud:**
```json
{
  "mascotaId": "guid",
  "fecha": "2024-01-15T10:00:00",
  "peso": 28.5,
  "temperatura": 38.2,
  "frecuenciaCardiaca": 95,
  "frecuenciaRespiratoria": 22,
  "presionArterial": "120/80",
  "condicionCorporal": 5,
  "hidratacion": "Adecuada",
  "mucosas": "Rosadas y húmedas",
  "tiempoLlenadoCapilar": "< 2 segundos",
  "estadoMental": "Alerta y responsivo",
  "notas": "Examen físico normal",
  "veterinarioId": "guid"
}
```

**Parámetros de Referencia:**

| Parámetro | Perros | Gatos |
|-----------|--------|-------|
| Temperatura | 38.0 - 39.2°C | 38.1 - 39.2°C |
| FC (Frecuencia Cardíaca) | 60 - 140 lpm | 140 - 220 lpm |
| FR (Frecuencia Respiratoria) | 10 - 30 rpm | 20 - 30 rpm |
| Condición Corporal | 1-9 (ideal: 4-5) | 1-9 (ideal: 5) |
| TLC (Tiempo Llenado Capilar) | < 2 segundos | < 2 segundos |

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Valoración registrada exitosamente",
  "data": {
    "id": "guid",
    "mascotaNombre": "Max",
    "fecha": "2024-01-15T10:00:00",
    "peso": 28.5,
    "temperatura": 38.2,
    "frecuenciaCardiaca": 95
  }
}
```

---

### 4.5 Eliminar Valoración
**Endpoint:** `DELETE /api/valoraciones/{id}`

**Autorización:** Requerida (Rol: Admin)

---

## Escala de Condición Corporal (1-9)

| Puntuación | Descripción | Recomendación |
|------------|-------------|---------------|
| 1-2 | Extremadamente delgado | Nutrición urgente |
| 3 | Delgado | Aumentar dieta |
| 4-5 | **Ideal** | Mantener |
| 6-7 | Sobrepeso | Reducir dieta |
| 8-9 | Obesidad | Dieta + ejercicio |

---

## Integración entre Módulos

### Flujo Típico de Consulta Completa
```
1. Recepcionista crea Cita
2. Veterinario realiza Valoración (signos vitales)
3. Veterinario crea Expediente (metodología SOAP)
4. Se registra Vacunación si aplica
5. Se registra Desparasitación si aplica
6. Si requiere cirugía, se agenda y registra posteriormente
7. Se genera Ticket de servicios
8. Cliente realiza Pago
```

### Vinculaciones Automáticas
- **Expediente ? Cita** - Se vinculan automáticamente
- **Valoración ? Expediente** - Datos de valoración se copian al campo "Objetivo"
- **Vacunación ? Expediente** - Se menciona en el campo "Plan"
- **Cirugía ? Expediente** - Requiere expediente pre-quirúrgico y post-quirúrgico

---

## Ejemplos Completos

### Ejemplo: Consulta de Rutina
```bash
# 1. Registrar valoración
POST /api/valoraciones
{
  "mascotaId": "guid",
  "peso": 28.5,
  "temperatura": 38.2,
  "frecuenciaCardiaca": 95,
  "frecuenciaRespiratoria": 22
}

# 2. Registrar vacunación
POST /api/vacunaciones
{
  "mascotaId": "guid",
  "nombreVacuna": "Vacuna Séxtuple",
  "fechaAplicacion": "2024-01-15T10:00:00"
}

# 3. Crear expediente
POST /api/expedientes
{
  "mascotaId": "guid",
  "subjetivo": "Propietario trae mascota para vacunación anual",
  "objetivo": "Peso: 28.5kg, T: 38.2°C, FC: 95lpm, FR: 22rpm",
  "analisis": "Mascota sana, apta para vacunación",
  "plan": "Vacuna séxtuple aplicada"
}
```

---

## Contacto y Soporte
**Desarrollador Responsable:** Developer 3 - Beto  
**Módulo:** Clínica & Historial Médico  
**Versión API:** 1.0  
**Última Actualización:** Enero 2024
