# API de Expedientes Médicos - Documentación

## Descripción General
El módulo de **Expedientes Médicos** implementa el sistema de registro clínico siguiendo la **metodología SOAP** (Subjetivo, Objetivo, Análisis, Plan). Permite mantener un historial médico completo y organizado de cada mascota.

---

## Metodología SOAP

### ¿Qué es SOAP?
SOAP es un método estandarizado de documentación médica que estructura la información clínica en 4 secciones:

| Sección | Descripción | Ejemplo |
|---------|-------------|---------|
| **S - Subjetivo** | Síntomas reportados por el propietario | "El dueño reporta que la mascota no come hace 2 días" |
| **O - Objetivo** | Hallazgos del examen físico | "Temperatura: 39.5°C, Frecuencia cardíaca: 120 lpm" |
| **A - Análisis** | Diagnóstico del veterinario | "Gastroenteritis aguda, posible infección bacteriana" |
| **P - Plan** | Tratamiento y seguimiento | "Antibióticos 250mg c/12h x 7 días, revisión en 3 días" |

---

## Endpoints

### 1. Obtener Expediente por ID
**Endpoint:** `GET /api/expedientes/{id}`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Parámetros de Ruta:**
- `id` (Guid) - ID del expediente

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "numeroExpediente": "EXP-20240115-001",
    "fecha": "2024-01-15T10:30:00",
    "mascota": {
      "id": "guid",
      "nombre": "Max",
      "especie": "Perro",
      "raza": "Labrador",
      "edad": "3 años",
      "peso": 28.5
    },
    "veterinario": {
      "id": "guid",
      "nombre": "Dr. González",
      "especialidad": "Medicina General"
    },
    "citaId": "guid",
    "motivoConsulta": "Revisión anual y vacunación",
    "subjetivo": "Propietario reporta que la mascota está activa, come bien, sin síntomas de enfermedad. Última desparasitación hace 4 meses.",
    "objetivo": "Peso: 28.5 kg, Temperatura: 38.2°C, FC: 95 lpm, FR: 22 rpm. Mucosas rosadas y húmedas. Hidratación adecuada. Piel y pelaje en buen estado. Auscultación cardiopulmonar normal.",
    "analisis": "Mascota en excelente estado general. No se observan signos de enfermedad. Condición corporal óptima (5/9).",
    "plan": "1. Aplicar vacuna séxtuple\n2. Desparasitación con fenbendazol\n3. Revisión en 12 meses\n4. Mantener dieta actual",
    "diagnostico": "Mascota clínicamente sana",
    "tratamiento": "Vacuna séxtuple aplicada. Desparasitante: Fenbendazol 50mg/kg PO SID x 3 días",
    "proximaRevision": "2025-01-15T10:00:00",
    "adjuntosMedicos": [
      {
        "id": "guid",
        "tipo": "Analisis",
        "nombre": "Biometría hemática",
        "descripcion": "Resultados dentro de parámetros normales",
        "url": "https://storage.adopets.com/adjuntos/analisis-123.pdf",
        "fecha": "2024-01-15T10:30:00"
      }
    ],
    "creadoPor": "Dr. González",
    "fechaCreacion": "2024-01-15T11:00:00"
  }
}
```

**Códigos de Estado:**
- `200 OK` - Expediente encontrado
- `404 Not Found` - Expediente no encontrado
- `401 Unauthorized` - No autenticado
- `403 Forbidden` - Sin permisos
- `500 Internal Server Error` - Error del servidor

---

### 2. Obtener Expedientes por Mascota
**Endpoint:** `GET /api/expedientes/mascota/{mascotaId}`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Parámetros de Ruta:**
- `mascotaId` (Guid) - ID de la mascota

**Descripción:** Retorna el historial médico completo de una mascota, ordenado por fecha (más reciente primero).

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "numeroExpediente": "EXP-20240115-001",
      "fecha": "2024-01-15T10:30:00",
      "veterinario": "Dr. González",
      "motivoConsulta": "Revisión anual",
      "diagnostico": "Mascota clínicamente sana",
      "proximaRevision": "2025-01-15T10:00:00"
    },
    {
      "id": "guid",
      "numeroExpediente": "EXP-20230720-045",
      "fecha": "2023-07-20T14:00:00",
      "veterinario": "Dr. Martínez",
      "motivoConsulta": "Vómito y diarrea",
      "diagnostico": "Gastroenteritis aguda",
      "proximaRevision": "2023-07-27T14:00:00"
    }
  ]
}
```

---

### 3. Obtener Expedientes por Veterinario
**Endpoint:** `GET /api/expedientes/veterinario/{veterinarioId}`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Parámetros de Ruta:**
- `veterinarioId` (Guid) - ID del veterinario

**Descripción:** Retorna todos los expedientes creados por un veterinario específico.

**Uso Típico:** Estadísticas, auditoría, revisión de casos del veterinario.

---

### 4. Crear Nuevo Expediente
**Endpoint:** `POST /api/expedientes`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Descripción:** Crea un nuevo expediente médico siguiendo metodología SOAP.

**Cuerpo de la Solicitud:**
```json
{
  "mascotaId": "guid",
  "citaId": "guid",
  "motivoConsulta": "Vómito y diarrea hace 2 días",
  "subjetivo": "Propietario reporta que la mascota presenta vómito desde hace 2 días, aproximadamente 3 episodios diarios. Diarrea líquida. Decaimiento. Disminución del apetito. Sin fiebre aparente.",
  "objetivo": "Peso: 27.8 kg (pérdida de 700g desde última visita). Temperatura: 39.8°C. FC: 125 lpm. FR: 28 rpm. Deshidratación leve (5%). Mucosas pálidas. Dolor abdominal a la palpación. Sonidos intestinales aumentados.",
  "analisis": "Gastroenteritis aguda. Posible origen bacteriano vs. alimentario. Deshidratación leve. Requiere rehidratación y tratamiento sintomático. Considerar análisis coprológico.",
  "plan": "1. Fluidoterapia SC (Hartmann 200ml)\n2. Metoclopramida 0.5mg/kg SC\n3. Antibiótico: Metronidazol 15mg/kg PO BID x 7 días\n4. Dieta blanda x 3 días\n5. Análisis coprológico\n6. Revisión en 3 días",
  "diagnostico": "Gastroenteritis aguda",
  "tratamiento": "Fluidoterapia aplicada. Metoclopramida aplicada. Receta: Metronidazol 250mg tabletas, 1 tab c/12h x 7 días",
  "proximaRevision": "2024-01-18T10:00:00",
  "notas": "Cliente informado sobre signos de alarma: vómito persistente, sangre en heces, letargia severa"
}
```

**Validaciones:**
- `mascotaId` - Requerido
- `motivoConsulta` - Requerido, máximo 500 caracteres
- `subjetivo` - Requerido, máximo 2000 caracteres
- `objetivo` - Requerido, máximo 2000 caracteres
- `analisis` - Requerido, máximo 2000 caracteres
- `plan` - Requerido, máximo 2000 caracteres
- `diagnostico` - Requerido, máximo 1000 caracteres

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Expediente creado exitosamente",
  "data": {
    "id": "guid",
    "numeroExpediente": "EXP-20240115-002",
    "fecha": "2024-01-15T14:30:00",
    "mascotaId": "guid",
    "mascotaNombre": "Max",
    "veterinarioId": "guid",
    "veterinarioNombre": "Dr. González",
    "diagnostico": "Gastroenteritis aguda"
  }
}
```

**Códigos de Estado:**
- `201 Created` - Expediente creado exitosamente
- `400 Bad Request` - Datos inválidos
- `401 Unauthorized` - No autenticado
- `403 Forbidden` - Sin permisos
- `500 Internal Server Error` - Error del servidor

---

### 5. Eliminar Expediente
**Endpoint:** `DELETE /api/expedientes/{id}`

**Autorización:** Requerida (Rol: Admin únicamente)

**Parámetros de Ruta:**
- `id` (Guid) - ID del expediente

**Descripción:** Elimina permanentemente un expediente médico. **Esta acción es irreversible y debe usarse con extrema precaución.**

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Expediente eliminado exitosamente",
  "data": null
}
```

**Códigos de Estado:**
- `200 OK` - Eliminación exitosa
- `404 Not Found` - Expediente no encontrado
- `403 Forbidden` - Sin permisos de administrador
- `500 Internal Server Error` - Error del servidor

---

### 6. Agregar Adjunto Médico
**Endpoint:** `POST /api/expedientes/{id}/adjuntos`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Parámetros de Ruta:**
- `id` (Guid) - ID del expediente

**Descripción:** Agrega un archivo adjunto al expediente (rayos X, análisis de laboratorio, ecografías, etc.).

**Cuerpo de la Solicitud:**
```json
{
  "tipo": "RayoX",
  "nombre": "Radiografía de tórax lateral",
  "descripcion": "Se observa silueta cardíaca de tamaño normal. Campos pulmonares limpios. Sin evidencia de masas o cuerpos extraños.",
  "archivoBase64": "data:image/jpeg;base64,/9j/4AAQSkZJRg...",
  "nombreArchivo": "rayosx-torax-lateral.jpg",
  "tamañoBytes": 245678
}
```

**Tipos de Adjunto Soportados:**
- `RayoX` - Radiografías
- `Ecografia` - Ultrasonidos
- `Analisis` - Análisis de laboratorio
- `Imagen` - Fotografías clínicas
- `Documento` - PDFs, reportes
- `Otro` - Otros documentos médicos

**Validaciones:**
- `tipo` - Requerido
- `nombre` - Requerido, máximo 200 caracteres
- `archivoBase64` - Requerido (formato base64)
- `nombreArchivo` - Requerido
- Tamaño máximo: 10 MB por archivo

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Adjunto agregado exitosamente",
  "data": {
    "id": "guid",
    "expedienteId": "guid",
    "tipo": "RayoX",
    "nombre": "Radiografía de tórax lateral",
    "descripcion": "Se observa silueta cardíaca normal",
    "url": "https://storage.adopets.com/adjuntos/rayox-123.jpg",
    "nombreArchivo": "rayosx-torax-lateral.jpg",
    "tamañoBytes": 245678,
    "fechaSubida": "2024-01-15T15:00:00",
    "subidoPor": "Dr. González"
  }
}
```

**Códigos de Estado:**
- `200 OK` - Adjunto agregado exitosamente
- `400 Bad Request` - Archivo inválido o muy grande
- `404 Not Found` - Expediente no encontrado
- `403 Forbidden` - Sin permisos
- `500 Internal Server Error` - Error del servidor

---

### 7. Eliminar Adjunto Médico
**Endpoint:** `DELETE /api/expedientes/adjuntos/{adjuntoId}`

**Autorización:** Requerida (Roles: Admin, Veterinario)

**Parámetros de Ruta:**
- `adjuntoId` (Guid) - ID del adjunto

**Descripción:** Elimina un adjunto médico del sistema.

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Adjunto eliminado exitosamente",
  "data": null
}
```

---

## Estructura de Datos SOAP

### Template de Expediente Completo
```markdown
EXPEDIENTE MÉDICO
Número: EXP-20240115-001
Fecha: 15/01/2024 10:30 AM
Mascota: Max (Perro - Labrador, 3 años)
Veterinario: Dr. González

??????????????????????????????????????

MOTIVO DE CONSULTA:
Vómito y diarrea hace 2 días

S - SUBJETIVO (Historia Clínica):
Propietario reporta que la mascota presenta vómito desde 
hace 2 días, aproximadamente 3 episodios diarios. Diarrea 
líquida, color amarillento. Decaimiento notable. Disminución 
del apetito (come aproximadamente 30% de su ración normal). 
Sin fiebre aparente según el dueño. Sin acceso a basura ni 
cambios recientes en la dieta.

O - OBJETIVO (Examen Físico):
Actitud: Alerta pero decaída
Condición corporal: 4/9 (peso ideal ligeramente bajo)
Peso: 27.8 kg (pérdida de 700g desde última visita)
Temperatura: 39.8°C (elevada)
FC: 125 lpm (aumentada)
FR: 22 rpm (normal)
Deshidratación: Leve (5%), TLC >2 seg
Mucosas: Pálidas, húmedas
Palpación abdominal: Dolor moderado en región epigástrica
Sonidos intestinales: Aumentados (hipermotilidad)
Linfonodos: Sin alteraciones

A - ANÁLISIS (Diagnóstico):
Gastroenteritis aguda
Etiología probable: Bacteriana vs. alimentaria
Deshidratación leve secundaria
Pronóstico: Favorable con tratamiento adecuado
Diagnóstico diferencial:
  - Gastroenteritis viral
  - Intoxicación alimentaria
  - Parásitos intestinales
  - Enfermedad inflamatoria intestinal (menos probable)

P - PLAN (Tratamiento y Seguimiento):
Tratamiento inmediato:
  1. Fluidoterapia SC (Solución Hartmann 200ml)
  2. Antiemético: Metoclopramida 0.5mg/kg SC
  
Tratamiento en casa:
  3. Metronidazol 15mg/kg PO BID x 7 días
  4. Dieta blanda (pollo hervido + arroz) x 3 días
  5. Probióticos x 5 días
  
Estudios complementarios:
  6. Análisis coprológico (muestra en 24h)
  
Seguimiento:
  7. Revisión en 3 días
  8. Signos de alarma explicados al propietario

??????????????????????????????????????

TRATAMIENTO APLICADO:
? Fluidoterapia SC aplicada
? Metoclopramida aplicada
? Receta entregada

PRÓXIMA REVISIÓN: 18/01/2024

NOTAS:
Cliente informado sobre signos de alarma:
- Vómito persistente o con sangre
- Sangre en heces
- Letargia severa
- Negativa total a comer/beber
- Empeoramiento de síntomas

Indicado acudir a urgencias si presenta algún signo de alarma.
```

---

## Lógica de Negocio

### Generación Automática de Número de Expediente
```csharp
// Formato: EXP-YYYYMMDD-XXX
string fechaParte = DateTime.Now.ToString("yyyyMMdd");
int consecutivo = ObtenerConsecutivoDelDia();
string numeroExpediente = $"EXP-{fechaParte}-{consecutivo:D3}";
// Ejemplo: EXP-20240115-001
```

### Validación de Campos SOAP
- **Subjetivo:** Debe contener información del propietario/observador
- **Objetivo:** Debe incluir parámetros medibles (temperatura, peso, FC, FR)
- **Análisis:** Debe incluir diagnóstico y diagnóstico diferencial
- **Plan:** Debe incluir tratamiento específico y seguimiento

### Vinculación con Citas
- Un expediente puede estar vinculado a una cita
- No es obligatorio (emergencias pueden no tener cita previa)
- La cita debe estar en estado "Completada" o "EnProceso"

---

## Ejemplos de Uso

### Ejemplo 1: Crear Expediente de Consulta General
```bash
curl -X POST https://api.adopets.com/api/expedientes \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "mascotaId": "12345678-1234-1234-1234-123456789012",
    "citaId": "87654321-4321-4321-4321-210987654321",
    "motivoConsulta": "Revisión anual y aplicación de vacunas",
    "subjetivo": "Propietario indica que la mascota está saludable, activa, sin síntomas. Última vacunación hace 1 año.",
    "objetivo": "Peso: 28.5kg, T: 38.2°C, FC: 95lpm, FR: 22rpm. Condición corporal 5/9. Mucosas rosadas. Hidratación adecuada.",
    "analisis": "Mascota en excelente estado de salud. Sin hallazgos patológicos. Cumple con protocolo de vacunación anual.",
    "plan": "1. Aplicar vacuna séxtuple\n2. Desparasitación preventiva\n3. Revisión en 12 meses",
    "diagnostico": "Mascota clínicamente sana",
    "tratamiento": "Vacuna séxtuple aplicada. Desparasitante fenbendazol 50mg/kg",
    "proximaRevision": "2025-01-15T10:00:00"
  }'
```

### Ejemplo 2: Agregar Rayos X al Expediente
```bash
curl -X POST https://api.adopets.com/api/expedientes/12345678-1234-1234-1234-123456789012/adjuntos \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tipo": "RayoX",
    "nombre": "Radiografía de cadera bilateral",
    "descripcion": "Displasia de cadera grado leve en articulación derecha",
    "archivoBase64": "data:image/jpeg;base64,/9j/4AAQ...",
    "nombreArchivo": "rayosx-cadera.jpg",
    "tamañoBytes": 456789
  }'
```

### Ejemplo 3: Consultar Historial de Mascota
```bash
curl -X GET https://api.adopets.com/api/expedientes/mascota/12345678-1234-1234-1234-123456789012 \
  -H "Authorization: Bearer {token}"
```

---

## Integración con Otros Módulos

### Módulo de Citas
- Al completar una cita, se puede crear automáticamente un expediente
- El expediente queda vinculado a la cita

### Módulo de Vacunaciones
- Las vacunas aplicadas se registran en el expediente
- Se vinculan automáticamente al plan de vacunación

### Módulo de Cirugías
- Las cirugías requieren un expediente previo (valoración pre-quirúrgica)
- El expediente post-quirúrgico documenta el resultado

### Módulo de Valoraciones
- Las valoraciones (signos vitales) se extraen del campo Objetivo
- Pueden registrarse por separado para tracking continuo

---

## Buenas Prácticas

### 1. Documentación Completa
- **Llenar todos los campos SOAP** - Cada sección aporta valor clínico
- **Ser específico** - "Temperatura: 39.8°C" es mejor que "fiebre"
- **Usar terminología médica** - Mantener profesionalismo

### 2. Objetividad
- **Sección Subjetiva** - Solo lo que reporta el propietario
- **Sección Objetiva** - Solo hallazgos medibles del examen
- **No mezclar secciones** - Mantener la estructura clara

### 3. Diagnóstico Diferencial
- Siempre considerar diagnósticos alternativos
- Documentar por qué se descartaron otras opciones
- Ayuda en casos complejos o de segunda opinión

### 4. Plan de Seguimiento
- Siempre incluir fecha de próxima revisión
- Documentar signos de alarma explicados al cliente
- Instrucciones claras de tratamiento en casa

### 5. Adjuntos Médicos
- Nombrar archivos descriptivamente
- Incluir descripción de hallazgos en cada adjunto
- Organizar por tipo (RayoX, Análisis, etc.)

---

## Reportes y Estadísticas

### Datos Útiles del Módulo
- Total de expedientes por mascota
- Expedientes por veterinario
- Diagnósticos más comunes
- Seguimiento de tratamientos
- Mascotas con expedientes pendientes de revisión

---

## Notas Técnicas

### Seguridad
- Solo veterinarios y administradores pueden crear expedientes
- Los expedientes contienen información médica sensible (HIPAA-compliant)
- Auditoría completa (quién creó, cuándo, modificaciones)

### Almacenamiento
- Campos de texto largo optimizados para búsqueda full-text
- Adjuntos almacenados en blob storage (Azure/AWS)
- Backup diario de expedientes médicos

### Performance
- Índices en `MascotaId`, `VeterinarioId`, `Fecha`
- Paginación recomendada para listados grandes
- Cache de expedientes frecuentemente consultados

---

## Contacto y Soporte
**Desarrollador Responsable:** Developer 3 - Beto  
**Módulo:** Clínica & Historial Médico  
**Versión API:** 1.0  
**Última Actualización:** Enero 2024  
**Metodología:** SOAP (Subjetivo, Objetivo, Análisis, Plan)
