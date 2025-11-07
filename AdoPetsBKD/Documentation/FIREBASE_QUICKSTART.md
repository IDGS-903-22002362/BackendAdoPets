# ? Quick Start - Firebase Authentication

## ?? Setup Rápido en 5 Minutos

### Backend (.NET)

#### 1. Configurar Firebase (Ya está hecho ?)
El código ya está implementado. Solo necesitas configurar las credenciales.

#### 2. Obtener Credenciales de Firebase

```bash
# 1. Ve a https://console.firebase.google.com/
# 2. Selecciona tu proyecto ? ?? Configuración ? Cuentas de servicio
# 3. Clic en "Generar nueva clave privada" ? Descarga el JSON
```

#### 3. Configurar appsettings.json

```json
{
  "Firebase": {
    "ProjectId": "COPIA_AQUI_project_id",
    "PrivateKey": "COPIA_AQUI_private_key (con \\n)",
    "ClientEmail": "COPIA_AQUI_client_email"
  }
}
```

#### 4. Ejecutar el Backend

```bash
dotnet run
```

? Endpoint disponible: `POST https://localhost:5001/api/v1/auth/firebase`

---

### Frontend Flutter

#### 1. Instalar Dependencias

```yaml
# pubspec.yaml
dependencies:
  firebase_core: ^2.24.0
  firebase_auth: ^4.15.0
  google_sign_in: ^6.1.5
  http: ^1.1.0
```

```bash
flutter pub get
```

#### 2. Configurar Firebase en Flutter

```bash
# Instalar Firebase CLI
npm install -g firebase-tools

# Instalar FlutterFire CLI
dart pub global activate flutterfire_cli

# Configurar proyecto
flutterfire configure
```

#### 3. Inicializar Firebase

```dart
// lib/main.dart
import 'package:firebase_core/firebase_core.dart';
import 'firebase_options.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );
  runApp(MyApp());
}
```

#### 4. Crear Servicio de Autenticación

```dart
// lib/services/auth_service.dart
import 'package:firebase_auth/firebase_auth.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class AuthService {
  static const API_URL = 'https://tu-api.com/api/v1/auth/firebase';
  
  final _firebaseAuth = FirebaseAuth.instance;
  final _googleSignIn = GoogleSignIn();
  
  Future<String?> loginWithGoogle() async {
    try {
      // 1. Google Sign In
      final googleUser = await _googleSignIn.signIn();
      if (googleUser == null) return null;
      
      final googleAuth = await googleUser.authentication;
      
      // 2. Firebase Sign In
      final credential = GoogleAuthProvider.credential(
        accessToken: googleAuth.accessToken,
        idToken: googleAuth.idToken,
      );
      
      final userCredential = await _firebaseAuth.signInWithCredential(credential);
      
      // 3. Obtener Firebase Token
      final firebaseToken = await userCredential.user?.getIdToken();
      if (firebaseToken == null) return null;
      
      // 4. Intercambiar por JWT de AdoPets
      final response = await http.post(
        Uri.parse(API_URL),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'idToken': firebaseToken}),
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['data']['accessToken'];
      }
      
      return null;
    } catch (e) {
      print('Error: $e');
      return null;
    }
  }
}
```

#### 5. Usar en la UI

```dart
// lib/screens/login_screen.dart
import 'package:flutter/material.dart';

class LoginScreen extends StatelessWidget {
  final _authService = AuthService();
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: ElevatedButton.icon(
          icon: Icon(Icons.login),
          label: Text('Continuar con Google'),
          onPressed: () async {
            final token = await _authService.loginWithGoogle();
            
            if (token != null) {
              // ? Login exitoso
              Navigator.pushReplacementNamed(context, '/home');
            } else {
              // ? Error
              ScaffoldMessenger.of(context).showSnackBar(
                SnackBar(content: Text('Error al iniciar sesión'))
              );
            }
          },
        ),
      ),
    );
  }
}
```

---

## ?? Probar que Funciona

### Test 1: Desde Postman

```bash
# 1. Importar colección:
#    Documentation/AdoPets_Firebase_Auth.postman_collection.json

# 2. Obtener un Firebase ID Token:
#    - Login en la app Flutter
#    - O desde Firebase Console ? Authentication ? Users
#    - Copiar el token

# 3. Ejecutar request "Login con Firebase"
#    POST /api/v1/auth/firebase
#    Body: {"idToken": "TU_TOKEN_AQUI"}

# 4. Deberías recibir:
#    {
#      "success": true,
#      "data": {
#        "accessToken": "eyJ...",
#        "usuario": {...}
#      }
#    }
```

### Test 2: Desde Flutter

```dart
void testAuth() async {
  final authService = AuthService();
  final token = await authService.loginWithGoogle();
  
  if (token != null) {
    print('? Token obtenido: ${token.substring(0, 20)}...');
    
    // Probar con una petición
    final response = await http.get(
      Uri.parse('https://tu-api.com/api/v1/auth/me'),
      headers: {'Authorization': 'Bearer $token'},
    );
    
    print('?? Usuario: ${response.body}');
  } else {
    print('? Error en autenticación');
  }
}
```

---

## ? Checklist de Verificación

### Backend
- [ ] Firebase Admin SDK instalado (`dotnet add package FirebaseAdmin`)
- [ ] Credenciales configuradas en `appsettings.json`
- [ ] Backend ejecutándose sin errores
- [ ] Logs muestran: "Firebase Admin SDK inicializado correctamente"

### Frontend Flutter
- [ ] Paquetes instalados (`firebase_core`, `firebase_auth`, `google_sign_in`)
- [ ] `flutterfire configure` ejecutado
- [ ] `Firebase.initializeApp()` en `main.dart`
- [ ] Google Sign-In habilitado en Firebase Console
- [ ] SHA-1 configurado (Android) en Firebase Console

### Testing
- [ ] Usuario puede hacer login con Google en la app
- [ ] Se obtiene un Firebase ID Token
- [ ] Se envía a `/api/v1/auth/firebase`
- [ ] Se recibe un JWT de AdoPets
- [ ] El JWT funciona en otros endpoints (ej: `/api/v1/mascotas`)

---

## ?? Troubleshooting Rápido

| Error | Solución |
|-------|----------|
| "Firebase no está inicializado" | Verifica credenciales en `appsettings.json` |
| "Token inválido" | El token expiró (1 hora), obtén uno nuevo |
| "PlatformException" (Flutter) | Ejecuta `flutterfire configure` |
| "Email no encontrado" | Verifica que Google Sign-In esté habilitado en Firebase Console |
| CORS error | Agrega el dominio a `Cors:AllowedOrigins` en appsettings.json |

---

## ?? Resultado Final

```
Usuario en app móvil:
  1. Presiona "Continuar con Google"
  2. Selecciona cuenta de Google
  3. Automáticamente entra a la app
  4. Puede usar todas las funcionalidades
  
Backend:
  - Valida token de Firebase
  - Crea/encuentra usuario
  - Genera JWT propio
  - Usuario puede acceder a todos los endpoints
```

---

## ?? Más Información

- **Configuración completa:** `Documentation/FIREBASE_SETUP.md`
- **Arquitectura:** `Documentation/FIREBASE_INTEGRATION_SUMMARY.md`
- **API Docs:** `Documentation/AUTHENTICATION_README.md`

---

## ?? ¡Todo Listo!

Ya tienes autenticación con Google funcionando en tu app móvil, compartiendo el backend con la web.

**Siguiente paso:** Habilitar más proveedores (Apple, Facebook) en Firebase Console ? Authentication ? Sign-in method
