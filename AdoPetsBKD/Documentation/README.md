# AdoPets Backend - Configuraci�n Base

## ?? Descripci�n
Backend API para la plataforma AdoPets - Sistema integral de gesti�n de refugio de animales con cl�nica veterinaria.

## ??? Stack Tecnol�gico
- **.NET 9**
- **Entity Framework Core 9**
- **SQL Server**
- **JWT Bearer Authentication**
- **Swagger/OpenAPI**

## ?? Configuraci�n Inicial

### 1. Prerrequisitos
- .NET 9 SDK
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 o Visual Studio Code

### 2. Configuraci�n de Base de Datos

La cadena de conexi�n est� configurada en `appsettings.json` y `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AdoPetsDb": "Server=localhost;Database=AdoPetsDb_Dev;User Id=sa;Password=Uucy291o;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Crear la Base de Datos

#### Opci�n 1: Usando Migraciones de EF Core

```bash
# Agregar migraci�n inicial
dotnet ef migrations add InitialCreate

# Aplicar migraciones
dotnet ef database update
```

#### Opci�n 2: Autom�ticamente al ejecutar
El proyecto est� configurado para aplicar migraciones autom�ticamente en modo desarrollo al iniciar.

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

- **Swagger UI**: `https://localhost:<puerto>/` (configurado como ruta ra�z en desarrollo)
- **Health Check**: `https://localhost:<puerto>/api/v1/health`
- **Ping**: `https://localhost:<puerto>/api/v1/health/ping`

## ?? Estructura del Proyecto

```
AdoPetsBKD/
??? Controllers/              # Controladores de API
?   ??? HealthController.cs   # Controlador de prueba
??? Infrastructure/
?   ??? Configuration/        # Clases de configuraci�n
?   ?   ??? AppSettings.cs    # Settings del app
?   ??? Data/                 # Contexto de EF Core
?       ??? AdoPetsDbContext.cs
??? appsettings.json          # Configuraci�n general
??? appsettings.Development.json  # Configuraci�n de desarrollo
??? Program.cs                # Punto de entrada de la aplicaci�n
```

## ?? Seguridad

### JWT Configuration
El proyecto est� configurado con autenticaci�n JWT. Los tokens deben incluirse en el header:

```
Authorization: Bearer <token>
```

### Pol�ticas de Autorizaci�n
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
En producci�n: 100 peticiones por minuto

### Logging
- Desarrollo: Nivel `Debug` con SQL queries
- Producci�n: Nivel `Information`

## ?? Pr�ximos Pasos

1. ? Configuraci�n base completada
2. ?? Crear entidades del dominio (Usuario, Mascota, Cita, etc.)
3. ?? Implementar repositorios
4. ?? Implementar casos de uso (Application Layer)
5. ?? Crear controladores de API
6. ?? Implementar autenticaci�n completa
7. ?? Integrar servicios externos (Azure Blob, PayPal, FCM)

## ?? Documentaci�n Adicional

Ver `documentacionAdoPets.md` para el dise�o completo del sistema.

## ?? Notas Importantes

- **NO** usar las credenciales de desarrollo en producci�n
- Cambiar `Jwt:SecretKey` antes de desplegar
- Las migraciones se aplican autom�ticamente en desarrollo
- Revisar los logs en caso de error de conexi�n a BD

## ?? Contribuci�n

Este proyecto sigue Clean Architecture con CQRS light y vertical slices.

---
**AdoPets Team** - Refugio de Animales con Cl�nica Veterinaria
