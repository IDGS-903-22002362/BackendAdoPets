# Configuración de Firebase para Autenticación Móvil

## ?? Pasos para Configurar Firebase

### 1. Crear/Configurar Proyecto en Firebase

1. Ve a [Firebase Console](https://console.firebase.google.com/)
2. Crea un nuevo proyecto o selecciona uno existente
3. Habilita **Authentication** en el menú lateral
4. En la pestaña **Sign-in method**, habilita:
   - Google Sign-in
   - (Opcional) Apple, Facebook, etc.

### 2. Obtener Credenciales del Service Account

1. En Firebase Console, haz clic en el ícono de ?? **Configuración del proyecto**
2. Ve a la pestaña **Cuentas de servicio**
3. En la sección **SDK Admin de Firebase**, selecciona **Node.js**
4. Haz clic en **Generar nueva clave privada**
5. Se descargará un archivo JSON similar a este:

```json
{
  "type": "service_account",
  "project_id": "tu-proyecto-12345",
  "private_key_id": "abc123...",
  "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASC...\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-xxxxx@tu-proyecto-12345.iam.gserviceaccount.com",
  "client_id": "123456789...",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/..."
}
```

### 3. Configurar appsettings.json

Copia los valores del archivo descargado a tu `appsettings.json`:

```json
{
  "Firebase": {
    "ProjectId": "tu-proyecto-12345",
    "PrivateKey": "-----BEGIN PRIVATE KEY-----\nMIIEvQIBADANBgkqhkiG9w0BAQEFAASC...\n-----END PRIVATE KEY-----\n",
    "ClientEmail": "firebase-adminsdk-xxxxx@tu-proyecto-12345.iam.gserviceaccount.com"
  }
}
```

**?? IMPORTANTE:**
- El `PrivateKey` debe mantener los saltos de línea como `\n`
- En producción, usa variables de entorno o Azure Key Vault para las credenciales
- **NO** subas el archivo de credenciales de Firebase a tu repositorio

### 4. Configurar Variables de Entorno (Producción)

En producción, es mejor usar variables de entorno:

**Linux/macOS:**
```bash
export Firebase__ProjectId="tu-proyecto-12345"
export Firebase__PrivateKey="-----BEGIN PRIVATE KEY-----\n..."
export Firebase__ClientEmail="firebase-adminsdk-xxxxx@tu-proyecto.iam.gserviceaccount.com"
```

**Windows PowerShell:**
```powershell
$env:Firebase__ProjectId="tu-proyecto-12345"
$env:Firebase__PrivateKey="-----BEGIN PRIVATE KEY-----\n..."
$env:Firebase__ClientEmail="firebase-adminsdk-xxxxx@tu-proyecto.iam.gserviceaccount.com"
```

**Azure App Service - Application Settings:**
```
Firebase:ProjectId = tu-proyecto-12345
Firebase:PrivateKey = -----BEGIN PRIVATE KEY-----\n...
Firebase:ClientEmail = firebase-adminsdk-xxxxx@tu-proyecto.iam.gserviceaccount.com
```

### 5. Configurar Firebase en la App Flutter

#### Instalación de Paquetes

```yaml
# pubspec.yaml
dependencies:
  firebase_core: ^2.24.0
  firebase_auth: ^4.15.0
  google_sign_in: ^6.1.5
  http: ^1.1.0
```

#### Configurar Firebase en Flutter

1. Instala Firebase CLI: `npm install -g firebase-tools`
2. Instala FlutterFire CLI: `dart pub global activate flutterfire_cli`
3. Configura Firebase: `flutterfire configure`
4. Sigue las instrucciones para seleccionar tu proyecto

#### Ejemplo de Implementación en Flutter

```dart
// lib/services/auth_service.dart
import 'package:firebase_auth/firebase_auth.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class AuthService {
  static const String API_BASE_URL = 'https://tu-api.com/api/v1';
  
  final FirebaseAuth _firebaseAuth = FirebaseAuth.instance;
  final GoogleSignIn _googleSignIn = GoogleSignIn();
  
  // 1?? Login con Google ? Obtener Firebase Token
  Future<String?> signInWithGoogle() async {
    try {
      // Iniciar Google Sign In
      final GoogleSignInAccount? googleUser = await _googleSignIn.signIn();
      if (googleUser == null) return null; // Usuario canceló
      
      // Obtener credenciales de Google
      final GoogleSignInAuthentication googleAuth = 
          await googleUser.authentication;
      
      // Crear credencial de Firebase
      final credential = GoogleAuthProvider.credential(
        accessToken: googleAuth.accessToken,
        idToken: googleAuth.idToken,
      );
      
      // Sign in a Firebase
      final userCredential = 
          await _firebaseAuth.signInWithCredential(credential);
      
      // ? Obtener Firebase ID Token
      final firebaseToken = await userCredential.user?.getIdToken();
      
      return firebaseToken;
    } catch (e) {
      print('Error en Google Sign In: $e');
      return null;
    }
  }
  
  // 2?? Intercambiar Firebase Token por JWT de AdoPets
  Future<Map<String, dynamic>?> exchangeFirebaseToken(String firebaseToken) async {
    try {
      final response = await http.post(
        Uri.parse('$API_BASE_URL/auth/firebase'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'idToken': firebaseToken,
          'deviceInfo': 'Flutter - ${Platform.operatingSystem}'
        }),
      );
      
      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['data']; // Contiene accessToken, refreshToken, usuario
      } else {
        print('Error del servidor: ${response.statusCode}');
        return null;
      }
    } catch (e) {
      print('Error al intercambiar token: $e');
      return null;
    }
  }
  
  // 3?? Flujo completo de autenticación
  Future<String?> authenticate() async {
    // Paso 1: Login con Google/Firebase
    final firebaseToken = await signInWithGoogle();
    if (firebaseToken == null) return null;
    
    // Paso 2: Intercambiar por JWT de AdoPets
    final adoPetsData = await exchangeFirebaseToken(firebaseToken);
    if (adoPetsData == null) return null;
    
    // Paso 3: Guardar token de AdoPets
    final adoPetsToken = adoPetsData['accessToken'] as String;
    await _saveToken(adoPetsToken);
    
    return adoPetsToken;
  }
  
  // 4?? Hacer peticiones autenticadas
  Future<http.Response> makeAuthenticatedRequest(String endpoint) async {
    final token = await _getStoredToken();
    
    return http.get(
      Uri.parse('$API_BASE_URL/$endpoint'),
      headers: {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json'
      },
    );
  }
  
  // Helpers para almacenar token (usar shared_preferences o flutter_secure_storage)
  Future<void> _saveToken(String token) async {
    // Implementar con shared_preferences o flutter_secure_storage
  }
  
  Future<String?> _getStoredToken() async {
    // Implementar con shared_preferences o flutter_secure_storage
    return null;
  }
  
  // Cerrar sesión
  Future<void> signOut() async {
    await _firebaseAuth.signOut();
    await _googleSignIn.signOut();
    // Eliminar token almacenado
  }
}
```

#### Uso en la App

```dart
// lib/screens/login_screen.dart
import 'package:flutter/material.dart';
import '../services/auth_service.dart';

class LoginScreen extends StatelessWidget {
  final AuthService _authService = AuthService();
  
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: ElevatedButton.icon(
          icon: Icon(Icons.login),
          label: Text('Iniciar sesión con Google'),
          onPressed: () async {
            // Mostrar loading
            showDialog(
              context: context,
              barrierDismissible: false,
              builder: (_) => Center(child: CircularProgressIndicator()),
            );
            
            // Autenticar
            final token = await _authService.authenticate();
            
            // Ocultar loading
            Navigator.pop(context);
            
            if (token != null) {
              // Login exitoso ? Ir a home
              Navigator.pushReplacementNamed(context, '/home');
            } else {
              // Error ? Mostrar mensaje
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

## ?? Flujo Completo

```
???????????????????????????????????????????????????????????????????
?                    FLUJO DE AUTENTICACIÓN                        ?
???????????????????????????????????????????????????????????????????

1. Usuario presiona "Iniciar con Google" en la app
   ?
2. Google Sign In (pantalla nativa de Google)
   ?
3. Usuario selecciona cuenta y autoriza
   ?
4. App recibe credenciales de Google (accessToken, idToken)
   ?
5. App usa credenciales para autenticar en Firebase
   ?
6. Firebase genera ID Token (JWT de Firebase)
   ?
7. App envía ID Token a tu backend: POST /api/v1/auth/firebase
   ?
8. Backend valida ID Token con Firebase Admin SDK
   ?
9. Backend busca usuario por email en base de datos
   ?
   ?? No existe ? Crea usuario automáticamente con rol "Adoptante"
   ?? Existe ? Recupera usuario existente
   ?
10. Backend genera JWT propio (con claims: userId, email, roles)
    ?
11. Backend devuelve JWT + datos del usuario
    ?
12. App guarda JWT en storage seguro
    ?
13. App usa JWT en header "Authorization: Bearer {token}" para todas las peticiones
```

## ? Verificación

Para verificar que todo funciona correctamente:

1. **En el backend:**
   - Verifica los logs al iniciar: "Firebase Admin SDK inicializado correctamente"
   - Endpoint disponible: `POST https://tu-api/api/v1/auth/firebase`

2. **En la app móvil:**
   - El usuario puede hacer login con Google
   - Se obtiene un Firebase ID Token
   - Se intercambia por un JWT de AdoPets
   - Las peticiones subsecuentes usan el JWT

3. **Probar con Postman/curl:**
```bash
# Obtén un Firebase ID Token desde la consola de Firebase
# Luego prueba:

curl -X POST https://tu-api/api/v1/auth/firebase \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "eyJhbGciOiJSUzI1NiIsImtp..."
  }'
```

## ?? Troubleshooting

### Error: "Firebase no está inicializado correctamente"
- Verifica que las credenciales en `appsettings.json` sean correctas
- Verifica que el `PrivateKey` mantenga los saltos de línea `\n`

### Error: "Token inválido o expirado"
- Los Firebase ID Tokens expiran en 1 hora
- Solicita un nuevo token desde la app

### Error: "Email no encontrado en token"
- Asegúrate de que Google Sign In esté configurado en Firebase Console
- Verifica que el usuario tenga un email asociado en Google

### Error en la app: "PlatformException"
- Verifica que hayas configurado Firebase en la app: `flutterfire configure`
- Verifica que los SHA-1/SHA-256 estén configurados en Firebase Console (Android)

## ?? Seguridad

### ? NO hacer:
- No almacenar credenciales de Firebase en el código fuente
- No subir el archivo de credenciales a Git
- No exponer el `PrivateKey` en logs

### ? SÍ hacer:
- Usar variables de entorno en producción
- Usar Azure Key Vault o AWS Secrets Manager
- Agregar `appsettings.Production.json` al `.gitignore`
- Rotar las credenciales periódicamente

## ?? Referencias

- [Firebase Admin SDK - .NET](https://firebase.google.com/docs/admin/setup)
- [Firebase Authentication - Flutter](https://firebase.google.com/docs/auth/flutter/start)
- [Google Sign-In - Flutter](https://pub.dev/packages/google_sign_in)
