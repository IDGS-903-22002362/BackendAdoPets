# AdoPets Backend - Configuración Base

## ?? Descripción
Backend API para la plataforma AdoPets - Sistema integral de gestión de refugio de animales con clínica veterinaria.

## ??? Stack Tecnológico
- **.NET 9**
- **Entity Framework Core 9**
- **SQL Server**
- **JWT Bearer Authentication**
- **Swagger/OpenAPI**

## ?? Configuración Inicial

### 1. Prerrequisitos
- .NET 9 SDK
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 o Visual Studio Code

### 2. Configuración de Base de Datos

La cadena de conexión está configurada en `appsettings.json` y `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AdoPetsDb": "Server=localhost;Database=AdoPetsDb_Dev;User Id=sa;Password=Uucy291o;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Crear la Base de Datos

#### Opción 1: Usando Migraciones de EF Core

```bash
# Agregar migración inicial
dotnet ef migrations add InitialCreate

# Aplicar migraciones
dotnet ef database update
```

#### Opción 2: Automáticamente al ejecutar
El proyecto está configurado para aplicar migraciones automáticamente en modo desarrollo al iniciar.

### 4. Ejecutar el Proyecto

```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

O desde Visual Studio: `F5` o `Ctrl+F5`

### 5. Acceder a la API

- **Swagger UI**: `https://localhost:<puerto>/` (configurado como ruta raíz en desarrollo)
- **Health Check**: `https://localhost:<puerto>/api/v1/health`
- **Ping**: `https://localhost:<puerto>/api/v1/health/ping`

## ?? Estructura del Proyecto

```
AdoPetsBKD/
??? Controllers/              # Controladores de API
?   ??? HealthController.cs   # Controlador de prueba
??? Infrastructure/
?   ??? Configuration/        # Clases de configuración
?   ?   ??? AppSettings.cs    # Settings del app
?   ??? Data/                 # Contexto de EF Core
?       ??? AdoPetsDbContext.cs
??? appsettings.json          # Configuración general
??? appsettings.Development.json  # Configuración de desarrollo
??? Program.cs                # Punto de entrada de la aplicación
```

## ?? Seguridad

### JWT Configuration
El proyecto está configurado con autenticación JWT. Los tokens deben incluirse en el header:

```
Authorization: Bearer <token>
```

### Políticas de Autorización
- `AdminOnly`: Solo administradores
- `VetOnly`: Veterinarios y administradores
- `StaffOnly`: Todo el personal del refugio

## ?? Testing

### Health Check
```bash
curl https://localhost:<puerto>/api/v1/health
```

Respuesta esperada:
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-01T00:00:00Z",
  "version": "1.0.0",
  "environment": "Development"
}
```

## ?? Configuraciones Importantes

### CORS
Configurado para permitir peticiones desde:
- `http://localhost:3000` (React)
- `http://localhost:5173` (Vite)
- `http://localhost:8080` (desarrollo)

### Rate Limiting
En desarrollo: 1000 peticiones por minuto
En producción: 100 peticiones por minuto

### Logging
- Desarrollo: Nivel `Debug` con SQL queries
- Producción: Nivel `Information`

## ?? Próximos Pasos

1. ? Configuración base completada
2. ?? Crear entidades del dominio (Usuario, Mascota, Cita, etc.)
3. ?? Implementar repositorios
4. ?? Implementar casos de uso (Application Layer)
5. ?? Crear controladores de API
6. ?? Implementar autenticación completa
7. ?? Integrar servicios externos (Azure Blob, PayPal, FCM)

## ?? Documentación Adicional

Ver `documentacionAdoPets.md` para el diseño completo del sistema.

## ?? Notas Importantes

- **NO** usar las credenciales de desarrollo en producción
- Cambiar `Jwt:SecretKey` antes de desplegar
- Las migraciones se aplican automáticamente en desarrollo
- Revisar los logs en caso de error de conexión a BD

## ?? Contribución

Este proyecto sigue Clean Architecture con CQRS light y vertical slices.

---
**AdoPets Team** - Refugio de Animales con Clínica Veterinaria
