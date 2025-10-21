# Documentación de la Base de Datos - AdoPets Backend

## Índice
1. [Visión General](#visión-general)
2. [Arquitectura de Entidades](#arquitectura-de-entidades)
3. [Módulo de Seguridad](#módulo-de-seguridad)
4. [Módulo de Mascotas](#módulo-de-mascotas)
5. [Módulo de Clínica](#módulo-de-clínica)
6. [Módulo de Historial Clínico](#módulo-de-historial-clínico)
7. [Módulo de Inventario](#módulo-de-inventario)
8. [Módulo de Donaciones](#módulo-de-donaciones)
9. [Módulo de Servicios](#módulo-de-servicios)
10. [Módulo de Auditoría](#módulo-de-auditoría)
11. [Relaciones entre Módulos](#relaciones-entre-módulos)
12. [Patrones de Diseño Aplicados](#patrones-de-diseño-aplicados)

---

## Visión General

**AdoPets** es un sistema integral para la gestión de refugios de animales que combina funcionalidades de:
- Gestión de mascotas y adopciones
- Clínica veterinaria con historial médico completo
- Control de inventario médico con sistema FIFO
- Gestión de donaciones mediante PayPal
- Administración de personal y servicios
- Auditoría y trazabilidad completa

### Filosofía de Diseño

La base de datos está construida siguiendo principios de **Domain-Driven Design (DDD)** con:
- Separación clara de dominios mediante módulos
- Entidades base con auditoría automática
- Soft delete para datos sensibles
- Trazabilidad completa de cambios críticos
- Normalización apropiada para evitar redundancia

---

## Arquitectura de Entidades

### Jerarquía de Entidades Base

La arquitectura utiliza tres clases base abstractas que proporcionan funcionalidad común:

#### 1. **BaseEntity**
```
- Id: Guid (PK)
```
**Propósito**: Entidad base mínima que proporciona identificador único usando GUID para facilitar la distribución y evitar colisiones.

**Por qué GUID**: 
- Permite generación de IDs en el cliente sin conflictos
- Facilita replicación y sincronización entre sistemas
- Mayor seguridad al no exponer secuencias predecibles

#### 2. **AuditableEntity** (hereda de BaseEntity)
```
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime? (UTC)
- CreatedBy: Guid? (FK Usuario)
- UpdatedBy: Guid? (FK Usuario)
```
**Propósito**: Añade campos de auditoría temporal y de autoría para rastrear cuándo y quién creó o modificó un registro.

**Por qué auditoría automática**:
- Cumplimiento con requisitos legales y normativos
- Trazabilidad para resolución de disputas
- Análisis de comportamiento de usuarios
- Debugging y soporte técnico

#### 3. **SoftDeletableEntity** (hereda de AuditableEntity)
```
- DeletedAt: DateTime? (UTC)
- DeletedBy: Guid? (FK Usuario)
- IsDeleted: bool (computed)
```
**Propósito**: Implementa eliminación lógica en lugar de física, preservando datos históricos.

**Por qué soft delete**:
- Recuperación de datos eliminados accidentalmente
- Cumplimiento con regulaciones de retención de datos
- Integridad referencial sin cascadas destructivas
- Análisis histórico completo

---

## Módulo de Seguridad

Este módulo maneja la autenticación, autorización, notificaciones y gestión de dispositivos.

### Entidades

#### **Usuario** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido)
- ApellidoPaterno: string (requerido)
- ApellidoMaterno: string (requerido)
- Email: string (requerido, único)
- Telefono: string?
- PasswordHash: string (requerido)
- PasswordSalt: string (requerido)
- Estatus: enum EstatusUsuario (default: Activo)
- UltimoAccesoAt: DateTime?
- AceptoPoliticasVersion: string?
- AceptoPoliticasAt: DateTime?
```

**Propósito**: Representa cualquier usuario del sistema (adoptantes, veterinarios, administradores, etc.)

**Por qué estos campos**:
- **Nombre completo separado**: Facilita búsquedas y ordenamiento por apellido
- **PasswordHash/Salt**: Almacenamiento seguro usando hashing + salt único por usuario
- **Estatus enum**: Control de acceso sin eliminar usuarios (suspensión, bloqueo)
- **UltimoAccesoAt**: Detección de cuentas inactivas, análisis de uso
- **AceptoPoliticasVersion**: Cumplimiento GDPR/LFPDPPP - requiere re-aceptación al cambiar políticas

**Relaciones**:
- 1:N con UsuarioRol (un usuario puede tener múltiples roles)
- 1:N con Dispositivo (un usuario puede tener múltiples dispositivos registrados)
- 1:N con Notificacion (un usuario recibe múltiples notificaciones)
- 1:N con ConsentimientoPolitica (historial de aceptaciones)
- 1:N con SolicitudAdopcion (un usuario puede solicitar adoptar múltiples mascotas)

---

#### **Rol** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, único)
- Descripcion: string?
```

**Propósito**: Define roles del sistema para autorización basada en roles (RBAC).

**Roles predefinidos**:
- **Admin**: Control total del sistema
- **Veterinario**: Gestión clínica y expedientes médicos
- **Recepcionista**: Gestión de citas y atención al público
- **Asistente**: Apoyo en procedimientos veterinarios
- **Adoptante**: Usuario que busca adoptar mascotas

**Por qué este diseño**:
- Separación de concerns entre autenticación y autorización
- Flexibilidad para añadir nuevos roles sin cambiar código
- Un usuario puede tener múltiples roles simultáneamente

**Relaciones**:
- N:M con Usuario a través de UsuarioRol

---

#### **UsuarioRol** (Tabla de Unión)
```sql
Campos:
- UsuarioId: Guid (FK Usuario, PK compuesta)
- RolId: Guid (FK Rol, PK compuesta)
- AsignadoAt: DateTime (default: now)
- AsignadoPor: Guid? (FK Usuario)
```

**Propósito**: Implementa la relación N:M entre Usuario y Rol con auditoría.

**Por qué campos adicionales**:
- **AsignadoAt**: Rastrear cuándo se otorgó el rol
- **AsignadoPor**: Accountability - quién otorgó permisos

---

#### **Dispositivo** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario, requerido)
- DeviceToken: string (requerido, único)
- Platform: enum (iOS, Android, Web)
- DeviceInfo: string?
- Activo: bool (default: true)
- UltimaConexion: DateTime?
```

**Propósito**: Gestión de dispositivos para notificaciones push y seguridad.

**Por qué es necesario**:
- Envío de notificaciones push específicas por dispositivo
- Seguridad: revocar acceso a dispositivos comprometidos
- Análisis de uso multi-dispositivo

---

#### **Notificacion** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- TipoNotificacion: enum
- UsuarioId: Guid (FK Usuario)
- Titulo: string (requerido)
- Mensaje: string (requerido)
- DataJson: string? (JSON adicional)
- Fecha: DateTime (default: now)
- DeliveredAt: DateTime?
- ReadAt: DateTime?
```

**Propósito**: Sistema de notificaciones in-app y push.

**Tipos de notificación**:
- AdoptionStatusChange: Cambios en solicitud de adopción
- AppointmentReminder: Recordatorios de citas
- InventoryLow: Alertas de stock bajo
- InventoryExpiring: Insumos próximos a vencer
- DonationReceived: Confirmación de donación
- System: Notificaciones generales del sistema

**Por qué tres timestamps**:
- **Fecha**: Cuándo se creó la notificación
- **DeliveredAt**: Cuándo llegó al dispositivo (tracking de entrega)
- **ReadAt**: Cuándo el usuario la leyó (engagement)

---

#### **PreferenciaNotificacion** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario)
- TipoNotificacion: enum
- Email: bool (default: true)
- Push: bool (default: true)
- InApp: bool (default: true)
```

**Propósito**: Permitir a usuarios controlar qué notificaciones reciben y por qué canal.

**Por qué es importante**:
- Cumplimiento con regulaciones anti-spam
- Mejor experiencia de usuario
- Reducción de tasa de desinstalación

---

#### **ConsentimientoPolitica** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario)
- Version: string (requerido)
- ConsentType: enum (TermsOfService, PrivacyPolicy, DataProcessing)
- AcceptedAt: DateTime (default: now)
- IpAddress: string?
- UserAgent: string?
```

**Propósito**: Historial de aceptaciones de políticas para cumplimiento legal.

**Por qué IP y UserAgent**:
- Evidencia legal de consentimiento informado
- Prevención de fraude
- Cumplimiento GDPR/LFPDPPP

---

## Módulo de Mascotas

Gestión completa de mascotas del refugio y proceso de adopción.

### Entidades

#### **Mascota** (SoftDeletableEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido)
- Especie: string (requerido) - perro, gato, conejo, etc.
- Raza: string?
- FechaNacimiento: DateTime?
- Sexo: enum SexoMascota (Macho, Hembra, Desconocido)
- Estatus: enum EstatusMascota (default: Disponible)
- Personalidad: string? (TEXT) - descripción temperamento
- EstadoSalud: string? (TEXT) - condición general
- RequisitoAdopcion: string? (TEXT) - requisitos especiales
- Origen: string? - cómo llegó al refugio
- Notas: string? (TEXT) - información adicional
```

**Propósito**: Entidad central que representa cada mascota del refugio.

**Estatus posibles**:
- **Disponible**: Lista para adopción
- **Reservada**: Proceso de adopción en curso
- **Adoptada**: Ya fue adoptada
- **NoAdoptable**: No apta para adopción (agresividad, enfermedad crónica)
- **EnTratamiento**: Bajo cuidados médicos intensivos

**Por qué soft delete**:
- Preservar historial de mascotas fallecidas
- Mantener integridad de expedientes médicos
- Análisis estadístico completo del refugio

**Por qué campos de texto libre**:
- **Personalidad**: Cada mascota es única, texto libre permite descripciones detalladas
- **RequisitoAdopcion**: Algunos animales necesitan condiciones especiales (casa con jardín, sin niños, etc.)
- **Origen**: Información de contexto importante (rescate, abandono, entregado por dueño)

**Relaciones**:
- 1:N con MascotaFoto (galería de imágenes)
- 1:N con SolicitudAdopcion (múltiples personas pueden solicitarla)
- 1:N con Expediente (historial médico completo)
- 1:N con Vacunacion, Desparasitacion, Cirugia, Valoracion

---

#### **MascotaFoto** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- Url: string (requerido)
- IsPrincipal: bool (default: false)
- Description: string?
- UploadedAt: DateTime (default: now)
```

**Propósito**: Gestión de galería de fotos para cada mascota.

**Por qué separado**:
- Múltiples fotos por mascota
- Optimización de consultas (no cargar todas las fotos siempre)
- IsPrincipal marca la foto de portada
- Facilita integración con CDN/storage externo

---

#### **SolicitudAdopcion** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario, requerido)
- MascotaId: Guid (FK Mascota, requerido)
- Estado: enum EstadoSolicitudAdopcion (default: Pendiente)
- Vivienda: enum TipoVivienda (Casa, Departamento, Quinta)
- NumNinios: int
- OtrasMascotas: bool
- HorasDisponibilidad: int - horas al día disponibles
- Direccion: string (requerido)
- IngresosMensuales: decimal?
- MotivoAdopcion: string? (TEXT)
- MotivoRechazo: string? (TEXT)
- FechaSolicitud: DateTime (default: now)
- FechaRevision: DateTime?
- FechaAprobacion: DateTime?
- RevisadoPor: Guid? (FK Usuario)
```

**Propósito**: Formulario de solicitud y proceso de adopción.

**Estados del proceso**:
1. **Pendiente**: Solicitud enviada, esperando revisión
2. **EnRevision**: Un empleado está evaluando la solicitud
3. **Aprobada**: Adoptante puede proceder con la adopción
4. **Rechazada**: No cumple requisitos
5. **Cancelada**: Adoptante desistió

**Por qué estos campos de evaluación**:
- **Vivienda**: Determinar si el espacio es adecuado
- **NumNinios**: Evaluar ambiente familiar (algunas mascotas no toleran niños)
- **OtrasMascotas**: Compatibilidad con otros animales
- **HorasDisponibilidad**: Asegurar que tendrá tiempo suficiente
- **IngresosMensuales**: Capacidad económica para mantener la mascota
- **MotivoAdopcion**: Detectar adopciones impulsivas o motivos inadecuados

**Por qué múltiples timestamps**:
- Análisis de tiempos de respuesta del refugio
- SLA (Service Level Agreement) interno
- Métricas de eficiencia operacional

**Relaciones**:
- N:1 con Usuario (solicitante)
- N:1 con Mascota (mascota solicitada)
- 1:N con SolicitudAdopcionAdjunto (documentos soporte)
- 1:N con AdopcionLog (historial de cambios)

---

#### **SolicitudAdopcionAdjunto** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- SolicitudId: Guid (FK SolicitudAdopcion)
- TipoAdjunto: enum (ComprobanteDomicilio, INE, ComprobanteIngresos, Otro)
- Url: string (requerido)
- FileName: string?
- FileSize: long?
- UploadedAt: DateTime (default: now)
```

**Propósito**: Documentación soporte de la solicitud de adopción.

**Por qué necesario**:
- Verificación de identidad
- Validación de dirección
- Comprobación de capacidad económica
- Requisito legal en algunas jurisdicciones

---

#### **AdopcionLog** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- SolicitudId: Guid (FK SolicitudAdopcion)
- FromEstado: enum EstadoSolicitudAdopcion
- ToEstado: enum EstadoSolicitudAdopcion
- Reason: string? (TEXT)
- ChangedBy: Guid (FK Usuario)
- ChangedAt: DateTime (default: now)
```

**Propósito**: Trazabilidad completa de cambios de estado en adopciones.

**Por qué es crítico**:
- Auditoría de decisiones
- Resolución de disputas
- Detección de patrones (ej: mismo usuario con múltiples rechazos)
- Análisis de proceso de adopción

---

## Módulo de Clínica

Gestión de citas veterinarias, salas y recordatorios.

### Entidades

#### **Cita** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid? (FK Mascota) - nullable para mascotas externas
- PropietarioId: Guid? (FK Usuario) - dueño de mascota externa
- VeterinarioId: Guid (FK Usuario, requerido)
- SalaId: Guid? (FK Sala)
- Tipo: enum TipoCita
- Status: enum StatusCita (default: Programada)
- StartAt: DateTime (requerido)
- EndAt: DateTime (requerido)
- DuracionMin: int
- Notas: string? (TEXT)
- MotivoConsulta: string? (TEXT)
- MotivoRechazo: string? (TEXT)
- PagoId: Guid? - reservado para módulo de pagos futuro
```

**Propósito**: Agenda veterinaria con prevención de conflictos de horario.

**Tipos de cita**:
- Consulta, Cirugia, Baño, Vacuna, Procedimiento, Urgencia, Seguimiento

**Por qué MascotaId nullable**:
- Permite atender mascotas que no pertenecen al refugio
- Servicio veterinario externo como fuente de ingresos
- PropietarioId identifica al dueño externo

**Por qué StartAt y EndAt**:
- Prevención de solapamiento de citas
- Cálculo automático de disponibilidad
- Optimización de uso de salas

**Relaciones**:
- N:1 con Mascota (opcional)
- N:1 con Usuario (propietario opcional)
- N:1 con Usuario (veterinario obligatorio)
- N:1 con Sala (opcional)
- 1:N con CitaRecordatorio (recordatorios automáticos)
- 1:N con CitaHistorialEstado (trazabilidad)
- 1:1 con ReporteUsoInsumos (insumos usados en la cita)

---

#### **Sala** (SoftDeletableEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, único)
- Descripcion: string?
- Capacidad: int (default: 1)
- Activa: bool (default: true)
```

**Propósito**: Gestión de espacios físicos para citas.

**Por qué necesario**:
- Prevención de doble reserva de sala
- Optimización de recursos físicos
- Capacidad permite salas compartidas (ej: área de vacunación)

---

#### **CitaRecordatorio** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- CitaId: Guid (FK Cita, requerido)
- TipoRecordatorio: enum (Email, Push, SMS)
- MinutosAntes: int - cuándo enviar antes de la cita
- Enviado: bool (default: false)
- EnviadoAt: DateTime?
- Error: string?
```

**Propósito**: Sistema automatizado de recordatorios de citas.

**Por qué múltiples recordatorios**:
- Reducción de no-shows
- Múltiples canales aumentan efectividad
- MinutosAntes configurable (24h antes, 1h antes, etc.)

---

#### **CitaHistorialEstado** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- CitaId: Guid (FK Cita, requerido)
- FromStatus: enum StatusCita
- ToStatus: enum StatusCita
- ChangedBy: Guid (FK Usuario)
- ChangedAt: DateTime (default: now)
- Notas: string?
```

**Propósito**: Auditoría de cambios de estado de citas.

**Por qué importante**:
- Detectar cancelaciones frecuentes
- Métricas de cumplimiento de citas
- Responsabilidad por cambios

---

## Módulo de Historial Clínico

Expediente médico completo de cada mascota.

### Entidades

#### **Expediente** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- VeterinarioId: Guid (FK Usuario, requerido)
- CitaId: Guid? (FK Cita) - si deriva de una cita
- MotivoConsulta: string? (TEXT)
- Anamnesis: string? (TEXT) - historia y síntomas
- Diagnostico: string (TEXT, requerido)
- Tratamiento: string? (TEXT)
- Notas: string? (TEXT)
- Pronostico: string? (TEXT)
- Fecha: DateTime (default: now)
```

**Propósito**: Nota clínica o expediente médico de una consulta.

**Por qué estructura SOAP**:
- **MotivoConsulta**: Razón de la visita
- **Anamnesis**: Historia y síntomas (Subjective)
- **Diagnostico**: Evaluación médica (Assessment)
- **Tratamiento**: Plan de acción (Plan)
- **Pronostico**: Expectativas de recuperación

**Relaciones**:
- N:1 con Mascota (múltiples consultas por mascota)
- 1:N con AdjuntoMedico (rayos X, análisis, etc.)

---

#### **AdjuntoMedico** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ExpedienteId: Guid (FK Expediente, requerido)
- TipoAdjunto: enum (RayoX, Ecografia, Analisis, Foto, Documento)
- Url: string (requerido)
- FileName: string?
- Description: string?
- UploadedAt: DateTime (default: now)
```

**Propósito**: Almacenar imágenes médicas y documentos relacionados.

**Por qué separado**:
- Optimización: no cargar archivos pesados siempre
- Permite múltiples archivos por expediente
- Integración con PACS (Picture Archiving and Communication System) futuro

---

#### **Vacunacion** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- VaccineName: string (requerido)
- Dose: string? - primera, segunda, refuerzo
- Lot: string? - lote de la vacuna (trazabilidad)
- AppliedAt: DateTime (default: now)
- NextDueAt: DateTime? - cuándo toca refuerzo
- VeterinarioId: Guid (FK Usuario, requerido)
- Notes: string? (TEXT)
- ReaccionAdversa: string? (TEXT)
```

**Propósito**: Historial de vacunación completo.

**Por qué Lot importante**:
- Retiros de lotes defectuosos
- Trazabilidad regulatoria
- Investigación de reacciones adversas

**Por qué NextDueAt**:
- Recordatorios automáticos
- Seguimiento de plan de vacunación
- Reportes de mascotas pendientes

---

#### **Desparasitacion** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- ProductName: string (requerido)
- TipoParasito: enum (Interno, Externo, Ambos)
- AppliedAt: DateTime (default: now)
- NextDueAt: DateTime?
- VeterinarioId: Guid (FK Usuario, requerido)
- Peso: decimal? - peso al momento de aplicación (dosificación)
- Notes: string?
```

**Propósito**: Control de desparasitaciones periódicas.

**Por qué peso**:
- Dosificación correcta depende del peso
- Tracking de crecimiento de cachorros
- Ajuste de tratamiento

---

#### **Cirugia** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- Tipo: string (requerido) - esterilización, fractura, tumor, etc.
- Descripcion: string? (TEXT)
- PerformedAt: DateTime (default: now)
- VeterinarioId: Guid (FK Usuario, requerido)
- Anesthesia: string? - tipo y dosis de anestesia
- DuracionMin: int?
- Complications: bool - si hubo complicaciones
- Notes: string? (TEXT)
- Medicacion: string? (TEXT) - post-operatoria
- CuidadosPostoperatorios: string? (TEXT)
- FechaRevision: DateTime? - cita de seguimiento
```

**Propósito**: Registro detallado de procedimientos quirúrgicos.

**Por qué tan detallado**:
- Referencia para cirugías futuras
- Información crítica para otros veterinarios
- Análisis de complicaciones
- Estándar médico veterinario

---

#### **Valoracion** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- VeterinarioId: Guid (FK Usuario)
- Peso: decimal?
- Temperatura: decimal?
- FrecuenciaCardiaca: int?
- FrecuenciaRespiratoria: int?
- CondicionCorporal: enum (MuyDelgado, Delgado, Ideal, Sobrepeso, Obeso)
- Fecha: DateTime (default: now)
- Notas: string? (TEXT)
```

**Propósito**: Signos vitales y evaluación física rutinaria.

**Por qué separado de Expediente**:
- Tomas rápidas sin consulta completa
- Seguimiento de evolución de peso
- Detección temprana de anomalías
- Estadísticas de salud general

---

## Módulo de Inventario

Sistema completo de gestión de inventario médico con control FIFO (First In, First Out).

### Entidades

#### **ItemInventario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, único)
- Unidad: string (requerido) - pz, ml, mg, kg, etc.
- Categoria: enum CategoriaInventario
- MinQty: decimal - stock mínimo para alertas
- Activo: bool (default: true)
- Notas: string? (TEXT)
- Descripcion: string? (TEXT)
```

**Propósito**: Catálogo maestro de insumos médicos.

**Categorías**:
- Medicamento, Vacuna, Alimento, MaterialCuracion, Quirurgico, Limpieza, Otro

**Por qué MinQty**:
- Generación automática de alertas de reabastecimiento
- Prevención de desabasto
- Optimización de compras

**Relaciones**:
- 1:N con LoteInventario (un item tiene múltiples lotes)
- 1:N con MovimientoInventario (historial de entradas/salidas)
- 1:N con AlertaInventario (alertas activas)

---

#### **LoteInventario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ItemId: Guid (FK ItemInventario, requerido)
- Lote: string (requerido)
- ExpDate: DateTime? - fecha de expiración
- QtyDisponible: decimal (requerido)
- QtyInicial: decimal (requerido)
- CreatedAt: DateTime (default: now)
- Notas: string?
```

**Propósito**: Control de lotes individuales con fechas de vencimiento.

**Por qué sistema de lotes**:
- **Trazabilidad**: Retiros de lotes defectuosos
- **FIFO**: Usar primero los que vencen primero
- **Auditoría**: Saber exactamente de dónde vino cada medicamento usado
- **Regulatorio**: Requisito legal para medicamentos controlados

**Por qué QtyInicial y QtyDisponible separados**:
- QtyInicial: referencia histórica
- QtyDisponible: stock actual
- Permite calcular consumo histórico del lote

**Relaciones**:
- N:1 con ItemInventario
- 1:N con MovimientoInventario (tracking de uso)
- 1:N con ReporteUsoSplitLote (uso en procedimientos)

---

#### **MovimientoInventario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ItemId: Guid (FK ItemInventario, requerido)
- LoteId: Guid? (FK LoteInventario) - null para movimientos generales
- TipoMovimiento: enum (Entrada, Salida, Ajuste, Vencimiento, Perdida)
- Cantidad: decimal (requerido)
- RazonMovimiento: string? (TEXT)
- ReferenciaId: Guid? - FK a Compra, ReporteUso, etc.
- ReferenciaType: string? - tipo de referencia
- UsuarioId: Guid? (FK Usuario)
- Fecha: DateTime (default: now)
```

**Propósito**: Registro de cada movimiento de inventario (entrada/salida).

**Por qué tipos de movimiento**:
- **Entrada**: Compra nueva
- **Salida**: Uso en procedimientos
- **Ajuste**: Corrección de conteos
- **Vencimiento**: Desecho por expiración
- **Perdida**: Robo, daño, etc.

**Por qué Referencia polimórfica**:
- ReferenciaId + ReferenciaType permiten vincular a diferentes entidades
- Flexibilidad sin crear múltiples FK opcionales
- Trazabilidad de origen/destino

---

#### **AlertaInventario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ItemId: Guid (FK ItemInventario, requerido)
- TipoAlerta: enum (StockBajo, ProximoVencimiento, Vencido)
- Mensaje: string (requerido)
- Severity: enum (Info, Warning, Critical)
- IsActive: bool (default: true)
- CreatedAt: DateTime (default: now)
- ResolvedAt: DateTime?
- ResolvedBy: Guid? (FK Usuario)
```

**Propósito**: Sistema de alertas automáticas de inventario.

**Por qué necesario**:
- Notificaciones proactivas
- Prevención de desabasto
- Reducción de desperdicio por vencimiento
- Dashboard de administración

---

#### **Proveedor** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, único)
- Contacto: string?
- Telefono: string?
- Email: string?
- Direccion: string? (TEXT)
- Activo: bool (default: true)
- Notas: string? (TEXT)
```

**Propósito**: Catálogo de proveedores de insumos.

**Por qué necesario**:
- Gestión de compras
- Evaluación de proveedores
- Información de contacto centralizada

---

#### **Compra** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- ProveedorId: Guid (FK Proveedor, requerido)
- NumeroFactura: string?
- FechaCompra: DateTime (default: now)
- Total: decimal (requerido)
- Status: enum StatusCompra (default: Pendiente)
- Notas: string? (TEXT)
- FechaRecepcion: DateTime?
- RecibidoPor: Guid? (FK Usuario)
```

**Propósito**: Registro de órdenes de compra a proveedores.

**Estados de compra**:
- **Pendiente**: Orden creada, esperando entrega
- **Confirmada**: Proveedor confirmó
- **Recibida**: Insumos recibidos completamente
- **Cancelada**: Orden cancelada
- **Parcial**: Recepción parcial

**Por qué FechaRecepcion separada**:
- Diferencia entre fecha de compra y recepción real
- Cálculo de tiempos de entrega de proveedores
- Auditoría

**Relaciones**:
- N:1 con Proveedor
- 1:N con DetalleCompra (items específicos de la compra)

---

#### **DetalleCompra** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- CompraId: Guid (FK Compra, requerido)
- ItemId: Guid (FK ItemInventario, requerido)
- Cantidad: decimal (requerido)
- PrecioUnitario: decimal (requerido)
- Lote: string?
- FechaExpiracion: DateTime?
```

**Propósito**: Detalle línea por línea de una compra.

**Por qué separado**:
- Una compra contiene múltiples items
- Normalización
- Cálculo automático de totales
- Creación automática de lotes al recibir

---

#### **ReporteUsoInsumos** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- CitaId: Guid (FK Cita, requerido, único)
- VetId: Guid (FK Usuario, requerido)
- Status: enum StatusReporte (Registrado, Revertido)
- SubmittedAt: DateTime (default: now)
- Notes: string? (TEXT)
- ClientUsageId: string? (único) - para idempotencia
- RevertedAt: DateTime?
- RevertedBy: Guid? (FK Usuario)
- RevertReason: string? (TEXT)
```

**Propósito**: Reporte post-procedimiento de insumos usados con descuento automático FIFO.

**Por qué necesario**:
- Registro preciso de consumo
- Actualización automática de inventario
- Costeo de procedimientos
- Auditoría de uso

**Por qué ClientUsageId**:
- **Idempotencia**: Prevenir duplicados si se reenvía la petición
- Seguridad contra errores de red

**Por qué reversible**:
- Corrección de errores
- Cancelación de procedimientos
- Auditoría completa (no delete)

**Relaciones**:
- 1:1 con Cita (un reporte por cita)
- 1:N con ReporteUsoInsumoDetalle (items específicos usados)

---

#### **ReporteUsoInsumoDetalle** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ReporteId: Guid (FK ReporteUsoInsumos, requerido)
- ItemId: Guid (FK ItemInventario, requerido)
- QtyUsada: decimal (requerido)
```

**Propósito**: Detalle de insumos usados en un procedimiento.

**Por qué no tiene LoteId directo**:
- El sistema automáticamente selecciona lotes con FIFO
- Puede usar múltiples lotes (si uno no tiene suficiente stock)

**Relaciones**:
- N:1 con ReporteUsoInsumos
- 1:N con ReporteUsoSplitLote (muestra qué lotes se usaron)

---

#### **ReporteUsoSplitLote** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ReporteDetalleId: Guid (FK ReporteUsoInsumoDetalle, requerido)
- LoteId: Guid (FK LoteInventario, requerido)
- QtyUsada: decimal (requerido)
```

**Propósito**: Registro exacto de qué lotes se usaron y en qué cantidad.

**Por qué tabla adicional**:
- **Trazabilidad total**: Saber exactamente qué lote de medicamento recibió cada mascota
- **FIFO multi-lote**: Si un lote no tiene suficiente, se completa con el siguiente
- **Auditoría**: Retiros de lotes o investigación de reacciones adversas

**Ejemplo**:
```
Procedimiento necesita 50ml de medicamento X
- Lote A (vence antes) tiene 30ml ? usa 30ml
- Lote B (vence después) tiene 100ml ? usa 20ml
Resultado: 2 registros en ReporteUsoSplitLote
```

---

## Módulo de Donaciones

Gestión de donaciones mediante integración con PayPal.

### Entidades

#### **Donacion** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid? (FK Usuario) - nullable para donantes anónimos
- Monto: decimal (requerido)
- Moneda: string (default: "MXN")
- Status: enum StatusDonacion (default: PENDING)
- PaypalOrderId: string (requerido, único)
- PaypalCaptureId: string? - ID de captura de PayPal
- PayerEmail: string?
- PayerName: string?
- PaypalPayerId: string?
- Source: enum SourceDonacion (Checkout, Webhook, Manual)
- Mensaje: string? (TEXT) - mensaje del donante
- Anonima: bool - ocultar en listados públicos
- CapturedAt: DateTime?
- CancelledAt: DateTime?
- CancellationReason: string? (TEXT)
```

**Propósito**: Registro de donaciones monetarias mediante PayPal.

**Estados de donación**:
- **PENDING**: Orden creada, esperando pago
- **CAPTURED**: Pago capturado exitosamente
- **CANCELLED**: Donante canceló
- **FAILED**: Pago falló
- **REFUNDED**: Devolución procesada

**Por qué múltiples IDs de PayPal**:
- **PaypalOrderId**: ID de la orden (creada antes del pago)
- **PaypalCaptureId**: ID de la captura (confirmación del pago)
- **PaypalPayerId**: ID del pagador en PayPal
- Necesario para reconciliación y soporte

**Por qué Source**:
- **Checkout**: Donación desde el sitio web
- **Webhook**: Procesada por webhook de PayPal
- **Manual**: Ingresada manualmente por admin

**Por qué Anonima**:
- Respeto a preferencias de privacidad
- Muro de donaciones público puede ocultar nombres

---

#### **WebhookEvent** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- EventType: string (requerido)
- EventId: string (requerido, único) - ID de PayPal
- Payload: string (TEXT, requerido) - JSON completo del webhook
- ProcessedAt: DateTime?
- Success: bool?
- ErrorMessage: string? (TEXT)
- CreatedAt: DateTime (default: now)
```

**Propósito**: Log de webhooks de PayPal para debugging e idempotencia.

**Por qué necesario**:
- **Idempotencia**: Evitar procesar el mismo evento dos veces
- **Debugging**: Ver payload exacto recibido
- **Auditoría**: Seguimiento de eventos de PayPal
- **Re-procesamiento**: Reintentar eventos fallidos

---

## Módulo de Servicios

Gestión de empleados, especialidades, servicios y horarios.

### Entidades

#### **Empleado** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario, requerido, único)
- Cedula: string? - cédula profesional (veterinarios)
- Disponibilidad: string? (TEXT)
- EmailLaboral: string?
- TelefonoLaboral: string?
- Tipo: enum TipoEmpleado
- Sueldo: decimal?
- FechaContratacion: DateTime?
- FechaBaja: DateTime?
- Activo: bool (default: true)
```

**Propósito**: Información extendida de empleados del refugio.

**Tipos de empleado**:
- Veterinario, Asistente, Recepcionista, Administrador, Voluntario

**Por qué separado de Usuario**:
- No todos los usuarios son empleados (adoptantes, donantes)
- Información laboral específica
- Relación 1:1 opcional con Usuario

**Por qué Cedula**:
- Requisito legal para veterinarios
- Validación de credenciales profesionales

**Relaciones**:
- 1:1 con Usuario
- N:M con Especialidad a través de EmpleadoEspecialidad
- 1:N con Horario (turnos de trabajo)

---

#### **Especialidad** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, único)
- Descripcion: string? (TEXT)
```

**Propósito**: Catálogo de especialidades veterinarias.

**Ejemplos**:
- Cirugía, Dermatología, Oftalmología, Cardiología, Urgencias, etc.

**Por qué necesario**:
- Asignación inteligente de citas según especialidad
- Búsqueda de veterinario apropiado
- Perfil profesional

---

#### **EmpleadoEspecialidad** (Tabla de Unión)
```sql
Campos:
- EmpleadoId: Guid (FK Empleado, PK compuesta)
- EspecialidadId: Guid (FK Especialidad, PK compuesta)
- FechaCertificacion: DateTime?
- NumeroCertificado: string?
- Notas: string?
```

**Propósito**: Relación N:M entre empleados y especialidades.

**Por qué certificación**:
- Validación de credenciales
- Renovación de certificados
- Auditoría profesional

---

#### **Servicio** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Descripcion: string (requerido)
- DuracionMinDefault: int - duración típica en minutos
- PrecioSugerido: decimal?
- Activo: bool (default: true)
- Notas: string? (TEXT)
- Categoria: enum CategoriaServicio
```

**Propósito**: Catálogo de servicios veterinarios ofrecidos.

**Categorías**:
- Consulta, Cirugia, Diagnostico, Estetica, Vacunacion, Emergencia, Otro

**Por qué DuracionMinDefault**:
- Auto-cálculo de EndAt al crear citas
- Optimización de agenda
- Estimación de capacidad diaria

**Por qué PrecioSugerido**:
- Preparación para módulo de pagos
- Cotizaciones
- Reportes financieros

---

#### **Horario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- EmpleadoId: Guid (FK Empleado, requerido)
- DiaSemana: enum DayOfWeek (0-6)
- HoraInicio: TimeSpan
- HoraFin: TimeSpan
- Activo: bool (default: true)
```

**Propósito**: Definir horarios de trabajo de empleados.

**Por qué necesario**:
- Validación de disponibilidad al crear citas
- Generación automática de agenda
- Prevención de asignación fuera de horario

**Ejemplo**:
```
Veterinario A:
- Lunes: 08:00 - 14:00
- Miércoles: 08:00 - 14:00
- Viernes: 08:00 - 14:00
```

---

## Módulo de Auditoría

Registro de eventos críticos, jobs programados y eventos de outbox.

### Entidades

#### **AuditLog** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid? (FK Usuario)
- Entidad: string? - nombre de la entidad afectada
- EntidadId: string? - ID del registro afectado
- Accion: enum AccionAudit
- BeforeJson: string? (TEXT) - estado antes del cambio
- AfterJson: string? (TEXT) - estado después del cambio
- CreatedAt: DateTime (default: now)
- TraceId: string? - para correlación de logs
- IpAddress: string?
- UserAgent: string?
```

**Propósito**: Log de auditoría para acciones críticas del sistema.

**Acciones auditadas**:
- Create, Update, Delete, StateChange, Login, Logout, PasswordChange, RoleAssignment

**Por qué JSON completo**:
- Permite comparación exacta de cambios
- Recuperación de datos en caso de error
- Análisis forense

**Por qué TraceId**:
- Correlación de logs distribuidos
- Seguimiento de flujo completo de operación
- Debugging de problemas complejos

**Por qué IP y UserAgent**:
- Detección de accesos sospechosos
- Análisis de seguridad
- Cumplimiento regulatorio

---

#### **OutboxEvent** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- EventType: string (requerido)
- AggregateId: string (requerido)
- Payload: string (TEXT, requerido) - JSON
- CreatedAt: DateTime (default: now)
- ProcessedAt: DateTime?
- FailedAt: DateTime?
- RetryCount: int (default: 0)
- LastError: string? (TEXT)
```

**Propósito**: Patrón Outbox para mensajería confiable.

**Por qué patrón Outbox**:
- **Consistencia**: Garantiza que eventos se publiquen solo si la transacción principal tiene éxito
- **Resiliencia**: Reintentos automáticos en caso de fallo
- **Auditoría**: Registro completo de eventos publicados

**Flujo**:
1. Transacción guarda cambio en BD + OutboxEvent
2. Job en background lee OutboxEvents no procesados
3. Publica a message broker (RabbitMQ, Azure Service Bus, etc.)
4. Marca como procesado

---

#### **JobProgramado** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- JobName: string (requerido, único)
- CronExpression: string (requerido)
- Activo: bool (default: true)
- UltimaEjecucion: DateTime?
- ProximaEjecucion: DateTime?
- Estado: enum EstadoJob (Pendiente, Ejecutando, Completado, Fallido)
- Parametros: string? (JSON)
```

**Propósito**: Configuración y tracking de jobs programados (cron jobs).

**Jobs típicos**:
- Envío de recordatorios de citas
- Generación de alertas de inventario
- Limpieza de notificaciones antiguas
- Procesamiento de webhooks pendientes
- Reportes programados

**Por qué en BD**:
- Configuración dinámica sin redespliegue
- Monitoring centralizado
- Historial de ejecuciones

---

## Relaciones entre Módulos

### Diagrama de Dependencias

```
???????????????
?  Security   ????
?  (Usuario)  ?  ?
???????????????  ?
                 ????? ????????????
???????????????  ?     ? Mascotas ?
?  Servicios  ????     ?  (Core)  ?
? (Empleado)  ?        ????????????
???????????????             ?
       ?                    ?
       ?????????????????????????? ????????????????
       ?                    ?     ?   Clínica    ?
       ?                    ?     ?   (Citas)    ?
       ?                    ?     ????????????????
       ?                    ?            ?
       ?                    ?????????????????? ????????????????????
       ?                    ?            ?     ? Historial Médico ?
       ?                    ?            ?     ?  (Expedientes)   ?
       ?                    ?            ?     ????????????????????
       ?                    ?            ?
       ?                    ?            ????? ????????????????
       ?                    ?                  ?  Inventario  ?
       ?                    ?                  ?   (FIFO)     ?
       ?                    ?                  ????????????????
       ?                    ?
       ?????????????????????????? ????????????????
                            ?     ?  Donaciones  ?
                            ?     ?   (PayPal)   ?
                            ?     ????????????????
                            ?
                            ????? ????????????????
                                  ?  Auditoría   ?
                                  ?   (Logs)     ?
                                  ????????????????
```

### Relaciones Clave

#### 1. **Usuario ? Múltiples Roles**
Un usuario puede ser simultáneamente adoptante, veterinario y donante.
```
Usuario (Juan Pérez)
?? Rol: Adoptante ? puede solicitar adopciones
?? Rol: Veterinario ? puede atender citas
?? Empleado ? tiene horarios y especialidades
```

#### 2. **Mascota ? Historial Completo**
Cada mascota tiene registro detallado de su historia.
```
Mascota (Firulais)
?? Expedientes médicos
?? Vacunaciones
?? Cirugías
?? Desparasitaciones
?? Valoraciones
?? Fotos
?? Solicitudes de adopción
```

#### 3. **Cita ? Múltiples Entidades**
Una cita conecta varios módulos.
```
Cita
?? Mascota (paciente)
?? Usuario (propietario si es externo)
?? Usuario (veterinario)
?? Sala (espacio físico)
?? Recordatorios
?? Expediente (resultado de la consulta)
?? ReporteUsoInsumos (insumos usados)
```

#### 4. **ReporteUsoInsumos ? Descuento FIFO**
Proceso complejo de descuento automático.
```
ReporteUsoInsumos
?? Cita (procedimiento origen)
?? Detalles (items usados)
?   ?? ReporteUsoSplitLote
?       ?? Lote A (30ml, vence primero)
?       ?? Lote B (20ml, vence después)
?? MovimientosInventario (salidas generadas)
```

---

## Patrones de Diseño Aplicados

### 1. **Domain-Driven Design (DDD)**

**Agregados**: Entidades principales que aseguran consistencia.
- **Usuario** es agregado de Dispositivos, Notificaciones
- **Mascota** es agregado de Fotos, Solicitudes
- **Compra** es agregado de DetalleCompra

**Value Objects**: Enumeraciones fuertemente tipadas.
- EstatusUsuario, TipoCita, StatusDonacion, etc.

### 2. **Repository Pattern**

Separación de lógica de acceso a datos mediante interfaces:
```
IUsuarioRepository
IMascotaRepository
IInventarioRepository
```

### 3. **Soft Delete Pattern**

Entidades críticas heredan de `SoftDeletableEntity`:
- Mascota (preservar historial de fallecidas)
- Sala (preservar historial de citas pasadas)

### 4. **Audit Pattern**

Todas las entidades importantes heredan de `AuditableEntity`:
- Timestamps automáticos en SaveChanges
- Tracking de usuario creador/modificador

### 5. **Outbox Pattern**

Para mensajería confiable:
- Eventos se guardan en OutboxEvent en misma transacción
- Job en background procesa y publica eventos
- Garantía de entrega eventual

### 6. **FIFO (First In, First Out)**

Para inventario de medicamentos:
- Lotes se ordenan por fecha de expiración
- Descuento automático del lote más antiguo
- ReporteUsoSplitLote registra exactamente qué lotes se usaron

### 7. **Idempotency Pattern**

Para operaciones críticas:
- **ClientUsageId** en ReporteUsoInsumos
- **EventId** único en WebhookEvent
- Previene duplicados por reintentos de red

### 8. **State Machine Pattern**

Entidades con flujo de estados bien definido:
- **SolicitudAdopcion**: Pendiente ? EnRevision ? Aprobada/Rechazada
- **Cita**: Programada ? EnProceso ? Completada/Cancelada
- **Compra**: Pendiente ? Confirmada ? Recibida

### 9. **Logging & Traceability**

Múltiples niveles de auditoría:
- **AdopcionLog**: Cambios de estado de solicitudes
- **CitaHistorialEstado**: Cambios de estado de citas
- **AuditLog**: Cambios críticos en cualquier entidad
- **MovimientoInventario**: Cada movimiento de stock

---

## Consideraciones de Seguridad

### 1. **Autenticación**
- Passwords hasheados con salt único por usuario
- Tokens JWT para sesiones stateless
- Refresh tokens para renovación segura

### 2. **Autorización**
- RBAC (Role-Based Access Control)
- Un usuario puede tener múltiples roles
- Permisos granulares por rol

### 3. **Privacidad**
- Soft delete preserva datos pero los oculta
- Donaciones anónimas respetan privacidad
- Consentimiento explícito de políticas con versioning

### 4. **Auditoría**
- Registro de IP y UserAgent en acciones críticas
- TraceId para correlación de operaciones
- Before/After JSON para análisis forense

### 5. **Integridad**
- FK con eliminación restringida previene huérfanos
- Transacciones garantizan consistencia
- Validaciones en capa de dominio

---

## Optimizaciones de Performance

### 1. **Índices**
Configuraciones de EntityFramework crean índices en:
- Email de Usuario (búsquedas frecuentes)
- PaypalOrderId (webhook lookups)
- Fechas de citas (queries de agenda)
- ExpDate de lotes (consultas FIFO)

### 2. **Query Filters**
Filtro global para soft delete:
```csharp
modelBuilder.Entity<Mascota>().HasQueryFilter(m => m.DeletedAt == null);
```
Automáticamente excluye eliminadas en todas las queries.

### 3. **Lazy Loading Deshabilitado**
Previene N+1 queries, requiere explicit `Include()`.

### 4. **Precision de Decimales**
Configuración global: `decimal(18,2)` para cantidades monetarias.

### 5. **JSON Columns**
Campos como `DataJson` en Notificacion permiten metadatos flexibles sin esquema rígido.

---

## Escalabilidad Futura

### Módulos Planeados

1. **Pagos**
   - Integración con múltiples pasarelas
   - Facturación electrónica
   - Subscripciones de padrinos de mascotas

2. **Reportes**
   - Dashboard de estadísticas
   - Exportación de reportes
   - Business Intelligence

3. **Comunicación**
   - Chat interno
   - Emails transaccionales
   - SMS para recordatorios

4. **Telemetría**
   - Sensores IoT (comederos automáticos, cámaras)
   - Tracking de salud en tiempo real

5. **Adopciones Virtuales**
   - Videollamadas de adopción
   - Tours virtuales del refugio
   - Sistema de matching automático

---

## Conclusión

La base de datos de **AdoPets** está diseñada para ser:
- **Escalable**: Arquitectura modular permite añadir funcionalidades sin afectar existentes
- **Auditable**: Trazabilidad completa de todas las operaciones críticas
- **Flexible**: Soft delete y JSON columns permiten evolución sin migraciones complejas
- **Segura**: Múltiples capas de validación y auditoría
- **Eficiente**: FIFO automático, índices optimizados, query filters

El diseño refleja las necesidades reales de un refugio moderno:
- Gestión de adopciones con proceso de evaluación
- Clínica veterinaria completa con historial médico
- Control de inventario médico con trazabilidad total
- Sistema de donaciones con integración de pagos
- Administración de personal y recursos

Cada decisión de diseño está justificada por requisitos funcionales, técnicos o regulatorios específicos.
