# API de Autenticaci�n y Gesti�n de Usuarios - AdoPets

## ?? Resumen

Se han creado las APIs/Endpoints para autenticaci�n y manejo de usuarios siguiendo las mejores pr�cticas de Clean Architecture y SOLID.

## ??? Arquitectura Implementada

### Capas Creadas:

1. **Application Layer**
   - DTOs (Data Transfer Objects)
   - Interfaces de Repositorios y Servicios

2. **Infrastructure Layer**
   - Implementaci�n de Repositorios
   - Implementaci�n de Servicios
   - Extensiones para DI

3. **API Layer**
   - Controladores REST

## ?? Estructura de Archivos Creados

```
AdoPetsBKD/
??? Application/
?   ??? Common/
?   ?   ??? ApiResponse.cs
?   ??? DTOs/
?   ?   ??? Auth/
?   ?   ?   ??? LoginRequestDto.cs
?   ?   ?   ??? LoginResponseDto.cs
?   ?   ?   ??? RegisterRequestDto.cs
?   ?   ?   ??? RefreshTokenRequestDto.cs
?   ?   ?   ??? ChangePasswordRequestDto.cs
?   ?   ?   ??? UsuarioDto.cs
?   ?   ??? Usuarios/
?   ?       ??? CreateUsuarioDto.cs
?   ?       ??? UpdateUsuarioDto.cs
?   ?       ??? UsuarioListDto.cs
?   ?       ??? UsuarioDetailDto.cs
?   ??? Interfaces/
?       ??? Repositories/
?       ?   ??? IUsuarioRepository.cs
?       ?   ??? IRolRepository.cs
?       ??? Services/
?           ??? IAuthService.cs
?           ??? IUsuarioService.cs
?           ??? ITokenService.cs
?           ??? IPasswordHasher.cs
??? Infrastructure/
?   ??? Extensions/
?   ?   ??? ServiceCollectionExtensions.cs
?   ??? Repositories/
?   ?   ??? UsuarioRepository.cs
?   ?   ??? RolRepository.cs
?   ??? Services/
?       ??? AuthService.cs
?       ??? UsuarioService.cs
?       ??? TokenService.cs
?       ??? PasswordHasher.cs
??? Controllers/
    ??? AuthController.cs
    ??? UsuariosController.cs
```

## ?? Configuraci�n Requerida

### 1. Agregar en Program.cs

Despu�s de `builder.Services.AddDbContext<AdoPetsDbContext>`, agregar:

```csharp
using AdoPetsBKD.Infrastructure.Extensions;

// ... c�digo existente ...

// Despu�s de Database Context, agregar:
builder.Services.AddApplicationServices();
```

### 2. Actualizar appsettings.json

Aseg�rate de tener configurado:

```json
{
  "ConnectionStrings": {
    "AdoPetsDb": "Server=localhost;Database=AdoPetsDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "AdoPetsAPI",
    "Audience": "AdoPetsApp",
    "SecretKey": "TuClaveSecretaMuySeguraDeAlMenos32Caracteres!123",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "Cors": {
    "PolicyName": "AdoPetsPolicy",
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:4200"]
  },
  "Policies": {
    "CurrentVersion": "1.0.0"
  }
}
```

## ?? Endpoints de Autenticaci�n

### POST /api/v1/auth/login
Iniciar sesi�n con credenciales

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "password": "Password123!",
  "rememberMe": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Inicio de sesi�n exitoso",
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "CfDJ8...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": {
      "id": "guid",
      "nombre": "Juan",
      "email": "usuario@ejemplo.com",
      "roles": ["Adoptante"]
    }
  }
}
```

### POST /api/v1/auth/register
Registrar nuevo usuario

**Request:**
```json
{
  "nombre": "Juan",
  "apellidoPaterno": "P�rez",
  "apellidoMaterno": "Garc�a",
  "email": "juan@ejemplo.com",
  "telefono": "5551234567",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "aceptaPoliticas": true
}
```

### POST /api/v1/auth/change-password
Cambiar contrase�a (requiere autenticaci�n)

**Request:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmNewPassword": "NewPassword123!"
}
```

### POST /api/v1/auth/logout
Cerrar sesi�n (requiere autenticaci�n)

### GET /api/v1/auth/me
Obtener informaci�n del usuario autenticado

## ?? Endpoints de Gesti�n de Usuarios

### GET /api/v1/usuarios
Obtener lista paginada de usuarios (Solo Admin)

**Query Params:**
- `pageNumber`: N�mero de p�gina (default: 1)
- `pageSize`: Tama�o de p�gina (default: 10)

### GET /api/v1/usuarios/{id}
Obtener usuario por ID (Staff)

### POST /api/v1/usuarios
Crear nuevo usuario (Solo Admin)

**Request:**
```json
{
  "nombre": "Mar�a",
  "apellidoPaterno": "L�pez",
  "apellidoMaterno": "Ram�rez",
  "email": "maria@ejemplo.com",
  "telefono": "5559876543",
  "password": "Password123!",
  "rolesIds": ["guid-rol-1", "guid-rol-2"]
}
```

### PUT /api/v1/usuarios/{id}
Actualizar usuario (Solo Admin)

**Request:**
```json
{
  "nombre": "Mar�a",
  "apellidoPaterno": "L�pez",
  "apellidoMaterno": "Ram�rez",
  "telefono": "5559876543",
  "rolesIds": ["guid-rol-1"]
}
```

### DELETE /api/v1/usuarios/{id}
Eliminar usuario (Solo Admin)

### PATCH /api/v1/usuarios/{id}/activate
Activar usuario (Solo Admin)

### PATCH /api/v1/usuarios/{id}/deactivate
Desactivar usuario (Solo Admin)

### POST /api/v1/usuarios/{id}/roles
Asignar roles a usuario (Solo Admin)

**Request:**
```json
["guid-rol-1", "guid-rol-2"]
```

## ?? Pol�ticas de Autorizaci�n

- **AdminOnly**: Solo usuarios con rol "Admin"
- **VetOnly**: Usuarios con rol "Veterinario" o "Admin"
- **StaffOnly**: Usuarios con roles "Admin", "Veterinario", "Recepcionista" o "Asistente"

## ??? Seguridad Implementada

1. **Hashing de Contrase�as**: HMACSHA512
2. **JWT Tokens**: Para autenticaci�n
3. **Validaci�n de DTOs**: DataAnnotations
4. **Pol�ticas de Autorizaci�n**: Role-based
5. **Validaci�n de Contrase�as**: 
   - M�nimo 8 caracteres
   - Al menos una may�scula
   - Al menos una min�scula
   - Al menos un n�mero
   - Al menos un car�cter especial

## ?? Probar los Endpoints

1. **Iniciar la aplicaci�n**
```bash
dotnet run
```

2. **Acceder a Swagger**
```
https://localhost:5001
```

3. **Registrar un usuario**
```bash
POST /api/v1/auth/register
```

4. **Iniciar sesi�n**
```bash
POST /api/v1/auth/login
```

5. **Usar el token** en los endpoints protegidos
```
Authorization: Bearer {tu_token}
```

## ?? Notas Importantes

1. Necesitas ejecutar las migraciones antes de usar la API
2. La base de datos debe tener al menos los roles b�sicos creados
3. El refresh token a�n no est� completamente implementado (pendiente de almacenamiento en BD)
4. Por defecto, el registro asigna el rol "Adoptante"

## ?? Pr�ximos Pasos Recomendados

1. Implementar almacenamiento de refresh tokens en BD
2. Agregar l�mite de intentos de login fallidos
3. Implementar recuperaci�n de contrase�a por email
4. Agregar verificaci�n de email
5. Implementar 2FA (Autenticaci�n de dos factores)
6. Agregar logs de auditor�a de accesos

## ?? Troubleshooting

Si encuentras errores de compilaci�n:
1. Verifica que todos los archivos se hayan creado correctamente
2. Aseg�rate de agregar `builder.Services.AddApplicationServices();` en Program.cs
3. Ejecuta `dotnet restore`
4. Limpia y reconstruye: `dotnet clean && dotnet build`
