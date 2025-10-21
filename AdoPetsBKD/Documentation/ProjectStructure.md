# Documentación de la Estructura del Proyecto - AdoPets Backend

## Índice
1. [Visión General de la Arquitectura](#visión-general-de-la-arquitectura)
2. [Estructura de Carpetas](#estructura-de-carpetas)
3. [Capa de Dominio (Domain)](#capa-de-dominio-domain)
4. [Capa de Aplicación (Application)](#capa-de-aplicación-application)
5. [Capa de Infraestructura (Infrastructure)](#capa-de-infraestructura-infrastructure)
6. [Capa de Presentación (Controllers)](#capa-de-presentación-controllers)
7. [Migraciones (Migrations)](#migraciones-migrations)
8. [Configuración y Archivos Raíz](#configuración-y-archivos-raíz)
9. [Convenciones de Nomenclatura](#convenciones-de-nomenclatura)
10. [Flujo de Datos](#flujo-de-datos)

---

## Visión General de la Arquitectura

El proyecto **AdoPets Backend** está construido siguiendo los principios de **Clean Architecture** (Arquitectura Limpia) y **Domain-Driven Design (DDD)**, lo que permite:

- ? Separación clara de responsabilidades
- ? Testabilidad mejorada
- ? Independencia de frameworks
- ? Mantenibilidad a largo plazo
- ? Escalabilidad horizontal

### Capas de la Arquitectura

```
???????????????????????????????????????????
?         Presentation Layer              ?  ? Controllers (API REST)
?         (Controllers, DTOs)             ?
???????????????????????????????????????????
              ?
???????????????????????????????????????????
?        Application Layer                ?  ? Servicios, Interfaces
?    (Use Cases, DTOs, Interfaces)        ?
???????????????????????????????????????????
              ?
???????????????????????????????????????????
?          Domain Layer                   ?  ? Entidades, Lógica de Negocio
?  (Entities, Value Objects, Domain Logic)?
???????????????????????????????????????????
              ?
???????????????????????????????????????????
?      Infrastructure Layer               ?  ? EF Core, Servicios Externos
?   (Data Access, External Services)      ?
???????????????????????????????????????????
```

**Reglas de dependencia**:
- ? **Domain** NO depende de nada (núcleo independiente)
- ? **Application** depende solo de **Domain**
- ? **Infrastructure** depende de **Domain** y **Application**
- ? **Controllers** dependen de **Application**

---

## Estructura de Carpetas

```
AdoPetsBKD/
??? ?? Domain/                    # Capa de Dominio (núcleo del negocio)
?   ??? ?? Common/                # Clases base compartidas
?   ??? ?? Entities/              # Entidades de negocio organizadas por módulo
?       ??? ?? Security/          # Usuarios, Roles, Autenticación
?       ??? ?? Mascotas/          # Mascotas, Adopciones
?       ??? ?? Clinica/           # Citas, Salas
?       ??? ?? HistorialClinico/  # Expedientes, Vacunas, Cirugías
?       ??? ?? Inventario/        # Items, Lotes, Movimientos
?       ??? ?? Donaciones/        # Donaciones PayPal
?       ??? ?? Servicios/         # Empleados, Especialidades
?       ??? ?? Auditoria/         # Logs, Eventos
?
??? ?? Application/               # Capa de Aplicación (casos de uso)
?   ??? ?? DTOs/                  # Data Transfer Objects
?   ?   ??? ?? Auth/              # DTOs de autenticación
?   ?   ??? ?? Usuarios/          # DTOs de usuarios
?   ??? ?? Interfaces/            # Contratos/interfaces
?   ?   ??? ?? Repositories/      # Interfaces de repositorios
?   ?   ??? ?? Services/          # Interfaces de servicios
?   ??? ?? Common/                # Clases comunes de aplicación
?
??? ?? Infrastructure/            # Capa de Infraestructura (implementaciones)
?   ??? ?? Data/                  # Acceso a datos con EF Core
?   ?   ??? ?? Configurations/    # Configuraciones de entidades EF
?   ?   ?   ??? ?? Security/      # Configs de entidades de seguridad
?   ?   ?   ??? ?? Mascotas/      # Configs de entidades de mascotas
?   ?   ?   ??? ?? Clinica/       # Configs de entidades de clínica
?   ?   ?   ??? ?? HistorialClinico/
?   ?   ?   ??? ?? Inventario/
?   ?   ?   ??? ?? Donaciones/
?   ?   ?   ??? ?? Servicios/
?   ?   ?   ??? ?? Auditoria/
?   ?   ??? ?? Seeders/           # Datos iniciales (seed)
?   ?   ??? AdoPetsDbContext.cs   # Contexto principal de EF Core
?   ??? ?? Repositories/          # Implementaciones de repositorios
?   ??? ?? Services/              # Implementaciones de servicios
?   ??? ?? Extensions/            # Extension methods
?   ??? ?? Configuration/         # Configuraciones de infraestructura
?
??? ?? Controllers/               # Capa de Presentación (API REST)
?   ??? AuthController.cs         # Endpoints de autenticación
?   ??? UsuariosController.cs     # Endpoints de usuarios
?   ??? HealthController.cs       # Health checks
?
??? ?? Migrations/                # Migraciones de Entity Framework
?
??? ?? Documentation/             # Documentación del proyecto
?   ??? DatabaseDesign.md         # Diseño de base de datos
?   ??? ProjectStructure.md       # Este archivo
?
??? Program.cs                    # Punto de entrada de la aplicación
??? AdoPetsBKD.csproj            # Archivo de proyecto .NET
```

---

## Capa de Dominio (Domain)

**Ubicación**: `Domain/`

**Propósito**: Contiene la lógica de negocio central, entidades y reglas de dominio. Es el corazón del sistema y **NO debe depender de ninguna otra capa**.

### ?? Domain/Common/

**Archivo**: `BaseEntity.cs`

**Qué contiene**:
- `BaseEntity`: Clase base con `Id: Guid`
- `AuditableEntity`: Añade timestamps de auditoría
- `SoftDeletableEntity`: Añade soft delete

**Para qué sirve**:
- Evitar repetición de código común
- Garantizar que todas las entidades tengan ID único
- Implementar auditoría automática en SaveChanges
- Permitir eliminación lógica sin perder datos

**Cómo se utiliza**:
```csharp
// Entidad simple
public class Rol : BaseEntity { }

// Entidad con auditoría
public class Usuario : AuditableEntity { }

// Entidad con soft delete
public class Mascota : SoftDeletableEntity { }
```

---

### ?? Domain/Entities/

Organizadas por **módulos de negocio** (bounded contexts de DDD).

#### ?? Domain/Entities/Security/

**Qué contiene**:
- `Usuario.cs`: Entidad principal de usuarios
- `Rol.cs`: Roles del sistema (Admin, Veterinario, etc.)
- `UsuarioRol.cs`: Relación N:M entre usuarios y roles
- `Dispositivo.cs`: Dispositivos registrados para notificaciones
- `Notificacion.cs`: Notificaciones in-app y push
- `PreferenciaNotificacion.cs`: Configuración de notificaciones por usuario
- `ConsentimientoPolitica.cs`: Historial de aceptación de políticas

**Para qué sirve**:
- Gestión de identidad y acceso (IAM)
- Control de autorización basado en roles (RBAC)
- Sistema de notificaciones
- Cumplimiento legal (GDPR, LFPDPPP)

**Cómo se utiliza**:
```csharp
// Crear usuario con rol
var usuario = new Usuario { Nombre = "Juan", Email = "juan@example.com" };
var rol = await _context.Roles.FirstAsync(r => r.Nombre == Roles.Veterinario);
usuario.UsuarioRoles.Add(new UsuarioRol { RolId = rol.Id });
```

---

#### ?? Domain/Entities/Mascotas/

**Qué contiene**:
- `Mascota.cs`: Entidad principal de mascotas
- `MascotaFoto.cs`: Galería de fotos
- `SolicitudAdopcion.cs`: Solicitudes de adopción
- `SolicitudAdopcionAdjunto.cs`: Documentos de la solicitud
- `AdopcionLog.cs`: Historial de cambios de estado

**Para qué sirve**:
- Gestión completa de mascotas del refugio
- Proceso de adopción con evaluación
- Tracking de estado de cada mascota
- Galería multimedia

**Cómo se utiliza**:
```csharp
// Mascota disponible para adopción
var mascota = new Mascota 
{
    Nombre = "Firulais",
    Especie = "Perro",
    Estatus = EstatusMascota.Disponible
};

// Solicitar adopción
var solicitud = new SolicitudAdopcion
{
    UsuarioId = adoptanteId,
    MascotaId = mascota.Id,
    Vivienda = TipoVivienda.Casa,
    MotivoAdopcion = "Busco compañía"
};
```

---

#### ?? Domain/Entities/Clinica/

**Qué contiene**:
- `Cita.cs`: Citas veterinarias
- `Sala.cs`: Salas de consulta/cirugía
- `CitaRecordatorio.cs`: Recordatorios automáticos
- `CitaHistorialEstado.cs`: Trazabilidad de cambios

**Para qué sirve**:
- Sistema de agenda veterinaria
- Gestión de espacios físicos
- Prevención de conflictos de horario
- Recordatorios automáticos para reducir no-shows

**Cómo se utiliza**:
```csharp
// Crear cita
var cita = new Cita
{
    MascotaId = mascotaId,
    VeterinarioId = veterinarioId,
    Tipo = TipoCita.Consulta,
    StartAt = DateTime.UtcNow.AddDays(1),
    DuracionMin = 30
};
cita.EndAt = cita.StartAt.AddMinutes(cita.DuracionMin);

// Validar solapamiento
bool haySolapamiento = cita.TieneSolapamiento(otraInicio, otraFin);
```

---

#### ?? Domain/Entities/HistorialClinico/

**Qué contiene**:
- `Expediente.cs`: Notas clínicas (SOAP)
- `Vacunacion.cs`: Registro de vacunas
- `Desparasitacion.cs`: Control de desparasitaciones
- `Cirugia.cs`: Procedimientos quirúrgicos
- `Valoracion.cs`: Signos vitales
- `AdjuntoMedico.cs`: Imágenes médicas (rayos X, etc.)

**Para qué sirve**:
- Historial médico completo de cada mascota
- Trazabilidad de tratamientos
- Cumplimiento de protocolos veterinarios
- Seguimiento de salud a largo plazo

**Cómo se utiliza**:
```csharp
// Crear expediente de consulta
var expediente = new Expediente
{
    MascotaId = mascotaId,
    VeterinarioId = veterinarioId,
    CitaId = citaId,
    MotivoConsulta = "Cojera en pata trasera",
    Diagnostico = "Esguince grado I",
    Tratamiento = "Reposo 7 días + antiinflamatorio"
};

// Registrar vacuna
var vacuna = new Vacunacion
{
    MascotaId = mascotaId,
    VaccineName = "Rabia",
    NextDueAt = DateTime.UtcNow.AddYears(1)
};
```

---

#### ?? Domain/Entities/Inventario/

**Qué contiene**:
- `ItemInventario.cs`: Catálogo de insumos
- `LoteInventario.cs`: Lotes con fechas de vencimiento
- `MovimientoInventario.cs`: Entradas/Salidas
- `AlertaInventario.cs`: Alertas automáticas
- `Proveedor.cs`: Proveedores
- `Compra.cs`: Órdenes de compra
- `DetalleCompra.cs`: Líneas de compra
- `ReporteUsoInsumos.cs`: Reporte post-procedimiento
- `ReporteUsoInsumoDetalle.cs`: Items usados
- `ReporteUsoSplitLote.cs`: Tracking FIFO de lotes

**Para qué sirve**:
- Control de inventario médico con FIFO automático
- Trazabilidad total de lotes (retirada de defectuosos)
- Gestión de compras a proveedores
- Alertas de stock bajo y próximos a vencer
- Costeo de procedimientos

**Cómo se utiliza**:
```csharp
// Registrar uso de insumos en procedimiento
var reporte = new ReporteUsoInsumos
{
    CitaId = citaId,
    VetId = veterinarioId,
    ClientUsageId = Guid.NewGuid().ToString() // idempotencia
};

reporte.Detalles.Add(new ReporteUsoInsumoDetalle
{
    ItemId = medicamentoId,
    QtyUsada = 50 // 50ml de antibiótico
});

// El sistema automáticamente:
// 1. Busca lotes con fecha de vencimiento más próxima (FIFO)
// 2. Descuenta stock de múltiples lotes si es necesario
// 3. Crea registros en ReporteUsoSplitLote con trazabilidad exacta
// 4. Genera MovimientoInventario de salida
```

---

#### ?? Domain/Entities/Donaciones/

**Qué contiene**:
- `Donacion.cs`: Donaciones vía PayPal
- `WebhookEvent.cs`: Log de webhooks de PayPal

**Para qué sirve**:
- Recepción de donaciones monetarias
- Integración con PayPal (órdenes y capturas)
- Idempotencia de webhooks
- Reportes de recaudación

**Cómo se utiliza**:
```csharp
// Crear donación pendiente
var donacion = new Donacion
{
    UsuarioId = usuarioId,
    Monto = 500,
    Moneda = "MXN",
    PaypalOrderId = "ORDER-123",
    Source = SourceDonacion.Checkout
};

// Capturar cuando PayPal confirma
donacion.Capturar(
    captureId: "CAPTURE-456",
    payerEmail: "donante@example.com"
);
```

---

#### ?? Domain/Entities/Servicios/

**Qué contiene**:
- `Empleado.cs`: Información laboral de empleados
- `Especialidad.cs`: Especialidades veterinarias
- `EmpleadoEspecialidad.cs`: Relación N:M
- `Servicio.cs`: Catálogo de servicios
- `Horario.cs`: Turnos de trabajo

**Para qué sirve**:
- Gestión de recursos humanos
- Asignación inteligente de citas según especialidad
- Control de horarios laborales
- Catálogo de servicios ofrecidos

**Cómo se utiliza**:
```csharp
// Empleado con especialidades
var empleado = new Empleado
{
    UsuarioId = usuarioId,
    Tipo = TipoEmpleado.Veterinario,
    Cedula = "12345678"
};

empleado.Especialidades.Add(new EmpleadoEspecialidad
{
    EspecialidadId = cirugiaId,
    FechaCertificacion = DateTime.UtcNow.AddYears(-2)
});

// Horario de trabajo
empleado.Horarios.Add(new Horario
{
    DiaSemana = DayOfWeek.Monday,
    HoraInicio = new TimeSpan(8, 0, 0),
    HoraFin = new TimeSpan(14, 0, 0)
});
```

---

#### ?? Domain/Entities/Auditoria/

**Qué contiene**:
- `AuditLog.cs`: Log de acciones críticas
- `OutboxEvent.cs`: Eventos para publicar (patrón outbox)
- `JobProgramado.cs`: Configuración de jobs

**Para qué sirve**:
- Auditoría completa de operaciones sensibles
- Mensajería confiable con patrón outbox
- Gestión de tareas programadas (cron jobs)
- Cumplimiento regulatorio

**Cómo se utiliza**:
```csharp
// Log de auditoría
var auditLog = new AuditLog
{
    UsuarioId = usuarioId,
    Entidad = nameof(Usuario),
    EntidadId = usuario.Id.ToString(),
    Accion = AccionAudit.PasswordChange,
    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
};

// Outbox event
var outboxEvent = new OutboxEvent
{
    EventType = "MascotaAdoptada",
    AggregateId = mascotaId.ToString(),
    Payload = JsonSerializer.Serialize(new { MascotaId, AdoptanteId })
};
```

---

## Capa de Aplicación (Application)

**Ubicación**: `Application/`

**Propósito**: Define los **casos de uso** de la aplicación, orquesta la lógica de dominio y define contratos (interfaces). Es la capa que coordina las operaciones.

### ?? Application/DTOs/

**Qué contiene**: Data Transfer Objects - objetos para transferir datos entre capas.

**Para qué sirve**:
- Desacoplar la API de las entidades de dominio
- Controlar exactamente qué datos se exponen
- Validaciones con DataAnnotations
- Modelar requests y responses específicos

**Estructura**:
```
DTOs/
??? Auth/                    # DTOs de autenticación
?   ??? LoginRequestDto.cs   # { Email, Password }
?   ??? LoginResponseDto.cs  # { Token, RefreshToken, Usuario }
?   ??? RegisterRequestDto.cs
?   ??? RefreshTokenRequestDto.cs
?   ??? ChangePasswordRequestDto.cs
?   ??? UsuarioDto.cs        # DTO de usuario sin datos sensibles
?
??? Usuarios/                # DTOs de gestión de usuarios
    ??? CreateUsuarioDto.cs
    ??? UpdateUsuarioDto.cs
    ??? UsuarioListDto.cs    # Para listados (datos mínimos)
    ??? UsuarioDetailDto.cs  # Para detalle completo
```

**Cómo se utiliza**:
```csharp
// En Controller - recibir DTO
[HttpPost("login")]
public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
{
    var response = await _authService.LoginAsync(request);
    return Ok(response);
}

// En Service - mapear de entidad a DTO
public UsuarioDto MapToDto(Usuario usuario)
{
    return new UsuarioDto
    {
        Id = usuario.Id,
        Email = usuario.Email,
        NombreCompleto = usuario.NombreCompleto
        // NO incluir PasswordHash ni datos sensibles
    };
}
```

---

### ?? Application/Interfaces/

**Qué contiene**: Contratos (interfaces) que definen qué puede hacer el sistema.

#### ?? Application/Interfaces/Repositories/

**Qué contiene**:
- `IUsuarioRepository.cs`
- `IRolRepository.cs`
- etc.

**Para qué sirve**:
- **Inversión de dependencias**: Application define el contrato, Infrastructure lo implementa
- Facilita testing con mocks
- Permite cambiar implementación sin afectar casos de uso

**Cómo se utiliza**:
```csharp
// Definir contrato en Application
public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(Guid id);
    Task<List<Usuario>> GetAllAsync();
    Task AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
}

// Implementar en Infrastructure
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AdoPetsDbContext _context;
    
    public async Task<Usuario?> GetByEmailAsync(string email)
        => await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
}

// Usar en Service
public class UsuarioService
{
    private readonly IUsuarioRepository _repository;
    
    public UsuarioService(IUsuarioRepository repository)
    {
        _repository = repository; // Inyección de dependencia
    }
}
```

---

#### ?? Application/Interfaces/Services/

**Qué contiene**:
- `IAuthService.cs`: Autenticación y autorización
- `ITokenService.cs`: Generación de JWT
- `IPasswordHasher.cs`: Hashing de passwords
- `IUsuarioService.cs`: Lógica de usuarios

**Para qué sirve**:
- Definir operaciones de negocio de alto nivel
- Separar lógica de aplicación de implementación
- Testability

**Cómo se utiliza**:
```csharp
// Interface define qué hace
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request);
}

// Service implementa cómo lo hace
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Buscar usuario
        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
        
        // 2. Validar password
        if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash, usuario.PasswordSalt))
            throw new UnauthorizedException();
        
        // 3. Generar token
        var token = _tokenService.GenerateToken(usuario);
        
        return new LoginResponseDto { Token = token };
    }
}
```

---

### ?? Application/Common/

**Qué contiene**: Clases comunes de la capa de aplicación.

**Archivo**: `ApiResponse.cs`

**Para qué sirve**:
- Estandarizar formato de respuestas de la API
- Manejar errores de forma consistente

**Cómo se utiliza**:
```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

// En controller
return Ok(ApiResponse<UsuarioDto>.SuccessResponse(usuarioDto));
return BadRequest(ApiResponse.ErrorResponse("Email ya existe"));
```

---

## Capa de Infraestructura (Infrastructure)

**Ubicación**: `Infrastructure/`

**Propósito**: Implementa los detalles técnicos: acceso a datos, servicios externos, configuraciones. **Depende** de Domain y Application.

### ?? Infrastructure/Data/

**Qué contiene**: Todo relacionado con Entity Framework Core y acceso a base de datos.

#### **AdoPetsDbContext.cs**

**Para qué sirve**:
- Contexto principal de Entity Framework Core
- Define DbSets (tablas)
- Aplica configuraciones de entidades
- Intercepta SaveChanges para auditoría automática

**Cómo se utiliza**:
```csharp
public class AdoPetsDbContext : DbContext
{
    // DbSets (representan tablas)
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Mascota> Mascotas => Set<Mascota>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplicar todas las configuraciones de la carpeta Configurations/
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdoPetsDbContext).Assembly);
        
        // Filtro global para soft delete
        modelBuilder.Entity<Mascota>().HasQueryFilter(m => m.DeletedAt == null);
    }
    
    // Auditoría automática
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps(); // Actualiza CreatedAt/UpdatedAt
        return base.SaveChangesAsync(cancellationToken);
    }
}

// Uso
await _context.Usuarios.AddAsync(usuario);
await _context.SaveChangesAsync();
```

---

#### ?? Infrastructure/Data/Configurations/

**Qué contiene**: Configuraciones de Entity Framework (Fluent API) organizadas por módulo.

**Para qué sirve**:
- Configurar relaciones entre entidades
- Definir índices, restricciones, longitudes
- Separar configuración de entidades (no contaminar entidades con atributos)

**Estructura**:
```
Configurations/
??? Security/
?   ??? UsuarioConfiguration.cs
?   ??? RolConfiguration.cs
?   ??? UsuarioRolConfiguration.cs
?   ??? NotificacionConfiguration.cs
?   ??? DispositivoConfiguration.cs
?   ??? PreferenciaNotificacionConfiguration.cs
?   ??? ConsentimientoPoliticaConfiguration.cs
?
??? Mascotas/
?   ??? MascotaConfiguration.cs
?   ??? MascotaFotoConfiguration.cs
?   ??? SolicitudAdopcionConfiguration.cs
?   ??? SolicitudAdopcionAdjuntoConfiguration.cs
?   ??? AdopcionLogConfiguration.cs
?
??? Clinica/
?   ??? CitaConfiguration.cs
?   ??? SalaConfiguration.cs
?   ??? CitaRecordatorioConfiguration.cs
?   ??? CitaHistorialEstadoConfiguration.cs
?
??? HistorialClinico/
?   ??? HistorialClinicoConfigurations.cs  # Agrupa varias
?
??? Inventario/
?   ??? ItemInventarioConfiguration.cs
?   ??? LoteInventarioConfiguration.cs
?   ??? InventarioGeneralConfigurations.cs
?   ??? ReporteUsoConfigurations.cs
?
??? Donaciones/
?   ??? DonacionesConfigurations.cs
?
??? Servicios/
?   ??? ServiciosConfigurations.cs
?
??? Auditoria/
    ??? AuditoriaConfigurations.cs
```

**Cómo se utiliza**:
```csharp
// Ejemplo: UsuarioConfiguration.cs
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // Tabla
        builder.ToTable("Usuarios");
        
        // Primary Key
        builder.HasKey(u => u.Id);
        
        // Propiedades
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        // Índices
        builder.HasIndex(u => u.Email)
            .IsUnique();
        
        // Relaciones
        builder.HasMany(u => u.UsuarioRoles)
            .WithOne(ur => ur.Usuario)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

#### ?? Infrastructure/Data/Seeders/

**Qué contiene**: `DatabaseSeeder.cs`

**Para qué sirve**:
- Insertar datos iniciales necesarios para el sistema
- Crear roles predefinidos
- Crear usuario administrador por defecto
- Datos de catálogos

**Cómo se utiliza**:
```csharp
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AdoPetsDbContext context)
    {
        // Seed roles
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Rol { Nombre = Roles.Admin, Descripcion = "Administrador del sistema" },
                new Rol { Nombre = Roles.Veterinario, Descripcion = "Médico veterinario" },
                new Rol { Nombre = Roles.Recepcionista, Descripcion = "Recepcionista" },
                new Rol { Nombre = Roles.Adoptante, Descripcion = "Usuario adoptante" }
            };
            
            await context.Roles.AddRangeAsync(roles);
            await context.SaveChangesAsync();
        }
    }
}

// Llamar en Program.cs al iniciar
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AdoPetsDbContext>();
await DatabaseSeeder.SeedAsync(context);
```

---

### ?? Infrastructure/Repositories/

**Qué contiene**: Implementaciones concretas de interfaces de repositorios.

**Archivos**:
- `UsuarioRepository.cs`
- `RolRepository.cs`

**Para qué sirve**:
- Implementar acceso a datos con Entity Framework
- Encapsular queries complejas
- Reutilización de lógica de acceso a datos

**Cómo se utiliza**:
```csharp
// UsuarioRepository.cs
public class UsuarioRepository : IUsuarioRepository
{
    private readonly AdoPetsDbContext _context;
    
    public UsuarioRepository(AdoPetsDbContext context)
    {
        _context = context;
    }
    
    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<List<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
            .Include(u => u.UsuarioRoles)
                .ThenInclude(ur => ur.Rol)
            .ToListAsync();
    }
    
    public async Task AddAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }
}
```

---

### ?? Infrastructure/Services/

**Qué contiene**: Implementaciones de servicios de aplicación.

**Archivos**:
- `AuthService.cs`: Lógica de autenticación
- `TokenService.cs`: Generación de JWT
- `PasswordHasher.cs`: Hashing con BCrypt o PBKDF2
- `UsuarioService.cs`: CRUD de usuarios

**Para qué sirve**:
- Implementar casos de uso complejos
- Orquestar múltiples repositorios
- Aplicar lógica de negocio

**Cómo se utiliza**:
```csharp
// PasswordHasher.cs
public class PasswordHasher : IPasswordHasher
{
    public (string hash, string salt) Hash(string password)
    {
        // Generar salt aleatorio
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        
        // Hash con salt
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        
        return (hash, salt);
    }
    
    public bool Verify(string password, string hash, string salt)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}

// TokenService.cs
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    
    public string GenerateToken(Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            // Roles
            ...usuario.UsuarioRoles.Select(ur => 
                new Claim(ClaimTypes.Role, ur.Rol.Nombre))
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

---

### ?? Infrastructure/Extensions/

**Qué contiene**: `ServiceCollectionExtensions.cs`

**Para qué sirve**:
- Registrar todos los servicios de infraestructura en DI
- Configurar Entity Framework
- Configurar autenticación JWT
- Mantener Program.cs limpio

**Cómo se utiliza**:
```csharp
// ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<AdoPetsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        
        // Servicios
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        
        // JWT
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => { /* config */ });
        
        return services;
    }
}

// Program.cs
builder.Services.AddInfrastructure(builder.Configuration);
```

---

### ?? Infrastructure/Configuration/

**Qué contiene**: `AppSettings.cs`

**Para qué sirve**:
- Clases fuertemente tipadas para configuración
- Mejor IntelliSense y type safety
- Validación de configuración al inicio

**Cómo se utiliza**:
```csharp
public class AppSettings
{
    public JwtSettings Jwt { get; set; }
    public PayPalSettings PayPal { get; set; }
}

public class JwtSettings
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationHours { get; set; }
}

// Program.cs
builder.Services.Configure<AppSettings>(builder.Configuration);

// Usar en servicio
public class TokenService
{
    private readonly AppSettings _settings;
    
    public TokenService(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }
}
```

---

## Capa de Presentación (Controllers)

**Ubicación**: `Controllers/`

**Propósito**: Exponer endpoints de la API REST. Recibe requests HTTP, valida, llama a servicios, y retorna responses.

### Archivos

#### **AuthController.cs**

**Endpoints**:
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/refresh-token` - Renovar token
- `POST /api/auth/change-password` - Cambiar contraseña

**Para qué sirve**: Autenticación y autorización.

---

#### **UsuariosController.cs**

**Endpoints**:
- `GET /api/usuarios` - Listar usuarios
- `GET /api/usuarios/{id}` - Obtener usuario por ID
- `POST /api/usuarios` - Crear usuario
- `PUT /api/usuarios/{id}` - Actualizar usuario
- `DELETE /api/usuarios/{id}` - Eliminar usuario

**Para qué sirve**: CRUD de usuarios.

---

#### **HealthController.cs**

**Endpoints**:
- `GET /api/health` - Health check

**Para qué sirve**: 
- Monitoreo de estado de la aplicación
- Load balancers y orquestadores (Kubernetes)
- Verificar conectividad a base de datos

**Cómo se utiliza**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    
    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }
    
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<List<UsuarioListDto>>> GetAll()
    {
        var usuarios = await _usuarioService.GetAllAsync();
        return Ok(usuarios);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDetailDto>> GetById(Guid id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        
        if (usuario == null)
            return NotFound();
        
        return Ok(usuario);
    }
    
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UsuarioDetailDto>> Create([FromBody] CreateUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var usuario = await _usuarioService.CreateAsync(dto);
        
        return CreatedAtAction(
            nameof(GetById), 
            new { id = usuario.Id }, 
            usuario
        );
    }
}
```

---

## Migraciones (Migrations)

**Ubicación**: `Migrations/`

**Qué contiene**:
- `20251021034046_InitialCreate.cs`: Migración inicial
- `20251021034046_InitialCreate.Designer.cs`: Snapshot de la migración
- `AdoPetsDbContextModelSnapshot.cs`: Estado actual del modelo

**Para qué sirve**:
- Versionamiento de esquema de base de datos
- Aplicar cambios incrementales
- Rollback si es necesario
- Sincronización entre ambientes

**Cómo se utiliza**:
```bash
# Crear nueva migración
dotnet ef migrations add NombreMigracion

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a migración específica
dotnet ef database update NombreMigracionAnterior

# Generar script SQL
dotnet ef migrations script
```

**Flujo de trabajo**:
1. Modificar entidad o configuración
2. Crear migración: `dotnet ef migrations add AgregarCampoX`
3. Revisar el código generado en `Migrations/`
4. Aplicar: `dotnet ef database update`

---

## Configuración y Archivos Raíz

### **Program.cs**

**Para qué sirve**: Punto de entrada de la aplicación ASP.NET Core.

**Qué hace**:
- Configura servicios (DI container)
- Configura middleware pipeline
- Inicializa base de datos
- Ejecuta seeders

**Estructura típica**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// ===== Configuración de Servicios =====

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure (DbContext, Repositories, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// CORS
builder.Services.AddCors(options => { /* ... */ });

var app = builder.Build();

// ===== Configuración de Middleware =====

// Swagger (solo en desarrollo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== Inicialización =====

// Aplicar migraciones y seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AdoPetsDbContext>();
    await context.Database.MigrateAsync(); // Aplicar migraciones pendientes
    await DatabaseSeeder.SeedAsync(context); // Insertar datos iniciales
}

app.Run();
```

---

### **AdoPetsBKD.csproj**

**Para qué sirve**: Archivo de proyecto .NET.

**Qué contiene**:
- TargetFramework (.NET 9)
- PackageReferences (NuGet packages)
- ProjectReferences (referencias a otros proyectos)

**Packages típicos**:
```xml
<ItemGroup>
  <!-- Entity Framework Core -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
  
  <!-- Authentication -->
  <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.0" />
  
  <!-- Password Hashing -->
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  
  <!-- Swagger -->
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
</ItemGroup>
```

---

## Convenciones de Nomenclatura

### Carpetas
- **PascalCase**: `Domain/`, `Application/`, `Infrastructure/`
- Nombres en **singular** para conceptos: `Common/`, `Configuration/`
- Nombres en **plural** para colecciones: `Entities/`, `Controllers/`, `Migrations/`

### Archivos
- **PascalCase** siempre: `Usuario.cs`, `AuthController.cs`
- Sufijos descriptivos:
  - Entidades: sin sufijo (`Usuario`)
  - Controladores: `Controller` (`UsuariosController`)
  - Servicios: `Service` (`AuthService`)
  - Repositorios: `Repository` (`UsuarioRepository`)
  - Interfaces: prefijo `I` (`IUsuarioService`)
  - DTOs: sufijo `Dto` (`LoginRequestDto`)
  - Configuraciones EF: sufijo `Configuration` (`UsuarioConfiguration`)

### Clases y Métodos
- **PascalCase**: `public class Usuario`, `public async Task<Usuario> GetByIdAsync()`
- Métodos async: sufijo `Async`
- Booleanos: prefijo `Is`, `Has`, `Can` (`IsDeleted`, `HasPermission`)

---

## Flujo de Datos

### Request ? Response

```
1. HTTP Request
   ?
2. Controller
   - Recibe request
   - Valida modelo (ModelState)
   - Llama a Service
   ?
3. Service (Application Layer)
   - Implementa caso de uso
   - Valida reglas de negocio
   - Llama a Repository(ies)
   ?
4. Repository (Infrastructure Layer)
   - Consulta base de datos con EF Core
   - Retorna entidades de dominio
   ?
5. Service
   - Mapea entidad ? DTO
   - Retorna DTO
   ?
6. Controller
   - Envuelve en ApiResponse
   - Retorna HTTP 200/400/404/etc.
   ?
7. HTTP Response (JSON)
```

### Ejemplo Completo: Login

```csharp
// 1. Request HTTP
POST /api/auth/login
{ "email": "user@example.com", "password": "Pass123!" }

// 2. Controller
[HttpPost("login")]
public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
{
    var response = await _authService.LoginAsync(request);
    return Ok(response);
}

// 3. Service
public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
{
    // 4. Repository
    var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
    
    if (usuario == null)
        throw new NotFoundException("Usuario no encontrado");
    
    // Validar password
    if (!_passwordHasher.Verify(request.Password, usuario.PasswordHash, usuario.PasswordSalt))
        throw new UnauthorizedException("Credenciales inválidas");
    
    // Generar token
    var token = _tokenService.GenerateToken(usuario);
    var refreshToken = _tokenService.GenerateRefreshToken();
    
    // Actualizar último acceso
    usuario.RegistrarAcceso();
    await _usuarioRepository.UpdateAsync(usuario);
    
    // 5. Mapear a DTO
    return new LoginResponseDto
    {
        Token = token,
        RefreshToken = refreshToken,
        Usuario = MapToUsuarioDto(usuario)
    };
}

// 6. Response HTTP
200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "8f7d6e5c4b3a2...",
  "usuario": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "nombreCompleto": "Juan Pérez García"
  }
}
```

---

## Resumen de Propósitos por Capa

| Capa | Responsabilidad | Ejemplo |
|------|----------------|---------|
| **Domain** | Lógica de negocio pura, entidades, reglas | `Mascota.EstaDisponibleParaAdopcion()` |
| **Application** | Casos de uso, orquestación | `AuthService.LoginAsync()` |
| **Infrastructure** | Detalles técnicos, BD, APIs externas | `UsuarioRepository.GetByEmailAsync()` |
| **Presentation** | API REST, recibir/enviar HTTP | `AuthController.Login()` |

---

## Beneficios de Esta Estructura

### ? Separación de Responsabilidades
Cada capa tiene un propósito claro, fácil de entender y modificar.

### ? Testabilidad
Interfaces permiten crear mocks para unit tests sin base de datos real.

### ? Mantenibilidad
Cambios en una capa no afectan a las demás (bajo acoplamiento).

### ? Escalabilidad
Fácil añadir nuevos módulos siguiendo la misma estructura.

### ? Independencia de Framework
La lógica de negocio (Domain) no depende de EF Core, ASP.NET, etc.

### ? Reutilización
Servicios y repositorios se pueden usar desde diferentes controllers o jobs en background.

---

## Próximos Pasos

### Añadir Nuevo Módulo (ej: Pagos)

1. **Domain**: Crear `Domain/Entities/Pagos/Pago.cs`
2. **Application**: 
   - Crear DTOs en `Application/DTOs/Pagos/`
   - Crear interfaces en `Application/Interfaces/`
3. **Infrastructure**:
   - Crear configuración EF en `Infrastructure/Data/Configurations/Pagos/`
   - Crear repositorio en `Infrastructure/Repositories/`
   - Crear servicio en `Infrastructure/Services/`
4. **Presentation**: Crear `Controllers/PagosController.cs`
5. **Migración**: `dotnet ef migrations add AgregarPagos`

### Añadir Nuevo Endpoint

1. Definir DTO de request/response en `Application/DTOs/`
2. Añadir método en servicio existente o crear nuevo
3. Añadir endpoint en controller
4. Documentar en Swagger (atributos XML comments)

---

## Conclusión

La estructura del proyecto **AdoPets Backend** sigue las mejores prácticas de Clean Architecture y DDD, resultando en un código:

- ?? **Modular**: Fácil añadir/quitar funcionalidades
- ?? **Testeable**: Interfaces permiten mocks
- ?? **Mantenible**: Bajo acoplamiento, alta cohesión
- ?? **Escalable**: Preparado para crecer
- ?? **Autodocumentado**: Estructura clara y predecible

Cada carpeta y archivo tiene un propósito específico, facilitando la navegación y comprensión tanto para nuevos desarrolladores como para el mantenimiento futuro del sistema.
