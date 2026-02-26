# Guía de Autenticación y Manejo de Tokens

Este documento explica cómo implementar la autenticación en el frontend para consumir los servicios del backend de **AdoPets**. El sistema utiliza **JWT (JSON Web Tokens)** como mecanismo principal de seguridad.

Soporta dos métodos de inicio de sesión:
1.  **Credenciales tradicionales** (Email y Contraseña).
2.  **Firebase Authentication** (Google, Facebook, etc.) intercambiando el token por uno del sistema.

---

## Flujo General de Autenticación

1.  **Login**: El cliente envía credenciales o un token de Firebase al backend.
2.  **Recepción del Token**: Si las credenciales son válidas, el backend responde con un `accessToken` (JWT).
3.  **Almacenamiento**: El frontend debe guardar este token de forma segura (ej. `SecureStorage` en móvil o `localStorage`/`sessionStorage` en web, aunque preferiblemente en cookies `httpOnly` si es posible).
4.  **Uso en Peticiones**: Para cada petición a un endpoint protegido, se debe incluir el token en el encabezado `Authorization`.

---

## 1. Autenticación con Credenciales (Email/Password)

Este método se utiliza para usuarios que se registraron directamente en la plataforma.

### Endpoint
`POST /api/v1/auth/login`

### Ejemplo de Petición
```json
{
  "email": "usuario@ejemplo.com",
  "password": "Password123!",
  "rememberMe": false
}
```

### Respuesta Exitosa (200 OK)
El servidor devolverá el token de acceso y la información del usuario.

```json
{
  "success": true,
  "message": "Inicio de sesión exitoso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "usuario@ejemplo.com",
      "nombreCompleto": "Juan Pérez",
      "roles": [
        "Adoptante"
      ]
    }
  }
}
```

### Manejo de Errores

*   **401 Unauthorized**: Credenciales incorrectas o usuario inactivo.
    ```json
    {
      "success": false,
      "message": "Credenciales inválidas",
      "errors": ["Intento de inicio de sesión fallido"]
    }
    ```
*   **400 Bad Request**: Datos faltantes o formato de email inválido.

---

## 2. Autenticación con Firebase

Este método es ideal para aplicaciones móviles o web que usan **Firebase Auth** para gestionar el inicio de sesión con proveedores sociales (Google, Apple, Facebook).

### Flujo Específico
1.  **Frontend**: El usuario inicia sesión con Firebase (ej. botón "Sign in with Google").
2.  **Frontend**: Obtiene el **ID Token** de Firebase del usuario autenticado (`user.getIdToken()`).
3.  **Frontend**: Envía este `idToken` al backend de AdoPets.
4.  **Backend**: Valida el token con Firebase, verifica si el usuario existe (o lo crea automáticamente) y **retorna un JWT propio del sistema**.
5.  **Frontend**: Usa el JWT del sistema (NO el de Firebase) para las siguientes peticiones a la API.

### Endpoint
`POST /api/v1/auth/firebase`

### Ejemplo de Petición
```json
{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6I..."
}
```
*(Nota: El `idToken` es una cadena larga JWT que te entrega el SDK de Firebase).*

### Respuesta Exitosa (200 OK)
La respuesta es idéntica a la del login tradicional.

```json
{
  "success": true,
  "message": "Autenticación con Firebase exitosa",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...", // ESTE es el token que debes usar para la API
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": {
      "id": "...",
      "email": "googleuser@gmail.com",
      "nombreCompleto": "Google User",
      "roles": ["Adoptante"]
    }
  }
}
```

---

## 3. Registro de Nuevos Usuarios

Para usuarios que no usan redes sociales y desean crear una cuenta nueva.

### Endpoint
`POST /api/v1/auth/register`

### Ejemplo de Petición
```json
{
  "nombre": "Ana",
  "apellidoPaterno": "López",
  "email": "ana.lopez@email.com",
  "password": "PasswordSeguro1!",
  "confirmPassword": "PasswordSeguro1!",
  "aceptaPoliticas": true
}
```

*   **Validaciones Importantes**:
    *   Password: Mínimo 8 caracteres, al menos 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial.
    *   Email: Formato válido y no debe existir previamente.

---

## 4. Consumo de Endpoints Protegidos

Una vez que tienes el `accessToken`, debes incluirlo en **todas** las peticiones a endpoints que requieran autenticación (ej. agendar citas, ver perfil, etc.).

### Header Requerido
```http
Authorization: Bearer <TU_ACCESS_TOKEN>
```

### Ejemplo: Obtener Perfil del Usuario (`GET /api/v1/auth/me`)

**Petición (Fetch JS):**
```javascript
const token = "eyJhbGciOiJIUzI1NiIs..."; // Token obtenido en el login

fetch('https://api.adopets.com/api/v1/auth/me', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(response => {
  if (response.status === 401) {
    // Token expirado o inválido -> Redirigir a Login
    console.error("No autorizado");
  }
  return response.json();
})
.then(data => console.log(data));
```

### Respuesta Exitosa
```json
{
  "success": true,
  "data": {
    "id": "...",
    "email": "...",
    "roles": ["Admin"]
  }
}
```

### Códigos de Estado Comunes
*   **200 OK**: Petición exitosa.
*   **401 Unauthorized**: No enviaste el token, el token expiró o es inválido. **Acción**: Redirigir al usuario al login.
*   **403 Forbidden**: El token es válido, pero el usuario no tiene permisos (ej. un "Adoptante" intentando acceder a un endpoint de "Admin").

---

## 5. Roles y Permisos

El sistema maneja roles para restringir el acceso a ciertas funcionalidades. Los roles principales son:

*   **Admin**: Acceso total al sistema.
*   **Veterinario**: Gestión de citas, expedientes médicos, historial clínico.
*   **Recepcionista/Asistente**: Gestión de agenda, clientes.
*   **Adoptante**: Rol por defecto para usuarios de la app móvil/web pública. Puede ver mascotas, solicitar adopciones y agendar citas.

El backend valida automáticamente estos roles. Si intentas acceder a un recurso sin el rol adecuado, recibirás un error **403 Forbidden**.
