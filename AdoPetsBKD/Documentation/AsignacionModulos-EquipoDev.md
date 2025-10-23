# Plan de Asignación de Módulos - AdoPets Backend

## Información del Proyecto

- **Nombre**: AdoPets Backend
- **Tecnología**: .NET 9 / ASP.NET Core Web API
- **Arquitectura**: Clean Architecture (DDD)
- **Base de Datos**: SQL Server con Entity Framework Core
- **Equipo**: 5 Desarrolladores

---

## ?? Distribución de Módulos por Desarrollador

La asignación se ha realizado considerando:
- ? Equilibrio de complejidad técnica
- ? Dependencias entre módulos
- ? Carga de trabajo balanceada
- ? Experiencia requerida en cada área
- ? Posibilidad de trabajo paralelo

---

## ????? Developer 1 - **Security & Authentication Lead**

### Módulos Asignados
#### 1. **Módulo de Seguridad (Security)** ??
- **Prioridad**: CRÍTICA (Bloqueante para otros módulos)
- **Complejidad**: Alta
- **Tiempo Estimado**: 3-4 semanas

#### Responsabilidades:
- ? Sistema de autenticación JWT completo
- ? Gestión de usuarios (CRUD)
- ? Sistema de roles y permisos (RBAC)
- ? Hash de contraseñas con salt
- ? Refresh tokens
- ? Gestión de dispositivos
- ? Sistema de notificaciones (in-app, push, email)
- ? Preferencias de notificación por usuario
- ? Consentimientos y políticas de privacidad (GDPR/LFPDPPP)

#### Entidades:
- `Usuario`
- `Rol`
- `UsuarioRol`
- `Dispositivo`
- `Notificacion`
- `PreferenciaNotificacion`
- `ConsentimientoPolitica`

#### Endpoints a Desarrollar:
```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
POST   /api/auth/change-password
GET    /api/usuarios
GET    /api/usuarios/{id}
PUT    /api/usuarios/{id}
DELETE /api/usuarios/{id}
POST   /api/usuarios/{id}/roles
GET    /api/notificaciones
PUT    /api/notificaciones/{id}/read
```

#### Consideraciones Especiales:
- Implementar middleware de autenticación
- Configurar políticas de autorización
- Implementar rate limiting
- Logs de auditoría para acciones de seguridad
- Documentar esquema de tokens y claims

---

## ????? Developer 2 - **Adoption & Pets Management Lead**

### Módulos Asignados
#### 1. **Módulo de Mascotas (Mascotas)** ??
- **Prioridad**: Alta
- **Complejidad**: Media
- **Tiempo Estimado**: 2-3 semanas
- **Depende de**: Security (autenticación básica)

#### Responsabilidades:
- ? CRUD de mascotas del refugio
- ? Gestión de galería de fotos
- ? Sistema de soft delete
- ? Filtros y búsquedas avanzadas
- ? Proceso completo de adopción
- ? Solicitudes de adopción con evaluación
- ? Adjuntos de documentos soporte
- ? Trazabilidad de cambios de estado (AdopcionLog)

#### Entidades:
- `Mascota`
- `MascotaFoto`
- `SolicitudAdopcion`
- `SolicitudAdopcionAdjunto`
- `AdopcionLog`

#### Endpoints a Desarrollar:
```
GET    /api/mascotas (con filtros: disponible, especie, edad, etc.)
GET    /api/mascotas/{id}
POST   /api/mascotas
PUT    /api/mascotas/{id}
DELETE /api/mascotas/{id} (soft delete)
POST   /api/mascotas/{id}/fotos
DELETE /api/mascotas/{id}/fotos/{fotoId}
POST   /api/solicitudes-adopcion
GET    /api/solicitudes-adopcion/{id}
PUT    /api/solicitudes-adopcion/{id}/aprobar
PUT    /api/solicitudes-adopcion/{id}/rechazar
GET    /api/solicitudes-adopcion/usuario/{userId}
POST   /api/solicitudes-adopcion/{id}/adjuntos
```

#### Consideraciones Especiales:
- Integración con servicio de almacenamiento de imágenes (Azure Blob, AWS S3, o local)
- Validación de tamaño y formato de imágenes
- Generación de thumbnails
- Máquina de estados para adopciones
- Notificaciones a adoptantes

---

## ????? Developer 3 - **Clinic & Medical Records Lead**

### Módulos Asignados
#### 1. **Módulo de Clínica (Clinica)** ??
- **Prioridad**: Alta
- **Complejidad**: Media-Alta
- **Tiempo Estimado**: 2-3 semanas
- **Depende de**: Security, Mascotas, Servicios (parcial)

#### 2. **Módulo de Historial Clínico (HistorialClinico)** ??
- **Prioridad**: Alta
- **Complejidad**: Media-Alta
- **Tiempo Estimado**: 2-3 semanas
- **Depende de**: Clinica

#### Responsabilidades Clínica:
- ? Sistema de agenda de citas
- ? Prevención de solapamiento de horarios
- ? Gestión de salas
- ? Recordatorios automáticos (email, SMS, push)
- ? Historial de estados de citas
- ? Citas para mascotas del refugio y externas
- ? **Tickets de procedimientos** (recién agregado)
- ? **Solicitudes de citas digitales** (recién agregado)
- ? **Sistema de pagos con PayPal** (recién agregado)

#### Responsabilidades Historial Clínico:
- ? Expedientes médicos (metodología SOAP)
- ? Registro de vacunaciones con plan de refuerzos
- ? Desparasitaciones periódicas
- ? Cirugías con detalles completos
- ? Valoraciones (signos vitales)
- ? Adjuntos médicos (rayos X, análisis, imágenes)

#### Entidades Clínica:
- `Sala`
- `Cita`
- `CitaRecordatorio`
- `CitaHistorialEstado`
- `Ticket` ? NUEVO
- `TicketDetalle` ? NUEVO
- `Pago` ? NUEVO
- `SolicitudCitaDigital` ? NUEVO

#### Entidades Historial Clínico:
- `Expediente`
- `AdjuntoMedico`
- `Vacunacion`
- `Desparasitacion`
- `Cirugia`
- `Valoracion`

#### Endpoints a Desarrollar:

**Clínica:**
```
GET    /api/citas (con filtros por fecha, veterinario, estado)
GET    /api/citas/{id}
POST   /api/citas
PUT    /api/citas/{id}
DELETE /api/citas/{id}
PUT    /api/citas/{id}/cancelar
PUT    /api/citas/{id}/completar
GET    /api/citas/disponibilidad (verificar horarios libres)
GET    /api/salas
POST   /api/salas

// NUEVOS ENDPOINTS - Tickets
POST   /api/tickets
GET    /api/tickets/{id}
GET    /api/tickets/numero/{numero}
GET    /api/tickets/cliente/{clienteId}
PUT    /api/tickets/{id}/entregar
GET    /api/tickets/{id}/pdf

// NUEVOS ENDPOINTS - Citas Digitales
POST   /api/solicitudescitasdigitales
GET    /api/solicitudescitasdigitales/{id}
GET    /api/solicitudescitasdigitales/pendientes
POST   /api/solicitudescitasdigitales/verificar-disponibilidad
POST   /api/solicitudescitasdigitales/confirmar
POST   /api/solicitudescitasdigitales/rechazar

// NUEVOS ENDPOINTS - Pagos
POST   /api/pagos
POST   /api/pagos/paypal/create-order
POST   /api/pagos/paypal/capture
GET    /api/pagos/{id}
GET    /api/pagos/usuario/{userId}
POST   /api/pagos/webhook/paypal
```

**Historial Clínico:**
```
POST   /api/expedientes
GET    /api/expedientes/{id}
GET    /api/expedientes/mascota/{mascotaId}
POST   /api/expedientes/{id}/adjuntos
POST   /api/vacunaciones
GET    /api/vacunaciones/mascota/{mascotaId}
POST   /api/desparasitaciones
GET    /api/desparasitaciones/mascota/{mascotaId}
POST   /api/cirugias
GET    /api/cirugias/mascota/{mascotaId}
POST   /api/valoraciones
GET    /api/valoraciones/mascota/{mascotaId}
```

#### Consideraciones Especiales:
- Algoritmo de detección de conflictos de horarios
- Job programado para envío de recordatorios
- Validación de plan de vacunación
- Alertas de refuerzos próximos a vencer
- Generación de PDF para tickets (QuestPDF)
- Integración con PayPal SDK
- Webhooks de PayPal para actualización automática de pagos
- Sistema de anticipos (50% del costo)

---

## ????? Developer 4 - **Inventory & Supply Chain Lead**

### Módulos Asignados
#### 1. **Módulo de Inventario (Inventario)** ??
- **Prioridad**: Alta
- **Complejidad**: Alta (Sistema FIFO)
- **Tiempo Estimado**: 3-4 semanas
- **Depende de**: Security, Clinica (para reportes de uso)

#### Responsabilidades:
- ? Catálogo maestro de insumos médicos
- ? Sistema de lotes con fechas de vencimiento
- ? Control FIFO (First In, First Out) automatizado
- ? Movimientos de inventario (entradas, salidas, ajustes)
- ? Alertas automáticas de stock bajo
- ? Alertas de productos próximos a vencer
- ? Gestión de proveedores
- ? Órdenes de compra
- ? Recepción de mercancía con creación de lotes
- ? Reportes de uso de insumos por procedimiento
- ? Trazabilidad completa de lotes (crítico para retiros)

#### Entidades:
- `ItemInventario`
- `LoteInventario`
- `MovimientoInventario`
- `AlertaInventario`
- `Proveedor`
- `Compra`
- `DetalleCompra`
- `ReporteUsoInsumos`
- `ReporteUsoInsumoDetalle`
- `ReporteUsoSplitLote`

#### Endpoints a Desarrollar:
```
GET    /api/inventario (con filtros: categoría, stock bajo, vencimiento)
GET    /api/inventario/{id}
POST   /api/inventario
PUT    /api/inventario/{id}
GET    /api/inventario/{id}/lotes
POST   /api/inventario/movimientos
GET    /api/inventario/alertas
POST   /api/proveedores
GET    /api/proveedores
POST   /api/compras
GET    /api/compras/{id}
PUT    /api/compras/{id}/recibir
POST   /api/reportes-uso (descuento automático FIFO)
GET    /api/reportes-uso/{id}/trazabilidad
GET    /api/inventario/reportes/valoracion-stock
```

#### Consideraciones Especiales:
- **Algoritmo FIFO crítico**: Descuento automático del lote más antiguo
- Manejo de split de lotes cuando uno no tiene suficiente stock
- Job programado para verificar vencimientos
- Job programado para generar alertas de stock mínimo
- Cálculo de costos por procedimiento
- Validación de stock antes de permitir uso
- Historial completo para auditorías
- Reporte de retiro de lotes defectuosos

---

## ????? Developer 5 - **Services, Donations & Infrastructure Lead**

### Módulos Asignados
#### 1. **Módulo de Servicios (Servicios)** ??
- **Prioridad**: Media
- **Complejidad**: Media
- **Tiempo Estimado**: 2 semanas
- **Depende de**: Security

#### 2. **Módulo de Donaciones (Donaciones)** ??
- **Prioridad**: Media
- **Complejidad**: Media (Integración PayPal)
- **Tiempo Estimado**: 2 semanas
- **Depende de**: Security

#### 3. **Módulo de Auditoría (Auditoria)** ??
- **Prioridad**: Media-Alta (Transversal)
- **Complejidad**: Media
- **Tiempo Estimado**: 1-2 semanas
- **Depende de**: Security

#### Responsabilidades Servicios:
- ? Gestión de empleados (información laboral)
- ? Relación con usuarios (1:1 opcional)
- ? Especialidades veterinarias
- ? Asignación de especialidades a empleados
- ? Catálogo de servicios veterinarios
- ? Horarios de trabajo
- ? Validación de disponibilidad

#### Responsabilidades Donaciones:
- ? Recepción de donaciones vía PayPal
- ? Webhooks de PayPal
- ? Manejo de idempotencia (evitar duplicados)
- ? Donaciones anónimas
- ? Reporte de recaudación
- ? Captura y confirmación de pagos

#### Responsabilidades Auditoría:
- ? AuditLog (registro de acciones críticas)
- ? OutboxEvent (patrón outbox para mensajería)
- ? JobProgramado (configuración de cron jobs)
- ? Trazabilidad completa con before/after JSON
- ? Correlación con TraceId

#### Entidades Servicios:
- `Empleado`
- `Especialidad`
- `EmpleadoEspecialidad`
- `Servicio`
- `Horario`

#### Entidades Donaciones:
- `Donacion`
- `WebhookEvent`

#### Entidades Auditoría:
- `AuditLog`
- `OutboxEvent`
- `JobProgramado`

#### Endpoints a Desarrollar:

**Servicios:**
```
GET    /api/empleados
GET    /api/empleados/{id}
POST   /api/empleados
PUT    /api/empleados/{id}
GET    /api/especialidades
POST   /api/especialidades
POST   /api/empleados/{id}/especialidades
GET    /api/servicios
POST   /api/servicios
PUT    /api/servicios/{id}
GET    /api/horarios/empleado/{empleadoId}
POST   /api/horarios
```

**Donaciones:**
```
POST   /api/donaciones/paypal/create-order
POST   /api/donaciones/paypal/capture
POST   /api/donaciones/webhook (webhook de PayPal)
GET    /api/donaciones
GET    /api/donaciones/{id}
GET    /api/donaciones/usuario/{userId}
GET    /api/donaciones/reportes/total-recaudado
```

**Auditoría:**
```
GET    /api/auditoria/logs (con filtros: usuario, entidad, fecha)
GET    /api/auditoria/outbox (eventos pendientes)
POST   /api/auditoria/outbox/{id}/retry
GET    /api/jobs
POST   /api/jobs
PUT    /api/jobs/{id}/toggle
```

#### Consideraciones Especiales:
- Configuración de PayPal SDK
- Validación de signatures de webhooks
- Retry logic para OutboxEvents
- Implementación de jobs con Hangfire o Quartz.NET
- Dashboard de auditoría
- Filtros de búsqueda avanzada en logs

---

## ?? Cronograma Sugerido

### **Sprint 1 (Semanas 1-2): Fundamentos**
| Dev | Tareas |
|-----|--------|
| Dev 1 | ? Autenticación JWT, Usuarios, Roles |
| Dev 2 | ? Entidades de Mascotas (esperando autenticación) |
| Dev 3 | ? Entidades de Clínica y Historial |
| Dev 4 | ? Entidades de Inventario |
| Dev 5 | ? Servicios, Auditoría (configuración inicial) |

### **Sprint 2 (Semanas 3-4): Desarrollo Core**
| Dev | Tareas |
|-----|--------|
| Dev 1 | ? Notificaciones, Dispositivos, Preferencias |
| Dev 2 | ? CRUD Mascotas, Fotos, Solicitudes Adopción |
| Dev 3 | ? CRUD Citas, Salas, Sistema de agenda |
| Dev 4 | ? CRUD Inventario, Lotes, Movimientos |
| Dev 5 | ? CRUD Empleados, Servicios, Horarios |

### **Sprint 3 (Semanas 5-6): Funcionalidades Avanzadas**
| Dev | Tareas |
|-----|--------|
| Dev 1 | ? Consentimientos, Políticas, Auditoría de seguridad |
| Dev 2 | ? Proceso completo de adopción, Documentos adjuntos |
| Dev 3 | ? Historial Clínico completo, Tickets, Citas Digitales |
| Dev 4 | ? Sistema FIFO, Alertas, Reportes de uso |
| Dev 5 | ? Donaciones PayPal, Webhooks |

### **Sprint 4 (Semanas 7-8): Integraciones y Refinamiento**
| Dev | Tareas |
|-----|--------|
| Dev 1 | ? Rate limiting, Políticas avanzadas, Testing |
| Dev 2 | ? Integración con storage, Notificaciones adoptantes |
| Dev 3 | ? Recordatorios automáticos, PayPal para citas, PDFs |
| Dev 4 | ? Jobs de alertas, Reportes avanzados |
| Dev 5 | ? Jobs programados, OutboxEvent processing |

### **Sprint 5 (Semanas 9-10): Testing e Integración**
| Todos | Tareas |
|-------|--------|
| Equipo | ? Testing de integración entre módulos |
| Equipo | ? Testing E2E de flujos completos |
| Equipo | ? Corrección de bugs |
| Equipo | ? Optimización de queries |
| Equipo | ? Documentación final |

---

## ?? Matriz de Dependencias

| Módulo | Depende De | Bloquea A |
|--------|-----------|-----------|
| **Security** | - | Todos los demás |
| **Mascotas** | Security | Clínica (parcial), Adopciones |
| **Clínica** | Security, Servicios (parcial) | Historial Clínico, Inventario |
| **Historial Clínico** | Security, Clínica, Mascotas | - |
| **Inventario** | Security, Clínica (reportes uso) | - |
| **Servicios** | Security | Clínica |
| **Donaciones** | Security | - |
| **Auditoría** | Security | - |

**Conclusión**: Dev 1 (Security) debe comenzar primero y completar autenticación básica para desbloquear a los demás.

---

## ?? Responsabilidades Compartidas

### Todos los Desarrolladores:
1. ? Escribir tests unitarios para su código
2. ? Documentar endpoints en Swagger/OpenAPI
3. ? Seguir convenciones de código del equipo
4. ? Code reviews cruzados
5. ? Participar en daily standups
6. ? Actualizar tablero Kanban/Scrum

### Tech Lead / Arquitecto (puede ser Dev 1 o externo):
1. ? Definir estándares de código
2. ? Revisar arquitectura
3. ? Resolver bloqueos técnicos
4. ? Configuración inicial del proyecto
5. ? CI/CD pipeline

---

## ?? Checklist por Desarrollador

### Para cada módulo, el desarrollador debe:

#### Backend (API)
- [ ] Crear entidades de dominio
- [ ] Configurar Entity Framework (Fluent API)
- [ ] Crear DTOs (Request/Response)
- [ ] Implementar repositorios (si aplica)
- [ ] Implementar servicios de negocio
- [ ] Crear controladores API
- [ ] Validaciones de datos (FluentValidation)
- [ ] Manejo de errores centralizado
- [ ] Logging con ILogger
- [ ] Tests unitarios (mínimo 80% cobertura)
- [ ] Tests de integración
- [ ] Documentación Swagger
- [ ] Seeders de datos de prueba

#### Base de Datos
- [ ] Crear migración de EF Core
- [ ] Verificar índices en columnas clave
- [ ] Verificar relaciones y constraints
- [ ] Verificar soft delete (si aplica)
- [ ] Scripts de datos iniciales

#### Documentación
- [ ] README del módulo
- [ ] Diagramas de flujo (si son complejos)
- [ ] Ejemplos de uso de API
- [ ] Notas de configuración especial

---

## ??? Herramientas y Stack Técnico

### Desarrollo
- **IDE**: Visual Studio 2022 / Rider / VS Code
- **Framework**: .NET 9 / ASP.NET Core Web API
- **ORM**: Entity Framework Core 9
- **Base de Datos**: SQL Server
- **Autenticación**: JWT Bearer Tokens
- **Validación**: FluentValidation
- **Mapping**: AutoMapper (opcional)
- **Testing**: xUnit / NUnit + Moq
- **Documentación**: Swagger/OpenAPI

### Integraciones
- **PayPal**: PayPalCheckoutSdk
- **PDF Generation**: QuestPDF
- **Jobs**: Hangfire / Quartz.NET
- **Storage**: Azure Blob Storage / AWS S3
- **Email**: SendGrid / SMTP
- **SMS**: Twilio

### DevOps
- **Control de Versiones**: Git + GitHub
- **CI/CD**: GitHub Actions / Azure DevOps
- **Contenedores**: Docker
- **Monitoreo**: Application Insights / Serilog

---

## ?? Canales de Comunicación

### Daily Standup (15 min)
- **Horario**: 9:00 AM
- **Plataforma**: Microsoft Teams / Zoom
- **Formato**: ¿Qué hice ayer? ¿Qué haré hoy? ¿Tengo bloqueos?

### Sprint Planning (2 horas)
- **Frecuencia**: Cada 2 semanas
- **Objetivo**: Definir tareas del sprint

### Sprint Review (1 hora)
- **Frecuencia**: Cada 2 semanas
- **Objetivo**: Demostrar funcionalidades completadas

### Sprint Retrospective (1 hora)
- **Frecuencia**: Cada 2 semanas
- **Objetivo**: Mejorar procesos del equipo

### Chat Continuo
- **Plataforma**: Slack / Microsoft Teams
- **Canales**:
  - #general
  - #dev-backend
  - #code-reviews
  - #bugs
  - #devops

---

## ?? Criterios de "Definition of Done"

Una tarea se considera completa cuando:
- ? Código compilado sin warnings
- ? Tests unitarios escritos y pasando
- ? Code review aprobado por al menos 1 peer
- ? Migración de BD aplicada en desarrollo
- ? Endpoint documentado en Swagger
- ? Logs implementados en puntos clave
- ? Manejo de errores implementado
- ? Validaciones de entrada implementadas
- ? Integrado en rama `develop` sin conflictos
- ? Demo funcional en ambiente de desarrollo

---

## ?? Métricas de Éxito

### Por Desarrollador
- Cumplimiento de estimaciones (±20%)
- Cobertura de tests (>80%)
- Bugs críticos en producción (0)
- Tiempo promedio de code review (<24h)

### Por Equipo
- Velocidad del sprint (story points completados)
- Tiempo de ciclo (tiempo de tarea en progreso)
- Deuda técnica controlada
- Disponibilidad de API (>99%)

---

## ?? Recomendaciones Finales

### Para Dev 1 (Security):
- Comienza inmediatamente, tu módulo es bloqueante
- Documenta bien el esquema de tokens y claims
- Crea un usuario de prueba en el seeder
- Implementa middleware de autenticación robusto

### Para Dev 2 (Mascotas):
- Coordina con Dev 1 para tener autenticación básica
- Define estrategia de almacenamiento de imágenes temprano
- Prepara datos de prueba de mascotas variadas

### Para Dev 3 (Clínica):
- Tienes el módulo más grande, prioriza bien
- El algoritmo de detección de conflictos es crítico
- Coordina con Dev 5 para integración PayPal
- Considera usar librerías para manejo de calendarios

### Para Dev 4 (Inventario):
- El sistema FIFO es complejo, dedica tiempo al diseño
- Escribe tests extensivos para el algoritmo de descuento
- Documenta bien la lógica de split de lotes
- Considera casos edge (stock 0, múltiples lotes vencidos)

### Para Dev 5 (Servicios/Donaciones):
- Tu trabajo es más distribuido, organiza bien el tiempo
- Configura PayPal en sandbox desde el inicio
- Implementa los jobs programados de forma genérica
- Los logs de auditoría benefician a todos, házlos bien

---

## ?? Convenciones de Código

### Commits
```
feat: Agregar endpoint de creación de usuarios
fix: Corregir cálculo de IVA en tickets
docs: Actualizar README de módulo de inventario
test: Agregar tests para sistema FIFO
refactor: Mejorar algoritmo de detección de conflictos
```

### Ramas
```
main          -> Producción
develop       -> Desarrollo integrado
feature/SEC-123-jwt-auth -> Nueva funcionalidad
bugfix/INV-456-fifo-fix  -> Corrección de bug
hotfix/SEC-789-security  -> Corrección urgente en producción
```

### Pull Requests
- Título descriptivo
- Descripción de cambios
- Screenshots (si aplica)
- Checklist de tareas completadas
- Asignar al menos 1 reviewer

---

## ?? Inicio del Proyecto

### Día 1:
1. ? Reunión de kickoff (2 horas)
2. ? Configuración de ambientes de desarrollo
3. ? Creación de repositorio y estructura inicial
4. ? Asignación oficial de módulos
5. ? Dev 1 comienza con autenticación

### Semana 1:
- Daily standups diarios
- Configuración de herramientas
- Primeros commits de cada desarrollador
- Primera code review cruzada

---

**¡Éxito al equipo! ??**

---

**Versión**: 1.0  
**Fecha**: Enero 2024  
**Proyecto**: AdoPets Backend  
**Equipo**: 5 Developers  
**Duración Estimada**: 10 semanas (2.5 meses)
