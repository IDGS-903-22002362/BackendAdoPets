# Ejemplos de Uso - API AdoPets

## ?? Colecci�n Postman

### Variables de Entorno
```json
{
  "baseUrl": "https://localhost:5001/api/v1",
  "token": ""
}
```

## ?? Autenticaci�n

### 1. Registrar Usuario
```bash
curl -X POST "{{baseUrl}}/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Juan",
    "apellidoPaterno": "P�rez",
    "apellidoMaterno": "Garc�a",
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
      "apellidoPaterno": "P�rez",
      "apellidoMaterno": "Garc�a",
      "email": "juan.perez@ejemplo.com",
      "telefono": "5551234567",
      "nombreCompleto": "Juan P�rez Garc�a",
      "estatus": 1,
      "roles": ["Adoptante"],
      "tienePoliticasAceptadas": true,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  },
  "errors": []
}
```

### 2. Iniciar Sesi�n
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
  "message": "Inicio de sesi�n exitoso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64string...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "nombreCompleto": "Juan P�rez Garc�a",
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
  "message": "Credenciales inv�lidas",
  "data": null,
  "errors": ["Credenciales inv�lidas"]
}
```

### 3. Obtener Usuario Actual (Requiere Token)
```bash
curl -X GET "{{baseUrl}}/auth/me" \
  -H "Authorization: Bearer {{token}}"
```

### 4. Cambiar Contrase�a (Requiere Token)
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

### 5. Cerrar Sesi�n (Requiere Token)
```bash
curl -X POST "{{baseUrl}}/auth/logout" \
  -H "Authorization: Bearer {{token}}"
```

## ?? Gesti�n de Usuarios

### 1. Listar Usuarios (Solo Admin)
```bash
curl -X GET "{{baseUrl}}/usuarios?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer {{token}}"
```

**Respuesta Exitosa (200):**
```json
{
  "success": true,
  "message": "Operaci�n exitosa",
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "nombreCompleto": "Juan P�rez Garc�a",
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
  "message": "Operaci�n exitosa",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nombre": "Juan",
    "apellidoPaterno": "P�rez",
    "apellidoMaterno": "Garc�a",
    "nombreCompleto": "Juan P�rez Garc�a",
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
    "nombre": "Mar�a",
    "apellidoPaterno": "L�pez",
    "apellidoMaterno": "Ram�rez",
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
    "nombreCompleto": "Mar�a L�pez Ram�rez",
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
    "nombre": "Mar�a",
    "apellidoPaterno": "L�pez",
    "apellidoMaterno": "Ram�rez",
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

## ?? C�digos de Estado HTTP

- **200 OK**: Operaci�n exitosa
- **201 Created**: Recurso creado exitosamente
- **400 Bad Request**: Datos inv�lidos o error de validaci�n
- **401 Unauthorized**: No autenticado o token inv�lido
- **403 Forbidden**: No tiene permisos para realizar la acci�n
- **404 Not Found**: Recurso no encontrado
- **500 Internal Server Error**: Error del servidor

## ?? Validaciones de Contrase�a

La contrase�a debe cumplir con:
- M�nimo 8 caracteres
- Al menos una letra may�scula (A-Z)
- Al menos una letra min�scula (a-z)
- Al menos un n�mero (0-9)
- Al menos un car�cter especial (@$!%*?&#)

Ejemplos v�lidos:
- `Password123!`
- `MySecure@Pass1`
- `Admin#2024Pass`

Ejemplos inv�lidos:
- `password` (falta may�scula, n�mero y especial)
- `PASSWORD123` (falta min�scula y especial)
- `Pass1!` (muy corta, menos de 8 caracteres)

## ?? Roles del Sistema

| Rol | Descripci�n |
|-----|-------------|
| Admin | Acceso completo al sistema |
| Veterinario | Gesti�n de historiales cl�nicos y citas |
| Recepcionista | Gesti�n de citas y mascotas |
| Asistente | Soporte en tareas operativas |
| Adoptante | Usuario que puede adoptar mascotas |

## ??? Testing en Swagger

1. Navega a `https://localhost:5001`
2. Haz clic en "Authorize" (bot�n con candado)
3. Ingresa: `Bearer {tu_token}`
4. Prueba los endpoints directamente desde la UI

## ?? Tips

1. **Guardar el token**: Despu�s del login, guarda el `accessToken` en una variable
2. **Token en headers**: Usa el formato `Authorization: Bearer {token}`
3. **Expiraci�n**: El token expira en 60 minutos por defecto
4. **Refresh Token**: (Pendiente de implementaci�n completa)
