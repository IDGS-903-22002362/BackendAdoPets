# ?? Guía de Depuración - Error 401 al Registrar Mascota

## ?? Resumen del Problema

La app Flutter está recibiendo un **401 Unauthorized** al intentar registrar una mascota en `/api/v1/MisMascotas`.

### Logs del Backend:
```
Authorization failed. These requirements were not met:
DenyAnonymousAuthorizationRequirement: Requires an authenticated user.
AuthenticationScheme: Bearer was challenged.
```

**Esto significa:** El token JWT NO está llegando al servidor o no está en el formato correcto.

---

## ? Cambios Realizados en el Backend

### 1. **Middleware de Logging de Autenticación**
Se agregó `AuthenticationLoggingMiddleware.cs` que mostrará:
- ? Si el header `Authorization` está presente
- ? El formato del header
- ? Los primeros caracteres del token
- ? Todos los headers si `Authorization` no está presente

### 2. **Eventos de JWT Bearer**
Se agregaron eventos en `Program.cs` que loggean:
- `OnMessageReceived` - Cuando se recibe el token
- `OnAuthenticationFailed` - Cuando falla la autenticación (con detalles del error)
- `OnTokenValidated` - Cuando el token es válido (con claims)
- `OnChallenge` - Cuando se desafía la autenticación (401)

### 3. **Logging en el Controlador**
El método `Create` de `MisMascotasController` ahora tiene logging que muestra todos los claims del usuario.

---

## ?? Qué Buscar en los Logs del Backend

Cuando ejecutes la app y intentes registrar una mascota, busca en los logs:

### **Si el token NO llega:**
```
?? === INICIO AUTH DEBUG ===
?? Endpoint: POST /api/v1/MisMascotas
? Authorization Header NO presente
?? Headers disponibles:
   - Content-Type: [valor]
   - User-Agent: [valor]
   - ... (otros headers)
```

### **Si el token llega pero está mal formateado:**
```
?? === INICIO AUTH DEBUG ===
? Authorization Header Presente
?? Header NO comienza con 'Bearer '
   Valor: {el_valor_incorrecto}
```

### **Si el token llega correctamente:**
```
?? === INICIO AUTH DEBUG ===
? Authorization Header Presente
   Comienza con 'Bearer': True
   Token extraído, longitud: XXX caracteres
```

Luego verás los eventos de JWT:
```
?? OnMessageReceived - Token recibido
? OnTokenValidated - Token válido
   Claims: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier=...
```

---

## ?? Problemas Comunes y Soluciones

### **Problema 1: Token No Se Envía**
**Síntoma:** `Authorization Header NO presente`

**Causa:** El `ApiService` de Flutter no está agregando el header

**Solución en Flutter:**
```dart
// En api_service.dart
Future<ApiResponse<T>> post<T>({
  required String endpoint,
  Map<String, dynamic>? body,
  bool requiresAuth = true, // ?? Asegúrate que sea true por defecto
}) async {
  final headers = <String, String>{
    'Content-Type': 'application/json',
  };

  // ?? Asegúrate que este bloque se ejecute
  if (requiresAuth) {
    final token = await _storageService.getAccessToken();
    if (token != null) {
      headers['Authorization'] = 'Bearer $token'; // ? Formato correcto
    }
  }

  // ... resto del código
}
```

**Verificación en Flutter:**
```dart
// Agrega logs antes de hacer la petición
print('?? Enviando petición a: $endpoint');
print('?? Headers: $headers');
```

---

### **Problema 2: Token Expiró**
**Síntoma:** 
```
? OnAuthenticationFailed: SecurityTokenExpiredException
   Mensaje: IDX10223: Lifetime validation failed. The token is expired.
```

**Solución:** Obtén un nuevo token haciendo login nuevamente

---

### **Problema 3: Token Inválido o de Otro Sistema**
**Síntoma:**
```
? OnAuthenticationFailed: SecurityTokenInvalidSignatureException
   Mensaje: IDX10503: Signature validation failed. Keys tried: ...
```

**Causa:** El token fue generado con una clave secreta diferente o es un token de Firebase (no el JWT del backend)

**Solución:** Asegúrate de usar el token que devuelve el endpoint `/api/v1/Auth/firebase`, NO el Firebase ID Token directamente.

---

### **Problema 4: DTO Incorrecto**
**Síntoma:** El token se valida pero luego obtienes un error 400

**Causa:** El modelo Flutter no coincide con `CreateMascotaUsuarioDto`

**Modelo esperado por el backend:**
```json
{
  "nombre": "string",          // ? Requerido, max 100 caracteres
  "especie": "string",         // ? Requerido, max 50 caracteres
  "raza": "string?",           // ?? Opcional, max 100 caracteres
  "fechaNacimiento": "date?",  // ?? Opcional
  "sexo": 0 o 1,               // ? Requerido (0=Macho, 1=Hembra)
  "personalidad": "string?",   // ?? Opcional, max 500 caracteres
  "estadoSalud": "string?",    // ?? Opcional, max 500 caracteres
  "notas": "string?"           // ?? Opcional, max 2000 caracteres
}
```

**Modelo Flutter correcto:**
```dart
class RegistrarMascotaRequest {
  final String nombre;
  final String especie;
  final String? raza;
  final DateTime? fechaNacimiento;
  final int sexo; // 0 = Macho, 1 = Hembra
  final String? personalidad;
  final String? estadoSalud;
  final String? notas;

  Map<String, dynamic> toJson() => {
    'nombre': nombre,
    'especie': especie,
    if (raza != null) 'raza': raza,
    if (fechaNacimiento != null) 
      'fechaNacimiento': fechaNacimiento!.toIso8601String(),
    'sexo': sexo,
    if (personalidad != null) 'personalidad': personalidad,
    if (estadoSalud != null) 'estadoSalud': estadoSalud,
    if (notas != null) 'notas': notas,
  };
}
```

---

## ?? Checklist de Verificación

### **En Flutter:**
- [ ] El `ApiService` agrega el header `Authorization: Bearer {token}`
- [ ] El token guardado es el del backend (no el de Firebase)
- [ ] El endpoint es `/MisMascotas` (sin `/api/v1` porque ya está en `ApiConfig.baseUrl`)
- [ ] El modelo `RegistrarMascotaRequest` coincide con `CreateMascotaUsuarioDto`
- [ ] Agregaste logs para ver qué headers se envían

### **En Backend:**
- [ ] El servidor está corriendo en `http://192.168.100.11:5151`
- [ ] CORS permite el origen de Flutter
- [ ] La configuración JWT está correcta en `appsettings.json`
- [ ] Los logs muestran información de autenticación

---

## ?? Prueba Manual con Postman/Thunder Client

### **1. Obtener Token:**
```http
POST http://192.168.100.11:5151/api/v1/Auth/firebase
Content-Type: application/json

{
  "idToken": "{tu_firebase_token}"
}
```

**Respuesta esperada:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1...",
    "refreshToken": "...",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "usuario": { ... }
  }
}
```

### **2. Registrar Mascota:**
```http
POST http://192.168.100.11:5151/api/v1/MisMascotas
Authorization: Bearer {accessToken_del_paso_anterior}
Content-Type: application/json

{
  "nombre": "Firulais",
  "especie": "Perro",
  "raza": "Labrador",
  "sexo": 0,
  "estadoSalud": "Saludable"
}
```

**Si funciona en Postman pero no en Flutter:** El problema está en cómo Flutter envía el token.

---

## ?? Código de Depuración para Flutter

Agrega esto en tu `api_service.dart`:

```dart
Future<ApiResponse<T>> post<T>({
  required String endpoint,
  Map<String, dynamic>? body,
  required T Function(dynamic) fromJson,
  bool requiresAuth = true,
}) async {
  final url = Uri.parse('${ApiConfig.baseUrl}$endpoint');
  
  final headers = <String, String>{
    'Content-Type': 'application/json',
  };

  if (requiresAuth) {
    final token = await _storageService.getAccessToken();
    
    // ?? LOGGING DE DEPURACIÓN
    print('?? === DEBUG TOKEN ===');
    print('   requiresAuth: $requiresAuth');
    print('   token != null: ${token != null}');
    if (token != null) {
      print('   token length: ${token.length}');
      print('   token preview: ${token.substring(0, min(30, token.length))}...');
      headers['Authorization'] = 'Bearer $token';
      print('   ? Authorization header agregado');
    } else {
      print('   ? Token es null - NO se puede autenticar');
    }
    print('?? === FIN DEBUG ===');
  }

  // ?? LOGGING DE REQUEST
  print('?? POST $url');
  print('?? Headers: $headers');
  print('?? Body: ${jsonEncode(body)}');

  try {
    final response = await http.post(
      url,
      headers: headers,
      body: jsonEncode(body),
    );

    // ?? LOGGING DE RESPONSE
    print('?? Status: ${response.statusCode}');
    print('?? Body: ${response.body}');

    // ... resto del código
  } catch (e) {
    print('? Exception: $e');
    rethrow;
  }
}
```

---

## ?? Próximos Pasos

1. **Ejecuta el backend** con los cambios aplicados
2. **Ejecuta la app Flutter** e intenta registrar una mascota
3. **Revisa los logs del backend** (busca los emojis ??, ??, ?, ?)
4. **Revisa los logs de Flutter** (busca los emojis ??, ??, ??)
5. **Compara** qué dice Flutter que envía vs. qué dice el backend que recibe

Si sigues teniendo problemas, comparte:
- Los logs completos del backend
- Los logs de Flutter
- Tu código de `api_service.dart`
