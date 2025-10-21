# Documentaci�n de la Base de Datos - AdoPets Backend

## �ndice
1. [Visi�n General](#visi�n-general)
2. [Arquitectura de Entidades](#arquitectura-de-entidades)
3. [M�dulo de Seguridad](#m�dulo-de-seguridad)
4. [M�dulo de Mascotas](#m�dulo-de-mascotas)
5. [M�dulo de Cl�nica](#m�dulo-de-cl�nica)
6. [M�dulo de Historial Cl�nico](#m�dulo-de-historial-cl�nico)
7. [M�dulo de Inventario](#m�dulo-de-inventario)
8. [M�dulo de Donaciones](#m�dulo-de-donaciones)
9. [M�dulo de Servicios](#m�dulo-de-servicios)
10. [M�dulo de Auditor�a](#m�dulo-de-auditor�a)
11. [Relaciones entre M�dulos](#relaciones-entre-m�dulos)
12. [Patrones de Dise�o Aplicados](#patrones-de-dise�o-aplicados)

---

## Visi�n General

**AdoPets** es un sistema integral para la gesti�n de refugios de animales que combina funcionalidades de:
- Gesti�n de mascotas y adopciones
- Cl�nica veterinaria con historial m�dico completo
- Control de inventario m�dico con sistema FIFO
- Gesti�n de donaciones mediante PayPal
- Administraci�n de personal y servicios
- Auditor�a y trazabilidad completa

### Filosof�a de Dise�o

La base de datos est� construida siguiendo principios de **Domain-Driven Design (DDD)** con:
- Separaci�n clara de dominios mediante m�dulos
- Entidades base con auditor�a autom�tica
- Soft delete para datos sensibles
- Trazabilidad completa de cambios cr�ticos
- Normalizaci�n apropiada para evitar redundancia

---

## Arquitectura de Entidades

### Jerarqu�a de Entidades Base

La arquitectura utiliza tres clases base abstractas que proporcionan funcionalidad com�n:

#### 1. **BaseEntity**
```
- Id: Guid (PK)
```
**Prop�sito**: Entidad base m�nima que proporciona identificador �nico usando GUID para facilitar la distribuci�n y evitar colisiones.

**Por qu� GUID**: 
- Permite generaci�n de IDs en el cliente sin conflictos
- Facilita replicaci�n y sincronizaci�n entre sistemas
- Mayor seguridad al no exponer secuencias predecibles

#### 2. **AuditableEntity** (hereda de BaseEntity)
```
- CreatedAt: DateTime (UTC)
- UpdatedAt: DateTime? (UTC)
- CreatedBy: Guid? (FK Usuario)
- UpdatedBy: Guid? (FK Usuario)
```
**Prop�sito**: A�ade campos de auditor�a temporal y de autor�a para rastrear cu�ndo y qui�n cre� o modific� un registro.

**Por qu� auditor�a autom�tica**:
- Cumplimiento con requisitos legales y normativos
- Trazabilidad para resoluci�n de disputas
- An�lisis de comportamiento de usuarios
- Debugging y soporte t�cnico

#### 3. **SoftDeletableEntity** (hereda de AuditableEntity)
```
- DeletedAt: DateTime? (UTC)
- DeletedBy: Guid? (FK Usuario)
- IsDeleted: bool (computed)
```
**Prop�sito**: Implementa eliminaci�n l�gica en lugar de f�sica, preservando datos hist�ricos.

**Por qu� soft delete**:
- Recuperaci�n de datos eliminados accidentalmente
- Cumplimiento con regulaciones de retenci�n de datos
- Integridad referencial sin cascadas destructivas
- An�lisis hist�rico completo

---

## M�dulo de Seguridad

Este m�dulo maneja la autenticaci�n, autorizaci�n, notificaciones y gesti�n de dispositivos.

### Entidades

#### **Usuario** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido)
- ApellidoPaterno: string (requerido)
- ApellidoMaterno: string (requerido)
- Email: string (requerido, �nico)
- Telefono: string?
- PasswordHash: string (requerido)
- PasswordSalt: string (requerido)
- Estatus: enum EstatusUsuario (default: Activo)
- UltimoAccesoAt: DateTime?
- AceptoPoliticasVersion: string?
- AceptoPoliticasAt: DateTime?
```

**Prop�sito**: Representa cualquier usuario del sistema (adoptantes, veterinarios, administradores, etc.)

**Por qu� estos campos**:
- **Nombre completo separado**: Facilita b�squedas y ordenamiento por apellido
- **PasswordHash/Salt**: Almacenamiento seguro usando hashing + salt �nico por usuario
- **Estatus enum**: Control de acceso sin eliminar usuarios (suspensi�n, bloqueo)
- **UltimoAccesoAt**: Detecci�n de cuentas inactivas, an�lisis de uso
- **AceptoPoliticasVersion**: Cumplimiento GDPR/LFPDPPP - requiere re-aceptaci�n al cambiar pol�ticas

**Relaciones**:
- 1:N con UsuarioRol (un usuario puede tener m�ltiples roles)
- 1:N con Dispositivo (un usuario puede tener m�ltiples dispositivos registrados)
- 1:N con Notificacion (un usuario recibe m�ltiples notificaciones)
- 1:N con ConsentimientoPolitica (historial de aceptaciones)
- 1:N con SolicitudAdopcion (un usuario puede solicitar adoptar m�ltiples mascotas)

---

#### **Rol** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, �nico)
- Descripcion: string?
```

**Prop�sito**: Define roles del sistema para autorizaci�n basada en roles (RBAC).

**Roles predefinidos**:
- **Admin**: Control total del sistema
- **Veterinario**: Gesti�n cl�nica y expedientes m�dicos
- **Recepcionista**: Gesti�n de citas y atenci�n al p�blico
- **Asistente**: Apoyo en procedimientos veterinarios
- **Adoptante**: Usuario que busca adoptar mascotas

**Por qu� este dise�o**:
- Separaci�n de concerns entre autenticaci�n y autorizaci�n
- Flexibilidad para a�adir nuevos roles sin cambiar c�digo
- Un usuario puede tener m�ltiples roles simult�neamente

**Relaciones**:
- N:M con Usuario a trav�s de UsuarioRol

---

#### **UsuarioRol** (Tabla de Uni�n)
```sql
Campos:
- UsuarioId: Guid (FK Usuario, PK compuesta)
- RolId: Guid (FK Rol, PK compuesta)
- AsignadoAt: DateTime (default: now)
- AsignadoPor: Guid? (FK Usuario)
```

**Prop�sito**: Implementa la relaci�n N:M entre Usuario y Rol con auditor�a.

**Por qu� campos adicionales**:
- **AsignadoAt**: Rastrear cu�ndo se otorg� el rol
- **AsignadoPor**: Accountability - qui�n otorg� permisos

---

#### **Dispositivo** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario, requerido)
- DeviceToken: string (requerido, �nico)
- Platform: enum (iOS, Android, Web)
- DeviceInfo: string?
- Activo: bool (default: true)
- UltimaConexion: DateTime?
```

**Prop�sito**: Gesti�n de dispositivos para notificaciones push y seguridad.

**Por qu� es necesario**:
- Env�o de notificaciones push espec�ficas por dispositivo
- Seguridad: revocar acceso a dispositivos comprometidos
- An�lisis de uso multi-dispositivo

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

**Prop�sito**: Sistema de notificaciones in-app y push.

**Tipos de notificaci�n**:
- AdoptionStatusChange: Cambios en solicitud de adopci�n
- AppointmentReminder: Recordatorios de citas
- InventoryLow: Alertas de stock bajo
- InventoryExpiring: Insumos pr�ximos a vencer
- DonationReceived: Confirmaci�n de donaci�n
- System: Notificaciones generales del sistema

**Por qu� tres timestamps**:
- **Fecha**: Cu�ndo se cre� la notificaci�n
- **DeliveredAt**: Cu�ndo lleg� al dispositivo (tracking de entrega)
- **ReadAt**: Cu�ndo el usuario la ley� (engagement)

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

**Prop�sito**: Permitir a usuarios controlar qu� notificaciones reciben y por qu� canal.

**Por qu� es importante**:
- Cumplimiento con regulaciones anti-spam
- Mejor experiencia de usuario
- Reducci�n de tasa de desinstalaci�n

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

**Prop�sito**: Historial de aceptaciones de pol�ticas para cumplimiento legal.

**Por qu� IP y UserAgent**:
- Evidencia legal de consentimiento informado
- Prevenci�n de fraude
- Cumplimiento GDPR/LFPDPPP

---

## M�dulo de Mascotas

Gesti�n completa de mascotas del refugio y proceso de adopci�n.

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
- Personalidad: string? (TEXT) - descripci�n temperamento
- EstadoSalud: string? (TEXT) - condici�n general
- RequisitoAdopcion: string? (TEXT) - requisitos especiales
- Origen: string? - c�mo lleg� al refugio
- Notas: string? (TEXT) - informaci�n adicional
```

**Prop�sito**: Entidad central que representa cada mascota del refugio.

**Estatus posibles**:
- **Disponible**: Lista para adopci�n
- **Reservada**: Proceso de adopci�n en curso
- **Adoptada**: Ya fue adoptada
- **NoAdoptable**: No apta para adopci�n (agresividad, enfermedad cr�nica)
- **EnTratamiento**: Bajo cuidados m�dicos intensivos

**Por qu� soft delete**:
- Preservar historial de mascotas fallecidas
- Mantener integridad de expedientes m�dicos
- An�lisis estad�stico completo del refugio

**Por qu� campos de texto libre**:
- **Personalidad**: Cada mascota es �nica, texto libre permite descripciones detalladas
- **RequisitoAdopcion**: Algunos animales necesitan condiciones especiales (casa con jard�n, sin ni�os, etc.)
- **Origen**: Informaci�n de contexto importante (rescate, abandono, entregado por due�o)

**Relaciones**:
- 1:N con MascotaFoto (galer�a de im�genes)
- 1:N con SolicitudAdopcion (m�ltiples personas pueden solicitarla)
- 1:N con Expediente (historial m�dico completo)
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

**Prop�sito**: Gesti�n de galer�a de fotos para cada mascota.

**Por qu� separado**:
- M�ltiples fotos por mascota
- Optimizaci�n de consultas (no cargar todas las fotos siempre)
- IsPrincipal marca la foto de portada
- Facilita integraci�n con CDN/storage externo

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
- HorasDisponibilidad: int - horas al d�a disponibles
- Direccion: string (requerido)
- IngresosMensuales: decimal?
- MotivoAdopcion: string? (TEXT)
- MotivoRechazo: string? (TEXT)
- FechaSolicitud: DateTime (default: now)
- FechaRevision: DateTime?
- FechaAprobacion: DateTime?
- RevisadoPor: Guid? (FK Usuario)
```

**Prop�sito**: Formulario de solicitud y proceso de adopci�n.

**Estados del proceso**:
1. **Pendiente**: Solicitud enviada, esperando revisi�n
2. **EnRevision**: Un empleado est� evaluando la solicitud
3. **Aprobada**: Adoptante puede proceder con la adopci�n
4. **Rechazada**: No cumple requisitos
5. **Cancelada**: Adoptante desisti�

**Por qu� estos campos de evaluaci�n**:
- **Vivienda**: Determinar si el espacio es adecuado
- **NumNinios**: Evaluar ambiente familiar (algunas mascotas no toleran ni�os)
- **OtrasMascotas**: Compatibilidad con otros animales
- **HorasDisponibilidad**: Asegurar que tendr� tiempo suficiente
- **IngresosMensuales**: Capacidad econ�mica para mantener la mascota
- **MotivoAdopcion**: Detectar adopciones impulsivas o motivos inadecuados

**Por qu� m�ltiples timestamps**:
- An�lisis de tiempos de respuesta del refugio
- SLA (Service Level Agreement) interno
- M�tricas de eficiencia operacional

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

**Prop�sito**: Documentaci�n soporte de la solicitud de adopci�n.

**Por qu� necesario**:
- Verificaci�n de identidad
- Validaci�n de direcci�n
- Comprobaci�n de capacidad econ�mica
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

**Prop�sito**: Trazabilidad completa de cambios de estado en adopciones.

**Por qu� es cr�tico**:
- Auditor�a de decisiones
- Resoluci�n de disputas
- Detecci�n de patrones (ej: mismo usuario con m�ltiples rechazos)
- An�lisis de proceso de adopci�n

---

## M�dulo de Cl�nica

Gesti�n de citas veterinarias, salas y recordatorios.

### Entidades

#### **Cita** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid? (FK Mascota) - nullable para mascotas externas
- PropietarioId: Guid? (FK Usuario) - due�o de mascota externa
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
- PagoId: Guid? - reservado para m�dulo de pagos futuro
```

**Prop�sito**: Agenda veterinaria con prevenci�n de conflictos de horario.

**Tipos de cita**:
- Consulta, Cirugia, Ba�o, Vacuna, Procedimiento, Urgencia, Seguimiento

**Por qu� MascotaId nullable**:
- Permite atender mascotas que no pertenecen al refugio
- Servicio veterinario externo como fuente de ingresos
- PropietarioId identifica al due�o externo

**Por qu� StartAt y EndAt**:
- Prevenci�n de solapamiento de citas
- C�lculo autom�tico de disponibilidad
- Optimizaci�n de uso de salas

**Relaciones**:
- N:1 con Mascota (opcional)
- N:1 con Usuario (propietario opcional)
- N:1 con Usuario (veterinario obligatorio)
- N:1 con Sala (opcional)
- 1:N con CitaRecordatorio (recordatorios autom�ticos)
- 1:N con CitaHistorialEstado (trazabilidad)
- 1:1 con ReporteUsoInsumos (insumos usados en la cita)

---

#### **Sala** (SoftDeletableEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, �nico)
- Descripcion: string?
- Capacidad: int (default: 1)
- Activa: bool (default: true)
```

**Prop�sito**: Gesti�n de espacios f�sicos para citas.

**Por qu� necesario**:
- Prevenci�n de doble reserva de sala
- Optimizaci�n de recursos f�sicos
- Capacidad permite salas compartidas (ej: �rea de vacunaci�n)

---

#### **CitaRecordatorio** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- CitaId: Guid (FK Cita, requerido)
- TipoRecordatorio: enum (Email, Push, SMS)
- MinutosAntes: int - cu�ndo enviar antes de la cita
- Enviado: bool (default: false)
- EnviadoAt: DateTime?
- Error: string?
```

**Prop�sito**: Sistema automatizado de recordatorios de citas.

**Por qu� m�ltiples recordatorios**:
- Reducci�n de no-shows
- M�ltiples canales aumentan efectividad
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

**Prop�sito**: Auditor�a de cambios de estado de citas.

**Por qu� importante**:
- Detectar cancelaciones frecuentes
- M�tricas de cumplimiento de citas
- Responsabilidad por cambios

---

## M�dulo de Historial Cl�nico

Expediente m�dico completo de cada mascota.

### Entidades

#### **Expediente** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- VeterinarioId: Guid (FK Usuario, requerido)
- CitaId: Guid? (FK Cita) - si deriva de una cita
- MotivoConsulta: string? (TEXT)
- Anamnesis: string? (TEXT) - historia y s�ntomas
- Diagnostico: string (TEXT, requerido)
- Tratamiento: string? (TEXT)
- Notas: string? (TEXT)
- Pronostico: string? (TEXT)
- Fecha: DateTime (default: now)
```

**Prop�sito**: Nota cl�nica o expediente m�dico de una consulta.

**Por qu� estructura SOAP**:
- **MotivoConsulta**: Raz�n de la visita
- **Anamnesis**: Historia y s�ntomas (Subjective)
- **Diagnostico**: Evaluaci�n m�dica (Assessment)
- **Tratamiento**: Plan de acci�n (Plan)
- **Pronostico**: Expectativas de recuperaci�n

**Relaciones**:
- N:1 con Mascota (m�ltiples consultas por mascota)
- 1:N con AdjuntoMedico (rayos X, an�lisis, etc.)

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

**Prop�sito**: Almacenar im�genes m�dicas y documentos relacionados.

**Por qu� separado**:
- Optimizaci�n: no cargar archivos pesados siempre
- Permite m�ltiples archivos por expediente
- Integraci�n con PACS (Picture Archiving and Communication System) futuro

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
- NextDueAt: DateTime? - cu�ndo toca refuerzo
- VeterinarioId: Guid (FK Usuario, requerido)
- Notes: string? (TEXT)
- ReaccionAdversa: string? (TEXT)
```

**Prop�sito**: Historial de vacunaci�n completo.

**Por qu� Lot importante**:
- Retiros de lotes defectuosos
- Trazabilidad regulatoria
- Investigaci�n de reacciones adversas

**Por qu� NextDueAt**:
- Recordatorios autom�ticos
- Seguimiento de plan de vacunaci�n
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
- Peso: decimal? - peso al momento de aplicaci�n (dosificaci�n)
- Notes: string?
```

**Prop�sito**: Control de desparasitaciones peri�dicas.

**Por qu� peso**:
- Dosificaci�n correcta depende del peso
- Tracking de crecimiento de cachorros
- Ajuste de tratamiento

---

#### **Cirugia** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- MascotaId: Guid (FK Mascota, requerido)
- Tipo: string (requerido) - esterilizaci�n, fractura, tumor, etc.
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

**Prop�sito**: Registro detallado de procedimientos quir�rgicos.

**Por qu� tan detallado**:
- Referencia para cirug�as futuras
- Informaci�n cr�tica para otros veterinarios
- An�lisis de complicaciones
- Est�ndar m�dico veterinario

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

**Prop�sito**: Signos vitales y evaluaci�n f�sica rutinaria.

**Por qu� separado de Expediente**:
- Tomas r�pidas sin consulta completa
- Seguimiento de evoluci�n de peso
- Detecci�n temprana de anomal�as
- Estad�sticas de salud general

---

## M�dulo de Inventario

Sistema completo de gesti�n de inventario m�dico con control FIFO (First In, First Out).

### Entidades

#### **ItemInventario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, �nico)
- Unidad: string (requerido) - pz, ml, mg, kg, etc.
- Categoria: enum CategoriaInventario
- MinQty: decimal - stock m�nimo para alertas
- Activo: bool (default: true)
- Notas: string? (TEXT)
- Descripcion: string? (TEXT)
```

**Prop�sito**: Cat�logo maestro de insumos m�dicos.

**Categor�as**:
- Medicamento, Vacuna, Alimento, MaterialCuracion, Quirurgico, Limpieza, Otro

**Por qu� MinQty**:
- Generaci�n autom�tica de alertas de reabastecimiento
- Prevenci�n de desabasto
- Optimizaci�n de compras

**Relaciones**:
- 1:N con LoteInventario (un item tiene m�ltiples lotes)
- 1:N con MovimientoInventario (historial de entradas/salidas)
- 1:N con AlertaInventario (alertas activas)

---

#### **LoteInventario** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ItemId: Guid (FK ItemInventario, requerido)
- Lote: string (requerido)
- ExpDate: DateTime? - fecha de expiraci�n
- QtyDisponible: decimal (requerido)
- QtyInicial: decimal (requerido)
- CreatedAt: DateTime (default: now)
- Notas: string?
```

**Prop�sito**: Control de lotes individuales con fechas de vencimiento.

**Por qu� sistema de lotes**:
- **Trazabilidad**: Retiros de lotes defectuosos
- **FIFO**: Usar primero los que vencen primero
- **Auditor�a**: Saber exactamente de d�nde vino cada medicamento usado
- **Regulatorio**: Requisito legal para medicamentos controlados

**Por qu� QtyInicial y QtyDisponible separados**:
- QtyInicial: referencia hist�rica
- QtyDisponible: stock actual
- Permite calcular consumo hist�rico del lote

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

**Prop�sito**: Registro de cada movimiento de inventario (entrada/salida).

**Por qu� tipos de movimiento**:
- **Entrada**: Compra nueva
- **Salida**: Uso en procedimientos
- **Ajuste**: Correcci�n de conteos
- **Vencimiento**: Desecho por expiraci�n
- **Perdida**: Robo, da�o, etc.

**Por qu� Referencia polim�rfica**:
- ReferenciaId + ReferenciaType permiten vincular a diferentes entidades
- Flexibilidad sin crear m�ltiples FK opcionales
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

**Prop�sito**: Sistema de alertas autom�ticas de inventario.

**Por qu� necesario**:
- Notificaciones proactivas
- Prevenci�n de desabasto
- Reducci�n de desperdicio por vencimiento
- Dashboard de administraci�n

---

#### **Proveedor** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, �nico)
- Contacto: string?
- Telefono: string?
- Email: string?
- Direccion: string? (TEXT)
- Activo: bool (default: true)
- Notas: string? (TEXT)
```

**Prop�sito**: Cat�logo de proveedores de insumos.

**Por qu� necesario**:
- Gesti�n de compras
- Evaluaci�n de proveedores
- Informaci�n de contacto centralizada

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

**Prop�sito**: Registro de �rdenes de compra a proveedores.

**Estados de compra**:
- **Pendiente**: Orden creada, esperando entrega
- **Confirmada**: Proveedor confirm�
- **Recibida**: Insumos recibidos completamente
- **Cancelada**: Orden cancelada
- **Parcial**: Recepci�n parcial

**Por qu� FechaRecepcion separada**:
- Diferencia entre fecha de compra y recepci�n real
- C�lculo de tiempos de entrega de proveedores
- Auditor�a

**Relaciones**:
- N:1 con Proveedor
- 1:N con DetalleCompra (items espec�ficos de la compra)

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

**Prop�sito**: Detalle l�nea por l�nea de una compra.

**Por qu� separado**:
- Una compra contiene m�ltiples items
- Normalizaci�n
- C�lculo autom�tico de totales
- Creaci�n autom�tica de lotes al recibir

---

#### **ReporteUsoInsumos** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- CitaId: Guid (FK Cita, requerido, �nico)
- VetId: Guid (FK Usuario, requerido)
- Status: enum StatusReporte (Registrado, Revertido)
- SubmittedAt: DateTime (default: now)
- Notes: string? (TEXT)
- ClientUsageId: string? (�nico) - para idempotencia
- RevertedAt: DateTime?
- RevertedBy: Guid? (FK Usuario)
- RevertReason: string? (TEXT)
```

**Prop�sito**: Reporte post-procedimiento de insumos usados con descuento autom�tico FIFO.

**Por qu� necesario**:
- Registro preciso de consumo
- Actualizaci�n autom�tica de inventario
- Costeo de procedimientos
- Auditor�a de uso

**Por qu� ClientUsageId**:
- **Idempotencia**: Prevenir duplicados si se reenv�a la petici�n
- Seguridad contra errores de red

**Por qu� reversible**:
- Correcci�n de errores
- Cancelaci�n de procedimientos
- Auditor�a completa (no delete)

**Relaciones**:
- 1:1 con Cita (un reporte por cita)
- 1:N con ReporteUsoInsumoDetalle (items espec�ficos usados)

---

#### **ReporteUsoInsumoDetalle** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ReporteId: Guid (FK ReporteUsoInsumos, requerido)
- ItemId: Guid (FK ItemInventario, requerido)
- QtyUsada: decimal (requerido)
```

**Prop�sito**: Detalle de insumos usados en un procedimiento.

**Por qu� no tiene LoteId directo**:
- El sistema autom�ticamente selecciona lotes con FIFO
- Puede usar m�ltiples lotes (si uno no tiene suficiente stock)

**Relaciones**:
- N:1 con ReporteUsoInsumos
- 1:N con ReporteUsoSplitLote (muestra qu� lotes se usaron)

---

#### **ReporteUsoSplitLote** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- ReporteDetalleId: Guid (FK ReporteUsoInsumoDetalle, requerido)
- LoteId: Guid (FK LoteInventario, requerido)
- QtyUsada: decimal (requerido)
```

**Prop�sito**: Registro exacto de qu� lotes se usaron y en qu� cantidad.

**Por qu� tabla adicional**:
- **Trazabilidad total**: Saber exactamente qu� lote de medicamento recibi� cada mascota
- **FIFO multi-lote**: Si un lote no tiene suficiente, se completa con el siguiente
- **Auditor�a**: Retiros de lotes o investigaci�n de reacciones adversas

**Ejemplo**:
```
Procedimiento necesita 50ml de medicamento X
- Lote A (vence antes) tiene 30ml ? usa 30ml
- Lote B (vence despu�s) tiene 100ml ? usa 20ml
Resultado: 2 registros en ReporteUsoSplitLote
```

---

## M�dulo de Donaciones

Gesti�n de donaciones mediante integraci�n con PayPal.

### Entidades

#### **Donacion** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid? (FK Usuario) - nullable para donantes an�nimos
- Monto: decimal (requerido)
- Moneda: string (default: "MXN")
- Status: enum StatusDonacion (default: PENDING)
- PaypalOrderId: string (requerido, �nico)
- PaypalCaptureId: string? - ID de captura de PayPal
- PayerEmail: string?
- PayerName: string?
- PaypalPayerId: string?
- Source: enum SourceDonacion (Checkout, Webhook, Manual)
- Mensaje: string? (TEXT) - mensaje del donante
- Anonima: bool - ocultar en listados p�blicos
- CapturedAt: DateTime?
- CancelledAt: DateTime?
- CancellationReason: string? (TEXT)
```

**Prop�sito**: Registro de donaciones monetarias mediante PayPal.

**Estados de donaci�n**:
- **PENDING**: Orden creada, esperando pago
- **CAPTURED**: Pago capturado exitosamente
- **CANCELLED**: Donante cancel�
- **FAILED**: Pago fall�
- **REFUNDED**: Devoluci�n procesada

**Por qu� m�ltiples IDs de PayPal**:
- **PaypalOrderId**: ID de la orden (creada antes del pago)
- **PaypalCaptureId**: ID de la captura (confirmaci�n del pago)
- **PaypalPayerId**: ID del pagador en PayPal
- Necesario para reconciliaci�n y soporte

**Por qu� Source**:
- **Checkout**: Donaci�n desde el sitio web
- **Webhook**: Procesada por webhook de PayPal
- **Manual**: Ingresada manualmente por admin

**Por qu� Anonima**:
- Respeto a preferencias de privacidad
- Muro de donaciones p�blico puede ocultar nombres

---

#### **WebhookEvent** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- EventType: string (requerido)
- EventId: string (requerido, �nico) - ID de PayPal
- Payload: string (TEXT, requerido) - JSON completo del webhook
- ProcessedAt: DateTime?
- Success: bool?
- ErrorMessage: string? (TEXT)
- CreatedAt: DateTime (default: now)
```

**Prop�sito**: Log de webhooks de PayPal para debugging e idempotencia.

**Por qu� necesario**:
- **Idempotencia**: Evitar procesar el mismo evento dos veces
- **Debugging**: Ver payload exacto recibido
- **Auditor�a**: Seguimiento de eventos de PayPal
- **Re-procesamiento**: Reintentar eventos fallidos

---

## M�dulo de Servicios

Gesti�n de empleados, especialidades, servicios y horarios.

### Entidades

#### **Empleado** (AuditableEntity)
```sql
Campos:
- Id: Guid (PK)
- UsuarioId: Guid (FK Usuario, requerido, �nico)
- Cedula: string? - c�dula profesional (veterinarios)
- Disponibilidad: string? (TEXT)
- EmailLaboral: string?
- TelefonoLaboral: string?
- Tipo: enum TipoEmpleado
- Sueldo: decimal?
- FechaContratacion: DateTime?
- FechaBaja: DateTime?
- Activo: bool (default: true)
```

**Prop�sito**: Informaci�n extendida de empleados del refugio.

**Tipos de empleado**:
- Veterinario, Asistente, Recepcionista, Administrador, Voluntario

**Por qu� separado de Usuario**:
- No todos los usuarios son empleados (adoptantes, donantes)
- Informaci�n laboral espec�fica
- Relaci�n 1:1 opcional con Usuario

**Por qu� Cedula**:
- Requisito legal para veterinarios
- Validaci�n de credenciales profesionales

**Relaciones**:
- 1:1 con Usuario
- N:M con Especialidad a trav�s de EmpleadoEspecialidad
- 1:N con Horario (turnos de trabajo)

---

#### **Especialidad** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Nombre: string (requerido, �nico)
- Descripcion: string? (TEXT)
```

**Prop�sito**: Cat�logo de especialidades veterinarias.

**Ejemplos**:
- Cirug�a, Dermatolog�a, Oftalmolog�a, Cardiolog�a, Urgencias, etc.

**Por qu� necesario**:
- Asignaci�n inteligente de citas seg�n especialidad
- B�squeda de veterinario apropiado
- Perfil profesional

---

#### **EmpleadoEspecialidad** (Tabla de Uni�n)
```sql
Campos:
- EmpleadoId: Guid (FK Empleado, PK compuesta)
- EspecialidadId: Guid (FK Especialidad, PK compuesta)
- FechaCertificacion: DateTime?
- NumeroCertificado: string?
- Notas: string?
```

**Prop�sito**: Relaci�n N:M entre empleados y especialidades.

**Por qu� certificaci�n**:
- Validaci�n de credenciales
- Renovaci�n de certificados
- Auditor�a profesional

---

#### **Servicio** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- Descripcion: string (requerido)
- DuracionMinDefault: int - duraci�n t�pica en minutos
- PrecioSugerido: decimal?
- Activo: bool (default: true)
- Notas: string? (TEXT)
- Categoria: enum CategoriaServicio
```

**Prop�sito**: Cat�logo de servicios veterinarios ofrecidos.

**Categor�as**:
- Consulta, Cirugia, Diagnostico, Estetica, Vacunacion, Emergencia, Otro

**Por qu� DuracionMinDefault**:
- Auto-c�lculo de EndAt al crear citas
- Optimizaci�n de agenda
- Estimaci�n de capacidad diaria

**Por qu� PrecioSugerido**:
- Preparaci�n para m�dulo de pagos
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

**Prop�sito**: Definir horarios de trabajo de empleados.

**Por qu� necesario**:
- Validaci�n de disponibilidad al crear citas
- Generaci�n autom�tica de agenda
- Prevenci�n de asignaci�n fuera de horario

**Ejemplo**:
```
Veterinario A:
- Lunes: 08:00 - 14:00
- Mi�rcoles: 08:00 - 14:00
- Viernes: 08:00 - 14:00
```

---

## M�dulo de Auditor�a

Registro de eventos cr�ticos, jobs programados y eventos de outbox.

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
- AfterJson: string? (TEXT) - estado despu�s del cambio
- CreatedAt: DateTime (default: now)
- TraceId: string? - para correlaci�n de logs
- IpAddress: string?
- UserAgent: string?
```

**Prop�sito**: Log de auditor�a para acciones cr�ticas del sistema.

**Acciones auditadas**:
- Create, Update, Delete, StateChange, Login, Logout, PasswordChange, RoleAssignment

**Por qu� JSON completo**:
- Permite comparaci�n exacta de cambios
- Recuperaci�n de datos en caso de error
- An�lisis forense

**Por qu� TraceId**:
- Correlaci�n de logs distribuidos
- Seguimiento de flujo completo de operaci�n
- Debugging de problemas complejos

**Por qu� IP y UserAgent**:
- Detecci�n de accesos sospechosos
- An�lisis de seguridad
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

**Prop�sito**: Patr�n Outbox para mensajer�a confiable.

**Por qu� patr�n Outbox**:
- **Consistencia**: Garantiza que eventos se publiquen solo si la transacci�n principal tiene �xito
- **Resiliencia**: Reintentos autom�ticos en caso de fallo
- **Auditor�a**: Registro completo de eventos publicados

**Flujo**:
1. Transacci�n guarda cambio en BD + OutboxEvent
2. Job en background lee OutboxEvents no procesados
3. Publica a message broker (RabbitMQ, Azure Service Bus, etc.)
4. Marca como procesado

---

#### **JobProgramado** (BaseEntity)
```sql
Campos:
- Id: Guid (PK)
- JobName: string (requerido, �nico)
- CronExpression: string (requerido)
- Activo: bool (default: true)
- UltimaEjecucion: DateTime?
- ProximaEjecucion: DateTime?
- Estado: enum EstadoJob (Pendiente, Ejecutando, Completado, Fallido)
- Parametros: string? (JSON)
```

**Prop�sito**: Configuraci�n y tracking de jobs programados (cron jobs).

**Jobs t�picos**:
- Env�o de recordatorios de citas
- Generaci�n de alertas de inventario
- Limpieza de notificaciones antiguas
- Procesamiento de webhooks pendientes
- Reportes programados

**Por qu� en BD**:
- Configuraci�n din�mica sin redespliegue
- Monitoring centralizado
- Historial de ejecuciones

---

## Relaciones entre M�dulos

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
       ?                    ?     ?   Cl�nica    ?
       ?                    ?     ?   (Citas)    ?
       ?                    ?     ????????????????
       ?                    ?            ?
       ?                    ?????????????????? ????????????????????
       ?                    ?            ?     ? Historial M�dico ?
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
                                  ?  Auditor�a   ?
                                  ?   (Logs)     ?
                                  ????????????????
```

### Relaciones Clave

#### 1. **Usuario ? M�ltiples Roles**
Un usuario puede ser simult�neamente adoptante, veterinario y donante.
```
Usuario (Juan P�rez)
?? Rol: Adoptante ? puede solicitar adopciones
?? Rol: Veterinario ? puede atender citas
?? Empleado ? tiene horarios y especialidades
```

#### 2. **Mascota ? Historial Completo**
Cada mascota tiene registro detallado de su historia.
```
Mascota (Firulais)
?? Expedientes m�dicos
?? Vacunaciones
?? Cirug�as
?? Desparasitaciones
?? Valoraciones
?? Fotos
?? Solicitudes de adopci�n
```

#### 3. **Cita ? M�ltiples Entidades**
Una cita conecta varios m�dulos.
```
Cita
?? Mascota (paciente)
?? Usuario (propietario si es externo)
?? Usuario (veterinario)
?? Sala (espacio f�sico)
?? Recordatorios
?? Expediente (resultado de la consulta)
?? ReporteUsoInsumos (insumos usados)
```

#### 4. **ReporteUsoInsumos ? Descuento FIFO**
Proceso complejo de descuento autom�tico.
```
ReporteUsoInsumos
?? Cita (procedimiento origen)
?? Detalles (items usados)
?   ?? ReporteUsoSplitLote
?       ?? Lote A (30ml, vence primero)
?       ?? Lote B (20ml, vence despu�s)
?? MovimientosInventario (salidas generadas)
```

---

## Patrones de Dise�o Aplicados

### 1. **Domain-Driven Design (DDD)**

**Agregados**: Entidades principales que aseguran consistencia.
- **Usuario** es agregado de Dispositivos, Notificaciones
- **Mascota** es agregado de Fotos, Solicitudes
- **Compra** es agregado de DetalleCompra

**Value Objects**: Enumeraciones fuertemente tipadas.
- EstatusUsuario, TipoCita, StatusDonacion, etc.

### 2. **Repository Pattern**

Separaci�n de l�gica de acceso a datos mediante interfaces:
```
IUsuarioRepository
IMascotaRepository
IInventarioRepository
```

### 3. **Soft Delete Pattern**

Entidades cr�ticas heredan de `SoftDeletableEntity`:
- Mascota (preservar historial de fallecidas)
- Sala (preservar historial de citas pasadas)

### 4. **Audit Pattern**

Todas las entidades importantes heredan de `AuditableEntity`:
- Timestamps autom�ticos en SaveChanges
- Tracking de usuario creador/modificador

### 5. **Outbox Pattern**

Para mensajer�a confiable:
- Eventos se guardan en OutboxEvent en misma transacci�n
- Job en background procesa y publica eventos
- Garant�a de entrega eventual

### 6. **FIFO (First In, First Out)**

Para inventario de medicamentos:
- Lotes se ordenan por fecha de expiraci�n
- Descuento autom�tico del lote m�s antiguo
- ReporteUsoSplitLote registra exactamente qu� lotes se usaron

### 7. **Idempotency Pattern**

Para operaciones cr�ticas:
- **ClientUsageId** en ReporteUsoInsumos
- **EventId** �nico en WebhookEvent
- Previene duplicados por reintentos de red

### 8. **State Machine Pattern**

Entidades con flujo de estados bien definido:
- **SolicitudAdopcion**: Pendiente ? EnRevision ? Aprobada/Rechazada
- **Cita**: Programada ? EnProceso ? Completada/Cancelada
- **Compra**: Pendiente ? Confirmada ? Recibida

### 9. **Logging & Traceability**

M�ltiples niveles de auditor�a:
- **AdopcionLog**: Cambios de estado de solicitudes
- **CitaHistorialEstado**: Cambios de estado de citas
- **AuditLog**: Cambios cr�ticos en cualquier entidad
- **MovimientoInventario**: Cada movimiento de stock

---

## Consideraciones de Seguridad

### 1. **Autenticaci�n**
- Passwords hasheados con salt �nico por usuario
- Tokens JWT para sesiones stateless
- Refresh tokens para renovaci�n segura

### 2. **Autorizaci�n**
- RBAC (Role-Based Access Control)
- Un usuario puede tener m�ltiples roles
- Permisos granulares por rol

### 3. **Privacidad**
- Soft delete preserva datos pero los oculta
- Donaciones an�nimas respetan privacidad
- Consentimiento expl�cito de pol�ticas con versioning

### 4. **Auditor�a**
- Registro de IP y UserAgent en acciones cr�ticas
- TraceId para correlaci�n de operaciones
- Before/After JSON para an�lisis forense

### 5. **Integridad**
- FK con eliminaci�n restringida previene hu�rfanos
- Transacciones garantizan consistencia
- Validaciones en capa de dominio

---

## Optimizaciones de Performance

### 1. **�ndices**
Configuraciones de EntityFramework crean �ndices en:
- Email de Usuario (b�squedas frecuentes)
- PaypalOrderId (webhook lookups)
- Fechas de citas (queries de agenda)
- ExpDate de lotes (consultas FIFO)

### 2. **Query Filters**
Filtro global para soft delete:
```csharp
modelBuilder.Entity<Mascota>().HasQueryFilter(m => m.DeletedAt == null);
```
Autom�ticamente excluye eliminadas en todas las queries.

### 3. **Lazy Loading Deshabilitado**
Previene N+1 queries, requiere explicit `Include()`.

### 4. **Precision de Decimales**
Configuraci�n global: `decimal(18,2)` para cantidades monetarias.

### 5. **JSON Columns**
Campos como `DataJson` en Notificacion permiten metadatos flexibles sin esquema r�gido.

---

## Escalabilidad Futura

### M�dulos Planeados

1. **Pagos**
   - Integraci�n con m�ltiples pasarelas
   - Facturaci�n electr�nica
   - Subscripciones de padrinos de mascotas

2. **Reportes**
   - Dashboard de estad�sticas
   - Exportaci�n de reportes
   - Business Intelligence

3. **Comunicaci�n**
   - Chat interno
   - Emails transaccionales
   - SMS para recordatorios

4. **Telemetr�a**
   - Sensores IoT (comederos autom�ticos, c�maras)
   - Tracking de salud en tiempo real

5. **Adopciones Virtuales**
   - Videollamadas de adopci�n
   - Tours virtuales del refugio
   - Sistema de matching autom�tico

---

## Conclusi�n

La base de datos de **AdoPets** est� dise�ada para ser:
- **Escalable**: Arquitectura modular permite a�adir funcionalidades sin afectar existentes
- **Auditable**: Trazabilidad completa de todas las operaciones cr�ticas
- **Flexible**: Soft delete y JSON columns permiten evoluci�n sin migraciones complejas
- **Segura**: M�ltiples capas de validaci�n y auditor�a
- **Eficiente**: FIFO autom�tico, �ndices optimizados, query filters

El dise�o refleja las necesidades reales de un refugio moderno:
- Gesti�n de adopciones con proceso de evaluaci�n
- Cl�nica veterinaria completa con historial m�dico
- Control de inventario m�dico con trazabilidad total
- Sistema de donaciones con integraci�n de pagos
- Administraci�n de personal y recursos

Cada decisi�n de dise�o est� justificada por requisitos funcionales, t�cnicos o regulatorios espec�ficos.
