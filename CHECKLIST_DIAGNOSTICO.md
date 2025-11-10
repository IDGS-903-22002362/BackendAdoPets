# ?? Checklist de Verificación - Error 401

## ? Estado Actual del Diagnóstico

### **Logs del Backend:**
```
Authorization failed. These requirements were not met:
DenyAnonymousAuthorizationRequirement: Requires an authenticated user.
OnChallenge - Autenticación desafiada
AuthenticationScheme: Bearer was challenged.
Request finished HTTP/1.1 POST http://192.168.100.11:5151/api/v1/MisMascotas - 401
```

### **Análisis:**
- ? **NO aparece**: `?? === INICIO AUTH DEBUG ===` (del middleware)
- ? **NO aparece**: `?? OnMessageReceived` (del evento JWT)
- ? **NO aparece**: `?? === INICIO DEBUG DE CLAIMS ===` (del controlador)

**Conclusión:** El token NO está llegando al servidor. La petición está siendo rechazada **ANTES** de que el JWT Bearer Handler intente procesarla.

---

## ?? Pasos para Solucionar

### **Paso 1: Verificar el Flujo de Login en Flutter**

Cuando haces login con Firebase, debes ver estos logs en Flutter:

```dart
?? Intercambiando token de Firebase por token de AdoPets...
   Firebase Token: {primeros_30_caracteres}...
   Endpoint: http://192.168.100.11:5151/api/v1/Auth/firebase
?? Respuesta del backend:
   success: true
   data: PRESENTE
? Token intercambiado exitosamente
   Usuario: {email}
   AccessToken length: XXX chars
   RefreshToken length: XXX chars
   TokenType: Bearer
?? Guardando tokens en storage...
   AccessToken: {primeros_20_caracteres}...
   RefreshToken: {primeros_20_caracteres}...
? Sesión guardada correctamente
?? Verificación - Token guardado: SÍ
```

**Si NO ves estos logs o el token guardado es NULL:**
- ? El token del backend no se está guardando correctamente
- ? No podrás hacer peticiones autenticadas

---

### **Paso 2: Reemplazar tu api_service.dart**

**Opción A: Reemplazo temporal (recomendado para debugging)**

1. Haz backup de tu `api_service.dart` actual
2. Reemplázalo con el contenido de `FLUTTER_API_SERVICE_DEBUG.dart`
3. Ejecuta tu app e intenta registrar una mascota
4. Revisa los logs de Flutter

**Opción B: Agregar logs manualmente**

Si no quieres reemplazar el archivo completo, agrega esto al inicio de tu método `post`:

```dart
Future<ApiResponse<T>> post<T>({
  required String endpoint,
  Map<String, dynamic>? body,
  required T Function(dynamic) fromJson,
  bool requiresAuth = true,
}) async {
  // ?? DEBUGGING TEMPORAL
  print('?? POST ${ApiConfig.baseUrl}$endpoint');
  print('?? requiresAuth: $requiresAuth');
  
  final headers = <String, String>{
    'Content-Type': 'application/json',
  };

  if (requiresAuth) {
    final token = await _storageService.getAccessToken();
    print('   Token: ${token == null ? "NULL ?" : "SÍ (${token.length} chars) ?"}');
    
    if (token != null) {
      headers['Authorization'] = 'Bearer $token';
      print('   Preview: ${token.substring(0, min(30, token.length))}...');
    } else {
      print('   ? PROBLEMA: Token es NULL - La petición fallará con 401');
    }
  }
  
  print('?? Headers: ${headers.keys.join(", ")}');
  // FIN DEBUGGING
  
  // ... resto del código original
}
```

---

### **Paso 3: Verificar el StorageService**

Asegúrate de que tu `StorageService` esté guardando y recuperando el token correctamente:

```dart
// storage_service.dart

import 'package:flutter_secure_storage/flutter_secure_storage.dart';

class StorageService {
  final _storage = const FlutterSecureStorage();
  
  // Claves
  static const _accessTokenKey = 'access_token';
  static const _refreshTokenKey = 'refresh_token';

  // Guardar token
  Future<void> saveAccessToken(String token) async {
    print('?? Guardando access token...');
    print('   Longitud: ${token.length}');
    await _storage.write(key: _accessTokenKey, value: token);
    
    // ?? Verificación inmediata
    final saved = await _storage.read(key: _accessTokenKey);
    print('   Verificado: ${saved != null ? "? GUARDADO" : "? FALLÓ"}');
  }

  // Obtener token
  Future<String?> getAccessToken() async {
    print('?? Obteniendo access token del storage...');
    final token = await _storage.read(key: _accessTokenKey);
    print('   Resultado: ${token != null ? "? ENCONTRADO (${token.length} chars)" : "? NULL"}');
    return token;
  }

  // Verificar si hay sesión activa
  Future<bool> hasActiveSession() async {
    final token = await getAccessToken();
    return token != null;
  }

  // Limpiar todo
  Future<void> clearAll() async {
    await _storage.deleteAll();
    print('??? Storage limpiado');
  }
}
```

---

### **Paso 4: Verificar que usas el token CORRECTO**

En tu `AuthService.signInWithGoogle()`, asegúrate de que después de obtener el token del backend lo guardes:

```dart
Future<ApiResponse<AuthResponse>> signInWithGoogle() async {
  try {
    // ... código de Firebase ...

    // 5. Obtener Firebase ID Token
    final idToken = await userCredential.user?.getIdToken();
    
    // 6. Intercambiar Firebase token por JWT de AdoPets
    final response = await _exchangeFirebaseToken(idToken);
    
    // ?? IMPORTANTE: Verificar que se guardó
    if (response.success && response.data != null) {
      print('? Login exitoso');
      
      // Verificar inmediatamente
      final savedToken = await _storageService.getAccessToken();
      if (savedToken == null) {
        print('??? ERROR CRÍTICO: Token NO se guardó en storage');
        return ApiResponse<AuthResponse>(
          success: false,
          message: 'Error al guardar token',
        );
      } else {
        print('? Token guardado y verificado');
      }
    }
    
    return response;
  } catch (e) {
    // ... manejo de errores ...
  }
}
```

---

## ?? Prueba de Diagnóstico Rápida

Agrega este código en algún lugar de tu app (por ejemplo, en un botón de debug):

```dart
// Widget de diagnóstico
ElevatedButton(
  onPressed: () async {
    print('');
    print('?? ========================================');
    print('?? DIAGNÓSTICO DE AUTENTICACIÓN');
    print('?? ========================================');
    
    final storageService = StorageService();
    
    // 1. Verificar token en storage
    final token = await storageService.getAccessToken();
    print('1. Token en storage:');
    if (token == null) {
      print('   ? NULL - NO HAY TOKEN GUARDADO');
      print('   ?? Solución: Haz login con Google');
    } else {
      print('   ? Token encontrado');
      print('   Longitud: ${token.length} caracteres');
      print('   Primeros 30: ${token.substring(0, min(30, token.length))}...');
      
      // Verificar formato JWT
      if (token.split('.').length == 3) {
        print('   ? Formato JWT válido (3 partes)');
        
        // Intentar decodificar el payload
        try {
          final parts = token.split('.');
          final payload = parts[1];
          final normalized = base64Url.normalize(payload);
          final decoded = utf8.decode(base64Url.decode(normalized));
          final json = jsonDecode(decoded);
          
          print('   ?? Claims del token:');
          print('      - nameid: ${json['nameid']}');
          print('      - email: ${json['email']}');
          print('      - exp: ${json['exp']}');
          
          // Verificar expiración
          final exp = json['exp'] as int;
          final expDate = DateTime.fromMillisecondsSinceEpoch(exp * 1000);
          final now = DateTime.now();
          
          if (expDate.isAfter(now)) {
            print('   ? Token NO expirado');
            print('      Expira en: ${expDate.difference(now).inMinutes} minutos');
          } else {
            print('   ? Token EXPIRADO');
            print('      Expiró hace: ${now.difference(expDate).inMinutes} minutos');
            print('   ?? Solución: Haz login nuevamente');
          }
        } catch (e) {
          print('   ?? No se pudo decodificar el token: $e');
        }
      } else {
        print('   ? NO es un JWT válido (no tiene 3 partes)');
        print('   ?? Puede ser un Firebase token, no un token del backend');
      }
    }
    
    // 2. Verificar usuario en storage
    final usuario = await storageService.getUsuario();
    print('');
    print('2. Usuario en storage:');
    if (usuario == null) {
      print('   ? NULL');
    } else {
      print('   ? Usuario encontrado');
      print('      Email: ${usuario.email}');
      print('      ID: ${usuario.id}');
    }
    
    // 3. Verificar sesión activa
    final hasSession = await storageService.hasActiveSession();
    print('');
    print('3. Sesión activa: ${hasSession ? "? SÍ" : "? NO"}');
    
    print('?? ========================================');
    print('');
  },
  child: Text('?? Diagnóstico'),
)
```

---

## ?? Checklist Final

Antes de intentar registrar una mascota, verifica:

- [ ] **Hiciste login con Google** y viste el log "? Token intercambiado exitosamente"
- [ ] **El token se guardó en storage** (viste "? Sesión guardada correctamente")
- [ ] **El token NO es null** al ejecutar el diagnóstico
- [ ] **El token es un JWT** (tiene 3 partes separadas por puntos)
- [ ] **El token NO está expirado**
- [ ] **El ApiService agrega el header Authorization** al hacer peticiones con `requiresAuth: true`

Si alguno de estos puntos falla, **ESE es tu problema**.

---

## ?? Resumen

**El problema NO está en el backend**, está en Flutter:

1. ? El token NO se está enviando al servidor
2. ? Posibles causas:
   - Token no se guardó después del login
   - Token es null al recuperarlo
   - `requiresAuth` está en false
   - El header Authorization no se está agregando

**Siguiente paso:**
1. Agrega los logs de debugging al `api_service.dart`
2. Haz login con Google
3. Intenta registrar una mascota
4. Comparte los logs completos de Flutter aquí
