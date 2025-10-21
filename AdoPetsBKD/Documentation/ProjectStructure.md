# Documentaci�n de la Estructura del Proyecto - AdoPets Backend

## �ndice
1. [Visi�n General de la Arquitectura](#visi�n-general-de-la-arquitectura)
2. [Estructura de Carpetas](#estructura-de-carpetas)
3. [Capa de Dominio (Domain)](#capa-de-dominio-domain)
4. [Capa de Aplicaci�n (Application)](#capa-de-aplicaci�n-application)
5. [Capa de Infraestructura (Infrastructure)](#capa-de-infraestructura-infrastructure)
6. [Capa de Presentaci�n (Controllers)](#capa-de-presentaci�n-controllers)
7. [Migraciones (Migrations)](#migraciones-migrations)
8. [Configuraci�n y Archivos Ra�z](#configuraci�n-y-archivos-ra�z)
9. [Convenciones de Nomenclatura](#convenciones-de-nomenclatura)
10. [Flujo de Datos](#flujo-de-datos)

---

## Visi�n General de la Arquitectura

El proyecto **AdoPets Backend** est� construido siguiendo los principios de **Clean Architecture** (Arquitectura Limpia) y **Domain-Driven Design (DDD)**, lo que permite:

- ? Separaci�n clara de responsabilidades
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
?          Domain Layer                   ?  ? Entidades, L�gica de Negocio
?  (Entities, Value Objects, Domain Logic)?
???????????????????????????????????????????
              ?
???????????????????????????????????????????
?      Infrastructure Layer               ?  ? EF Core, Servicios Externos
?   (Data Access, External Services)      ?
???????????????????????????????????????????
```

**Reglas de dependencia**:
- ? **Domain** NO depende de nada (n�cleo independiente)
- ? **Application** depende solo de **Domain**
- ? **Infrastructure** depende de **Domain** y **Application**
- ? **Controllers** dependen de **Application**

---

## Estructura de Carpetas

```
AdoPetsBKD/
??? ?? Domain/                    # Capa de Dominio (n�cleo del negocio)
?   ??? ?? Common/                # Clases base compartidas
?   ??? ?? Entities/              # Entidades de negocio organizadas por m�dulo
?       ??? ?? Security/          # Usuarios, Roles, Autenticaci�n
?       ??? ?? Mascotas/          # Mascotas, Adopciones
?       ??? ?? Clinica/           # Citas, Salas
?       ??? ?? HistorialClinico/  # Expedientes, Vacunas, Cirug�as
?       ??? ?? Inventario/        # Items, Lotes, Movimientos
?       ??? ?? Donaciones/        # Donaciones PayPal
?       ??? ?? Servicios/         # Empleados, Especialidades
?       ??? ?? Auditoria/         # Logs, Eventos
?
??? ?? Application/               # Capa de Aplicaci�n (casos de uso)
?   ??? ?? DTOs/                  # Data Transfer Objects
?   ?   ??? ?? Auth/              # DTOs de autenticaci�n
?   ?   ??? ?? Usuarios/          # DTOs de usuarios
?   ??? ?? Interfaces/            # Contratos/interfaces
?   ?   ??? ?? Repositories/      # Interfaces de repositorios
?   ?   ??? ?? Services/          # Interfaces de servicios
?   ??? ?? Common/                # Clases comunes de aplicaci�n
?
??? ?? Infrastructure/            # Capa de Infraestructura (implementaciones)
?   ??? ?? Data/                  # Acceso a datos con EF Core
?   ?   ??? ?? Configurations/    # Configuraciones de entidades EF
?   ?   ?   ??? ?? Security/      # Configs de entidades de seguridad
?   ?   ?   ??? ?? Mascotas/      # Configs de entidades de mascotas
?   ?   ?   ??? ?? Clinica/       # Configs de entidades de cl�nica
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
??? ?? Controllers/               # Capa de Presentaci�n (API REST)
?   ??? AuthController.cs         # Endpoints de autenticaci�n
?   ??? UsuariosController.cs     # Endpoints de usuarios
?   ??? HealthController.cs       # Health checks
?
??? ?? Migrations/                # Migraciones de Entity Framework
?
??? ?? Documentation/             # Documentaci�n del proyecto
?   ??? DatabaseDesign.md         # Dise�o de base de datos
?   ??? ProjectStructure.md       # Este archivo
?
??? Program.cs                    # Punto de entrada de la aplicaci�n
??? AdoPetsBKD.csproj            # Archivo de proyecto .NET
```

---

## Capa de Dominio (Domain)

**Ubicaci�n**: `Domain/`

**Prop�sito**: Contiene la l�gica de negocio central, entidades y reglas de dominio. Es el coraz�n del sistema y **NO debe depender de ninguna otra capa**.

### ?? Domain/Common/

**Archivo**: `BaseEntity.cs`

**Qu� contiene**:
- `BaseEntity`: Clase base con `Id: Guid`
- `AuditableEntity`: A�ade timestamps de auditor�a
- `SoftDeletableEntity`: A�ade soft delete

**Para qu� sirve**:
- Evitar repetici�n de c�digo com�n
- Garantizar que todas las entidades tengan ID �nico
- Implementar auditor�a autom�tica en SaveChanges
- Permitir eliminaci�n l�gica sin perder datos

**C�mo se utiliza**:
```csharp
// Entidad simple
public class Rol : BaseEntity { }

// Entidad con auditor�a
public class Usuario : AuditableEntity { }

// Entidad con soft delete
public class Mascota : SoftDeletableEntity { }
```

---

### ?? Domain/Entities/

Organizadas por **m�dulos de negocio** (bounded contexts de DDD).

#### ?? Domain/Entities/Security/

**Qu� contiene**:
- `Usuario.cs`: Entidad principal de usuarios
- `Rol.cs`: Roles del sistema (Admin, Veterinario, etc.)
- `UsuarioRol.cs`: Relaci�n N:M entre usuarios y roles
- `Dispositivo.cs`: Dispositivos registrados para notificaciones
- `Notificacion.cs`: Notificaciones in-app y push
- `PreferenciaNotificacion.cs`: Configuraci�n de notificaciones por usuario
- `ConsentimientoPolitica.cs`: Historial de aceptaci�n de pol�ticas

**Para qu� sirve**:
- Gesti�n de identidad y acceso (IAM)
- Control de autorizaci�n basado en roles (RBAC)
- Sistema de notificaciones
- Cumplimiento legal (GDPR, LFPDPPP)

**C�mo se utiliza**:
```csharp
// Crear usuario con rol
var usuario = new Usuario { Nombre = "Juan", Email = "juan@example.com" };
var rol = await _context.Roles.FirstAsync(r => r.Nombre == Roles.Veterinario);
usuario.UsuarioRoles.Add(new UsuarioRol { RolId = rol.Id });
```

---

#### ?? Domain/Entities/Mascotas/

**Qu� contiene**:
- `Mascota.cs`: Entidad principal de mascotas
- `MascotaFoto.cs`: Galer�a de fotos
- `SolicitudAdopcion.cs`: Solicitudes de adopci�n
- `SolicitudAdopcionAdjunto.cs`: Documentos de la solicitud
- `AdopcionLog.cs`: Historial de cambios de estado

**Para qu� sirve**:
- Gesti�n completa de mascotas del refugio
- Proceso de adopci�n con evaluaci�n
- Tracking de estado de cada mascota
- Galer�a multimedia

**C�mo se utiliza**:
```csharp
// Mascota disponible para adopci�n
var mascota = new Mascota 
{
    Nombre = "Firulais",
    Especie = "Perro",
    Estatus = EstatusMascota.Disponible
};

// Solicitar adopci�n
var solicitud = new SolicitudAdopcion
{
    UsuarioId = adoptanteId,
    MascotaId = mascota.Id,
    Vivienda = TipoVivienda.Casa,
    MotivoAdopcion = "Busco compa��a"
};
```

---

#### ?? Domain/Entities/Clinica/

**Qu� contiene**:
- `Cita.cs`: Citas veterinarias
- `Sala.cs`: Salas de consulta/cirug�a
- `CitaRecordatorio.cs`: Recordatorios autom�ticos
- `CitaHistorialEstado.cs`: Trazabilidad de cambios

**Para qu� sirve**:
- Sistema de agenda veterinaria
- Gesti�n de espacios f�sicos
- Prevenci�n de conflictos de horario
- Recordatorios autom�ticos para reducir no-shows

**C�mo se utiliza**:
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

**Qu� contiene**:
- `Expediente.cs`: Notas cl�nicas (SOAP)
- `Vacunacion.cs`: Registro de vacunas
- `Desparasitacion.cs`: Control de desparasitaciones
- `Cirugia.cs`: Procedimientos quir�rgicos
- `Valoracion.cs`: Signos vitales
- `AdjuntoMedico.cs`: Im�genes m�dicas (rayos X, etc.)

**Para qu� sirve**:
- Historial m�dico completo de cada mascota
- Trazabilidad de tratamientos
- Cumplimiento de protocolos veterinarios
- Seguimiento de salud a largo plazo

**C�mo se utiliza**:
```csharp
// Crear expediente de consulta
var expediente = new Expediente
{
    MascotaId = mascotaId,
    VeterinarioId = veterinarioId,
    CitaId = citaId,
    MotivoConsulta = "Cojera en pata trasera",
    Diagnostico = "Esguince grado I",
    Tratamiento = "Reposo 7 d�as + antiinflamatorio"
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

**Qu� contiene**:
- `ItemInventario.cs`: Cat�logo de insumos
- `LoteInventario.cs`: Lotes con fechas de vencimiento
- `MovimientoInventario.cs`: Entradas/Salidas
- `AlertaInventario.cs`: Alertas autom�ticas
- `Proveedor.cs`: Proveedores
- `Compra.cs`: �rdenes de compra
- `DetalleCompra.cs`: L�neas de compra
- `ReporteUsoInsumos.cs`: Reporte post-procedimiento
- `ReporteUsoInsumoDetalle.cs`: Items usados
- `ReporteUsoSplitLote.cs`: Tracking FIFO de lotes

**Para qu� sirve**:
- Control de inventario m�dico con FIFO autom�tico
- Trazabilidad total de lotes (retirada de defectuosos)
- Gesti�n de compras a proveedores
- Alertas de stock bajo y pr�ximos a vencer
- Costeo de procedimientos

**C�mo se utiliza**:
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
    QtyUsada = 50 // 50ml de antibi�tico
});

// El sistema autom�ticamente:
// 1. Busca lotes con fecha de vencimiento m�s pr�xima (FIFO)
// 2. Descuenta stock de m�ltiples lotes si es necesario
// 3. Crea registros en ReporteUsoSplitLote con trazabilidad exacta
// 4. Genera MovimientoInventario de salida
```

---

#### ?? Domain/Entities/Donaciones/

**Qu� contiene**:
- `Donacion.cs`: Donaciones v�a PayPal
- `WebhookEvent.cs`: Log de webhooks de PayPal

**Para qu� sirve**:
- Recepci�n de donaciones monetarias
- Integraci�n con PayPal (�rdenes y capturas)
- Idempotencia de webhooks
- Reportes de recaudaci�n

**C�mo se utiliza**:
```csharp
// Crear donaci�n pendiente
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

**Qu� contiene**:
- `Empleado.cs`: Informaci�n laboral de empleados
- `Especialidad.cs`: Especialidades veterinarias
- `EmpleadoEspecialidad.cs`: Relaci�n N:M
- `Servicio.cs`: Cat�logo de servicios
- `Horario.cs`: Turnos de trabajo

**Para qu� sirve**:
- Gesti�n de recursos humanos
- Asignaci�n inteligente de citas seg�n especialidad
- Control de horarios laborales
- Cat�logo de servicios ofrecidos

**C�mo se utiliza**:
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

**Qu� contiene**:
- `AuditLog.cs`: Log de acciones cr�ticas
- `OutboxEvent.cs`: Eventos para publicar (patr�n outbox)
- `JobProgramado.cs`: Configuraci�n de jobs

**Para qu� sirve**:
- Auditor�a completa de operaciones sensibles
- Mensajer�a confiable con patr�n outbox
- Gesti�n de tareas programadas (cron jobs)
- Cumplimiento regulatorio

**C�mo se utiliza**:
```csharp
// Log de auditor�a
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

## Capa de Aplicaci�n (Application)

**Ubicaci�n**: `Application/`

**Prop�sito**: Define los **casos de uso** de la aplicaci�n, orquesta la l�gica de dominio y define contratos (interfaces). Es la capa que coordina las operaciones.

### ?? Application/DTOs/

**Qu� contiene**: Data Transfer Objects - objetos para transferir datos entre capas.

**Para qu� sirve**:
- Desacoplar la API de las entidades de dominio
- Controlar exactamente qu� datos se exponen
- Validaciones con DataAnnotations
- Modelar requests y responses espec�ficos

**Estructura**:
```
DTOs/
??? Auth/                    # DTOs de autenticaci�n
?   ??? LoginRequestDto.cs   # { Email, Password }
?   ??? LoginResponseDto.cs  # { Token, RefreshToken, Usuario }
?   ??? RegisterRequestDto.cs
?   ??? RefreshTokenRequestDto.cs
?   ??? ChangePasswordRequestDto.cs
?   ??? UsuarioDto.cs        # DTO de usuario sin datos sensibles
?
??? Usuarios/                # DTOs de gesti�n de usuarios
    ??? CreateUsuarioDto.cs
    ??? UpdateUsuarioDto.cs
    ??? UsuarioListDto.cs    # Para listados (datos m�nimos)
    ??? UsuarioDetailDto.cs  # Para detalle completo
```

**C�mo se utiliza**:
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

**Qu� contiene**: Contratos (interfaces) que definen qu� puede hacer el sistema.

#### ?? Application/Interfaces/Repositories/

**Qu� contiene**:
- `IUsuarioRepository.cs`
- `IRolRepository.cs`
- etc.

**Para qu� sirve**:
- **Inversi�n de dependencias**: Application define el contrato, Infrastructure lo implementa
- Facilita testing con mocks
- Permite cambiar implementaci�n sin afectar casos de uso

**C�mo se utiliza**:
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
        _repository = repository; // Inyecci�n de dependencia
    }
}
```

---

#### ?? Application/Interfaces/Services/

**Qu� contiene**:
- `IAuthService.cs`: Autenticaci�n y autorizaci�n
- `ITokenService.cs`: Generaci�n de JWT
- `IPasswordHasher.cs`: Hashing de passwords
- `IUsuarioService.cs`: L�gica de usuarios

**Para qu� sirve**:
- Definir operaciones de negocio de alto nivel
- Separar l�gica de aplicaci�n de implementaci�n
- Testability

**C�mo se utiliza**:
```csharp
// Interface define qu� hace
public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequestDto request);
}

// Service implementa c�mo lo hace
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

**Qu� contiene**: Clases comunes de la capa de aplicaci�n.

**Archivo**: `ApiResponse.cs`

**Para qu� sirve**:
- Estandarizar formato de respuestas de la API
- Manejar errores de forma consistente

**C�mo se utiliza**:
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

**Ubicaci�n**: `Infrastructure/`

**Prop�sito**: Implementa los detalles t�cnicos: acceso a datos, servicios externos, configuraciones. **Depende** de Domain y Application.

### ?? Infrastructure/Data/

**Qu� contiene**: Todo relacionado con Entity Framework Core y acceso a base de datos.

#### **AdoPetsDbContext.cs**

**Para qu� sirve**:
- Contexto principal de Entity Framework Core
- Define DbSets (tablas)
- Aplica configuraciones de entidades
- Intercepta SaveChanges para auditor�a autom�tica

**C�mo se utiliza**:
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
    
    // Auditor�a autom�tica
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

**Qu� contiene**: Configuraciones de Entity Framework (Fluent API) organizadas por m�dulo.

**Para qu� sirve**:
- Configurar relaciones entre entidades
- Definir �ndices, restricciones, longitudes
- Separar configuraci�n de entidades (no contaminar entidades con atributos)

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

**C�mo se utiliza**:
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
        
        // �ndices
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

**Qu� contiene**: `DatabaseSeeder.cs`

**Para qu� sirve**:
- Insertar datos iniciales necesarios para el sistema
- Crear roles predefinidos
- Crear usuario administrador por defecto
- Datos de cat�logos

**C�mo se utiliza**:
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
                new Rol { Nombre = Roles.Veterinario, Descripcion = "M�dico veterinario" },
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

**Qu� contiene**: Implementaciones concretas de interfaces de repositorios.

**Archivos**:
- `UsuarioRepository.cs`
- `RolRepository.cs`

**Para qu� sirve**:
- Implementar acceso a datos con Entity Framework
- Encapsular queries complejas
- Reutilizaci�n de l�gica de acceso a datos

**C�mo se utiliza**:
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

**Qu� contiene**: Implementaciones de servicios de aplicaci�n.

**Archivos**:
- `AuthService.cs`: L�gica de autenticaci�n
- `TokenService.cs`: Generaci�n de JWT
- `PasswordHasher.cs`: Hashing con BCrypt o PBKDF2
- `UsuarioService.cs`: CRUD de usuarios

**Para qu� sirve**:
- Implementar casos de uso complejos
- Orquestar m�ltiples repositorios
- Aplicar l�gica de negocio

**C�mo se utiliza**:
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

**Qu� contiene**: `ServiceCollectionExtensions.cs`

**Para qu� sirve**:
- Registrar todos los servicios de infraestructura en DI
- Configurar Entity Framework
- Configurar autenticaci�n JWT
- Mantener Program.cs limpio

**C�mo se utiliza**:
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

**Qu� contiene**: `AppSettings.cs`

**Para qu� sirve**:
- Clases fuertemente tipadas para configuraci�n
- Mejor IntelliSense y type safety
- Validaci�n de configuraci�n al inicio

**C�mo se utiliza**:
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

## Capa de Presentaci�n (Controllers)

**Ubicaci�n**: `Controllers/`

**Prop�sito**: Exponer endpoints de la API REST. Recibe requests HTTP, valida, llama a servicios, y retorna responses.

### Archivos

#### **AuthController.cs**

**Endpoints**:
- `POST /api/auth/login` - Iniciar sesi�n
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/refresh-token` - Renovar token
- `POST /api/auth/change-password` - Cambiar contrase�a

**Para qu� sirve**: Autenticaci�n y autorizaci�n.

---

#### **UsuariosController.cs**

**Endpoints**:
- `GET /api/usuarios` - Listar usuarios
- `GET /api/usuarios/{id}` - Obtener usuario por ID
- `POST /api/usuarios` - Crear usuario
- `PUT /api/usuarios/{id}` - Actualizar usuario
- `DELETE /api/usuarios/{id}` - Eliminar usuario

**Para qu� sirve**: CRUD de usuarios.

---

#### **HealthController.cs**

**Endpoints**:
- `GET /api/health` - Health check

**Para qu� sirve**: 
- Monitoreo de estado de la aplicaci�n
- Load balancers y orquestadores (Kubernetes)
- Verificar conectividad a base de datos

**C�mo se utiliza**:
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

**Ubicaci�n**: `Migrations/`

**Qu� contiene**:
- `20251021034046_InitialCreate.cs`: Migraci�n inicial
- `20251021034046_InitialCreate.Designer.cs`: Snapshot de la migraci�n
- `AdoPetsDbContextModelSnapshot.cs`: Estado actual del modelo

**Para qu� sirve**:
- Versionamiento de esquema de base de datos
- Aplicar cambios incrementales
- Rollback si es necesario
- Sincronizaci�n entre ambientes

**C�mo se utiliza**:
```bash
# Crear nueva migraci�n
dotnet ef migrations add NombreMigracion

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a migraci�n espec�fica
dotnet ef database update NombreMigracionAnterior

# Generar script SQL
dotnet ef migrations script
```

**Flujo de trabajo**:
1. Modificar entidad o configuraci�n
2. Crear migraci�n: `dotnet ef migrations add AgregarCampoX`
3. Revisar el c�digo generado en `Migrations/`
4. Aplicar: `dotnet ef database update`

---

## Configuraci�n y Archivos Ra�z

### **Program.cs**

**Para qu� sirve**: Punto de entrada de la aplicaci�n ASP.NET Core.

**Qu� hace**:
- Configura servicios (DI container)
- Configura middleware pipeline
- Inicializa base de datos
- Ejecuta seeders

**Estructura t�pica**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// ===== Configuraci�n de Servicios =====

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

// ===== Configuraci�n de Middleware =====

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

// ===== Inicializaci�n =====

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

**Para qu� sirve**: Archivo de proyecto .NET.

**Qu� contiene**:
- TargetFramework (.NET 9)
- PackageReferences (NuGet packages)
- ProjectReferences (referencias a otros proyectos)

**Packages t�picos**:
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

### Clases y M�todos
- **PascalCase**: `public class Usuario`, `public async Task<Usuario> GetByIdAsync()`
- M�todos async: sufijo `Async`
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
        throw new UnauthorizedException("Credenciales inv�lidas");
    
    // Generar token
    var token = _tokenService.GenerateToken(usuario);
    var refreshToken = _tokenService.GenerateRefreshToken();
    
    // Actualizar �ltimo acceso
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
    "nombreCompleto": "Juan P�rez Garc�a"
  }
}
```

---

## Resumen de Prop�sitos por Capa

| Capa | Responsabilidad | Ejemplo |
|------|----------------|---------|
| **Domain** | L�gica de negocio pura, entidades, reglas | `Mascota.EstaDisponibleParaAdopcion()` |
| **Application** | Casos de uso, orquestaci�n | `AuthService.LoginAsync()` |
| **Infrastructure** | Detalles t�cnicos, BD, APIs externas | `UsuarioRepository.GetByEmailAsync()` |
| **Presentation** | API REST, recibir/enviar HTTP | `AuthController.Login()` |

---

## Beneficios de Esta Estructura

### ? Separaci�n de Responsabilidades
Cada capa tiene un prop�sito claro, f�cil de entender y modificar.

### ? Testabilidad
Interfaces permiten crear mocks para unit tests sin base de datos real.

### ? Mantenibilidad
Cambios en una capa no afectan a las dem�s (bajo acoplamiento).

### ? Escalabilidad
F�cil a�adir nuevos m�dulos siguiendo la misma estructura.

### ? Independencia de Framework
La l�gica de negocio (Domain) no depende de EF Core, ASP.NET, etc.

### ? Reutilizaci�n
Servicios y repositorios se pueden usar desde diferentes controllers o jobs en background.

---

## Pr�ximos Pasos

### A�adir Nuevo M�dulo (ej: Pagos)

1. **Domain**: Crear `Domain/Entities/Pagos/Pago.cs`
2. **Application**: 
   - Crear DTOs en `Application/DTOs/Pagos/`
   - Crear interfaces en `Application/Interfaces/`
3. **Infrastructure**:
   - Crear configuraci�n EF en `Infrastructure/Data/Configurations/Pagos/`
   - Crear repositorio en `Infrastructure/Repositories/`
   - Crear servicio en `Infrastructure/Services/`
4. **Presentation**: Crear `Controllers/PagosController.cs`
5. **Migraci�n**: `dotnet ef migrations add AgregarPagos`

### A�adir Nuevo Endpoint

1. Definir DTO de request/response en `Application/DTOs/`
2. A�adir m�todo en servicio existente o crear nuevo
3. A�adir endpoint en controller
4. Documentar en Swagger (atributos XML comments)

---

## Conclusi�n

La estructura del proyecto **AdoPets Backend** sigue las mejores pr�cticas de Clean Architecture y DDD, resultando en un c�digo:

- ?? **Modular**: F�cil a�adir/quitar funcionalidades
- ?? **Testeable**: Interfaces permiten mocks
- ?? **Mantenible**: Bajo acoplamiento, alta cohesi�n
- ?? **Escalable**: Preparado para crecer
- ?? **Autodocumentado**: Estructura clara y predecible

Cada carpeta y archivo tiene un prop�sito espec�fico, facilitando la navegaci�n y comprensi�n tanto para nuevos desarrolladores como para el mantenimiento futuro del sistema.
