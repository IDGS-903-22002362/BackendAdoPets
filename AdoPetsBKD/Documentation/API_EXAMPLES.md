# Ejemplos de Uso - API AdoPets

## ?? Colección Postman

### Variables de Entorno
```json
{
  "baseUrl": "https://localhost:5001/api/v1",
  "token": ""
}
```

## ?? Autenticación

### 1. Registrar Usuario
```bash
curl -X POST "{{baseUrl}}/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Juan",
    "apellidoPaterno": "Pérez",
    "apellidoMaterno": "García",
    "email": "juan.perez@ejemplo.com",
    "telefono": "5551234567",
    "password": "Password123!",
    "confirmPassword": "Password123!",
    "aceptaPoliticas": true
  }'
```

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Usuario registrado exitosamente",
  "data": {
    "accessToken": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64string...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nombre": "Juan",
      "apellidoPaterno": "Pérez",
      "apellidoMaterno": "García",
      "email": "juan.perez@ejemplo.com",
      "telefono": "5551234567",
      "nombreCompleto": "Juan Pérez García",
      "estatus": 1,
      "roles": ["Adoptante"],
      "tienePoliticasAceptadas": true,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  },
  "errors": []
}
```

### 2. Iniciar Sesión
```bash
curl -X POST "{{baseUrl}}/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "juan.perez@ejemplo.com",
    "password": "Password123!",
    "rememberMe": false
  }'
```

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Inicio de sesión exitoso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64string...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nombreCompleto": "Juan Pérez García",
      "email": "juan.perez@ejemplo.com",
      "roles": ["Adoptante"]
    }
  }
}
```

**Respuesta Error (401):**
```json
{
  "success": false,
  "message": "Credenciales inválidas",
  "data": null,
  "errors": ["Credenciales inválidas"]
}
```

### 3. Obtener Usuario Actual (Requiere Token)
```bash
curl -X GET "{{baseUrl}}/auth/me" \
  -H "Authorization: Bearer {{token}}"
```

### 4. Cambiar Contraseña (Requiere Token)
```bash
curl -X POST "{{baseUrl}}/auth/change-password" \
  -H "Authorization: Bearer {{token}}" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "Password123!",
    "newPassword": "NewPassword456!",
    "confirmNewPassword": "NewPassword456!"
  }'
```

### 5. Cerrar Sesión (Requiere Token)
```bash
curl -X POST "{{baseUrl}}/auth/logout" \
  -H "Authorization: Bearer {{token}}"
```

## ?? Gestión de Usuarios

### 1. Listar Usuarios (Solo Admin)
```bash
curl -X GET "{{baseUrl}}/usuarios?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer {{token}}"
```

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "nombreCompleto": "Juan Pérez García",
        "email": "juan.perez@ejemplo.com",
        "telefono": "5551234567",
        "estatus": 1,
        "roles": ["Adoptante"],
        "ultimoAccesoAt": "2024-01-15T10:30:00Z",
        "createdAt": "2024-01-15T08:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "totalCount": 1,
    "hasPrevious": false,
    "hasNext": false
  }
}
```

### 2. Obtener Usuario por ID (Staff)
```bash
curl -X GET "{{baseUrl}}/usuarios/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {{token}}"
```

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Operación exitosa",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombre": "Juan",
    "apellidoPaterno": "Pérez",
    "apellidoMaterno": "García",
    "nombreCompleto": "Juan Pérez García",
    "email": "juan.perez@ejemplo.com",
    "telefono": "5551234567",
    "estatus": 1,
    "ultimoAccesoAt": "2024-01-15T10:30:00Z",
    "aceptoPoliticasVersion": "1.0.0",
    "aceptoPoliticasAt": "2024-01-15T08:00:00Z",
    "roles": [
      {
        "id": "rol-guid",
        "nombre": "Adoptante",
        "descripcion": "Usuario que puede adoptar mascotas"
      }
    ],
    "createdAt": "2024-01-15T08:00:00Z",
    "updatedAt": null
  }
}
```

### 3. Crear Usuario (Solo Admin)
```bash
curl -X POST "{{baseUrl}}/usuarios" \
  -H "Authorization: Bearer {{token}}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "María",
    "apellidoPaterno": "López",
    "apellidoMaterno": "Ramírez",
    "email": "maria.lopez@ejemplo.com",
    "telefono": "5559876543",
    "password": "Password123!",
    "rolesIds": ["rol-guid-1", "rol-guid-2"]
  }'
```

**Respuesta Exitosa (201):**
```json
{
  "success": true,
  "message": "Usuario creado exitosamente",
  "data": {
    "id": "new-user-guid",
    "nombreCompleto": "María López Ramírez",
    "email": "maria.lopez@ejemplo.com",
    "roles": [
      {
        "id": "rol-guid-1",
        "nombre": "Veterinario"
      }
    ]
  }
}
```

### 4. Actualizar Usuario (Solo Admin)
```bash
curl -X PUT "{{baseUrl}}/usuarios/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {{token}}" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "María",
    "apellidoPaterno": "López",
    "apellidoMaterno": "Ramírez",
    "telefono": "5559876543",
    "rolesIds": ["rol-guid-1"]
  }'
```

### 5. Eliminar Usuario (Solo Admin)
```bash
curl -X DELETE "{{baseUrl}}/usuarios/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer {{token}}"
```

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Usuario eliminado exitosamente",
  "data": true
}
```

### 6. Activar Usuario (Solo Admin)
```bash
curl -X PATCH "{{baseUrl}}/usuarios/3fa85f64-5717-4562-b3fc-2c963f66afa6/activate" \
  -H "Authorization: Bearer {{token}}"
```

### 7. Desactivar Usuario (Solo Admin)
```bash
curl -X PATCH "{{baseUrl}}/usuarios/3fa85f64-5717-4562-b3fc-2c963f66afa6/deactivate" \
  -H "Authorization: Bearer {{token}}"
```

### 8. Asignar Roles (Solo Admin)
```bash
curl -X POST "{{baseUrl}}/usuarios/3fa85f64-5717-4562-b3fc-2c963f66afa6/roles" \
  -H "Authorization: Bearer {{token}}" \
  -H "Content-Type: application/json" \
  -d '["rol-guid-1", "rol-guid-2"]'
```

## ?? Códigos de Estado HTTP

- **200 OK**: Operación exitosa
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Datos inválidos o error de validación
- **401 Unauthorized**: No autenticado o token inválido
- **403 Forbidden**: No tiene permisos para realizar la acción
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor

## ?? Validaciones de Contraseña

La contraseña debe cumplir con:
- Mínimo 8 caracteres
- Al menos una letra mayúscula (A-Z)
- Al menos una letra minúscula (a-z)
- Al menos un número (0-9)
- Al menos un carácter especial (@$!%*?&#)

Ejemplos válidos:
- `Password123!`
- `MySecure@Pass1`
- `Admin#2024Pass`

Ejemplos inválidos:
- `password` (falta mayúscula, número y especial)
- `PASSWORD123` (falta minúscula y especial)
- `Pass1!` (muy corta, menos de 8 caracteres)

## ?? Roles del Sistema

| Rol | Descripción |
|-----|-------------|
| Admin | Acceso completo al sistema |
| Veterinario | Gestión de historiales clínicos y citas |
| Recepcionista | Gestión de citas y mascotas |
| Asistente | Soporte en tareas operativas |
| Adoptante | Usuario que puede adoptar mascotas |

## ??? Testing en Swagger

1. Navega a `https://localhost:5001`
2. Haz clic en "Authorize" (botón con candado)
3. Ingresa: `Bearer {tu_token}`
4. Prueba los endpoints directamente desde la UI

## ?? Tips

1. **Guardar el token**: Después del login, guarda el `accessToken` en una variable
2. **Token en headers**: Usa el formato `Authorization: Bearer {token}`
3. **Expiración**: El token expira en 60 minutos por defecto
4. **Refresh Token**: (Pendiente de implementación completa)
