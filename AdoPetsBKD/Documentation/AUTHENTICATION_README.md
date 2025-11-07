# API de Autenticación y Gestión de Usuarios - AdoPets

## ?? Resumen

Se han creado las APIs/Endpoints para autenticación y manejo de usuarios siguiendo las mejores prácticas de Clean Architecture y SOLID.

**NUEVO:** Ahora incluye autenticación con Firebase para aplicaciones móviles, permitiendo login con Google y otros proveedores manteniendo el sistema JWT propio.

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
  },
  "Firebase": {
    "ProjectId": "tu-proyecto-firebase",
    "PrivateKey": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqh...\n-----END PRIVATE KEY-----\n",
    "ClientEmail": "firebase-adminsdk-xxxxx@tu-proyecto.iam.gserviceaccount.com"
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
6. **Firebase Authentication**: 
   - Validación de tokens de Firebase
   - Auto-registro de usuarios móviles
   - Compatibilidad con múltiples proveedores de identidad

## ?? Casos de Uso

### Caso 1: Usuario Móvil Nuevo (Primera vez con Google)
1. Usuario hace login con Google en la app Flutter
2. Firebase genera un ID Token
3. App envía el token a `/api/v1/auth/firebase`
4. Backend valida el token con Firebase
5. Backend crea usuario automáticamente en BD local
6. Backend genera y devuelve JWT propio
7. App usa el JWT para todas las peticiones subsecuentes

### Caso 2: Usuario Móvil Existente
1. Usuario hace login con Google en la app Flutter
2. Firebase genera un ID Token
3. App envía el token a `/api/v1/auth/firebase`
4. Backend valida el token con Firebase
5. Backend encuentra usuario existente por email
6. Backend genera y devuelve JWT propio
7. App usa el JWT para todas las peticiones subsecuentes

### Caso 3: Usuario Web (Sin cambios)
1. Usuario ingresa email y contraseña
2. Backend valida credenciales contra BD local
3. Backend genera y devuelve JWT propio
4. Frontend web usa el JWT para todas las peticiones

## ?? Ventajas de esta Arquitectura

1. **Un solo sistema de autorización**: Todos los endpoints usan JWT propio
2. **Múltiples formas de autenticación**: Email/Password (web) y Firebase (móvil)
3. **Roles unificados**: Los mismos roles aplican para web y móvil
4. **Base de datos centralizada**: Todos los usuarios en una sola BD
5. **Escalable**: Fácil agregar más proveedores (Apple, Facebook, etc.)

## ?? Próximos Pasos Recomendados

1. Implementar almacenamiento de refresh tokens en BD
2. Agregar límite de intentos de login fallidos
3. Implementar recuperación de contraseña por email
4. Agregar verificación de email
5. Implementar 2FA (Autenticación de dos factores)
6. Agregar logs de auditoría de accesos
7. Agregar soporte para Apple Sign In
8. Implementar sincronización de foto de perfil de Google
9. Agregar refresh token de Firebase

## ?? Troubleshooting

Si encuentras errores de compilación:
1. Verifica que todos los archivos se hayan creado correctamente
2. Asegúrate de agregar `builder.Services.AddApplicationServices();` en Program.cs
3. Ejecuta `dotnet restore`
4. Limpia y reconstruye: `dotnet clean && dotnet build`

### Errores de Firebase:
1. **"Firebase no está inicializado"**: Verifica las credenciales en appsettings.json
2. **"Token inválido"**: El token de Firebase expiró, solicitar uno nuevo desde la app
3. **"Email no encontrado en token"**: Asegúrate de que el usuario tiene email en Firebase
