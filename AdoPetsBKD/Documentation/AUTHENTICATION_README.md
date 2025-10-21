# API de Autenticación y Gestión de Usuarios - AdoPets

## ?? Resumen

Se han creado las APIs/Endpoints para autenticación y manejo de usuarios siguiendo las mejores prácticas de Clean Architecture y SOLID.

## ??? Arquitectura Implementada

### Capas Creadas:

1. **Application Layer**
   - DTOs (Data Transfer Objects)
   - Interfaces de Repositorios y Servicios

2. **Infrastructure Layer**
   - Implementación de Repositorios
   - Implementación de Servicios
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

## ?? Configuración Requerida

### 1. Agregar en Program.cs

Después de `builder.Services.AddDbContext<AdoPetsDbContext>`, agregar:

```csharp
using AdoPetsBKD.Infrastructure.Extensions;

// ... código existente ...

// Después de Database Context, agregar:
builder.Services.AddApplicationServices();
```

### 2. Actualizar appsettings.json

Asegúrate de tener configurado:

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

## ?? Endpoints de Autenticación

### POST /api/v1/auth/login
Iniciar sesión con credenciales

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
  "message": "Inicio de sesión exitoso",
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
  "apellidoPaterno": "Pérez",
  "apellidoMaterno": "García",
  "email": "juan@ejemplo.com",
  "telefono": "5551234567",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "aceptaPoliticas": true
}
```

### POST /api/v1/auth/change-password
Cambiar contraseña (requiere autenticación)

**Request:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword123!",
  "confirmNewPassword": "NewPassword123!"
}
```

### POST /api/v1/auth/logout
Cerrar sesión (requiere autenticación)

### GET /api/v1/auth/me
Obtener información del usuario autenticado

## ?? Endpoints de Gestión de Usuarios

### GET /api/v1/usuarios
Obtener lista paginada de usuarios (Solo Admin)

**Query Params:**
- `pageNumber`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10)

### GET /api/v1/usuarios/{id}
Obtener usuario por ID (Staff)

### POST /api/v1/usuarios
Crear nuevo usuario (Solo Admin)

**Request:**
```json
{
  "nombre": "María",
  "apellidoPaterno": "López",
  "apellidoMaterno": "Ramírez",
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
  "nombre": "María",
  "apellidoPaterno": "López",
  "apellidoMaterno": "Ramírez",
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

## ?? Políticas de Autorización

- **AdminOnly**: Solo usuarios con rol "Admin"
- **VetOnly**: Usuarios con rol "Veterinario" o "Admin"
- **StaffOnly**: Usuarios con roles "Admin", "Veterinario", "Recepcionista" o "Asistente"

## ??? Seguridad Implementada

1. **Hashing de Contraseñas**: HMACSHA512
2. **JWT Tokens**: Para autenticación
3. **Validación de DTOs**: DataAnnotations
4. **Políticas de Autorización**: Role-based
5. **Validación de Contraseñas**: 
   - Mínimo 8 caracteres
   - Al menos una mayúscula
   - Al menos una minúscula
   - Al menos un número
   - Al menos un carácter especial

## ?? Probar los Endpoints

1. **Iniciar la aplicación**
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

4. **Iniciar sesión**
```bash
POST /api/v1/auth/login
```

5. **Usar el token** en los endpoints protegidos
```
Authorization: Bearer {tu_token}
```

## ?? Notas Importantes

1. Necesitas ejecutar las migraciones antes de usar la API
2. La base de datos debe tener al menos los roles básicos creados
3. El refresh token aún no está completamente implementado (pendiente de almacenamiento en BD)
4. Por defecto, el registro asigna el rol "Adoptante"

## ?? Próximos Pasos Recomendados

1. Implementar almacenamiento de refresh tokens en BD
2. Agregar límite de intentos de login fallidos
3. Implementar recuperación de contraseña por email
4. Agregar verificación de email
5. Implementar 2FA (Autenticación de dos factores)
6. Agregar logs de auditoría de accesos

## ?? Troubleshooting

Si encuentras errores de compilación:
1. Verifica que todos los archivos se hayan creado correctamente
2. Asegúrate de agregar `builder.Services.AddApplicationServices();` en Program.cs
3. Ejecuta `dotnet restore`
4. Limpia y reconstruye: `dotnet clean && dotnet build`
