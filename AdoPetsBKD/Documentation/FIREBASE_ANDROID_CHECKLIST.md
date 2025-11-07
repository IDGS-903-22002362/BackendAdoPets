# ?? Checklist de Firebase Authentication para Android

## ? Verificación Completa para Usar Firebase en tu Móvil

### ?? **Parte 1: Backend (.NET) - AdoPetsBKD**

#### 1.1 Credenciales de Firebase
- [x] **Archivo de credenciales descargado** desde Firebase Console
- [x] **Credenciales configuradas** en `appsettings.Development.json`:
  ```json
  {
    "Firebase": {
      "ProjectId": "adopets-c99db",
      "PrivateKey": "-----BEGIN PRIVATE KEY-----\n...",
      "ClientEmail": "firebase-adminsdk-fbsvc@adopets-c99db.iam.gserviceaccount.com"
    }
  }
  ```
  ? **Status**: Configurado

#### 1.2 Backend Ejecutándose
- [ ] **Backend ejecutándose** con `dotnet run`
- [ ] **Puerto abierto en todas las interfaces**: `http://0.0.0.0:5151`
  
  **Verificar en `Program.cs` o ejecutar:**
  ```bash
  dotnet run --urls "http://0.0.0.0:5151"
  ```

- [ ] **Logs muestran Firebase inicializado**:
  ```
  info: AdoPetsBKD.Infrastructure.Services.FirebaseAuthService[0]
        ? Firebase Admin SDK inicializado correctamente
  ```

#### 1.3 Endpoint Disponible
- [ ] **Endpoint funciona**: `POST http://192.168.100.11:5151/api/v1/auth/firebase`
  
  **Probar con navegador:**
  ```
  http://192.168.100.11:5151/swagger
  ```
  Deberías ver la documentación Swagger

#### 1.4 Firewall
- [ ] **Firewall permite conexiones** al puerto 5151
  
  **Verificar/Crear regla:**
  ```powershell
  # Ejecutar como Administrador
  New-NetFirewallRule -DisplayName "AdoPets Backend" -Direction Inbound -Protocol TCP -LocalPort 5151 -Action Allow
  ```

---

### ?? **Parte 2: Frontend (Flutter/Android)**

#### 2.1 Proyecto de Firebase Configurado

- [ ] **Proyecto creado** en [Firebase Console](https://console.firebase.google.com/)
  - Proyecto: `adopets-c99db` ?

- [ ] **App Android agregada** al proyecto Firebase
  
  **Verificar:**
  1. Firebase Console ? Proyecto `adopets-c99db`
  2. ?? **Configuración del proyecto** ? **Tus apps**
  3. Deberías ver tu app Android listada

#### 2.2 Google Sign-In Habilitado

- [ ] **Google Sign-In habilitado** en Firebase
  
  **Verificar:**
  1. Firebase Console ? **Authentication** ? **Sign-in method**
  2. **Google** debe estar **Habilitado** ?
  3. Si no está habilitado:
     - Haz clic en **Google**
     - Toggle a **Habilitado**
     - Ingresa un email de soporte
     - Guardar

#### 2.3 Paquetes de Flutter Instalados

- [ ] **Paquetes agregados** en `pubspec.yaml`:
  ```yaml
  dependencies:
    flutter:
      sdk: flutter
    
    # Firebase
    firebase_core: ^2.24.0
    firebase_auth: ^4.15.0
    google_sign_in: ^6.1.5
    
    # HTTP
    http: ^1.1.0
  ```

- [ ] **Paquetes instalados**:
  ```bash
  flutter pub get
  ```

#### 2.4 Firebase CLI y FlutterFire CLI

- [ ] **Firebase CLI instalado**:
  ```bash
  npm install -g firebase-tools
  
  # Verificar
  firebase --version
  ```

- [ ] **FlutterFire CLI instalado**:
  ```bash
  dart pub global activate flutterfire_cli
  
  # Verificar
  flutterfire --version
  ```

- [ ] **Login en Firebase CLI**:
  ```bash
  firebase login
  ```

#### 2.5 Configuración de Firebase en Flutter

- [ ] **Ejecutado `flutterfire configure`**:
  ```bash
  cd "C:\Users\dell\OneDrive\Escritorio\Trabajos 10\Android\P1\app_movil"
  
  flutterfire configure --project=adopets-c99db
  ```
  
  Esto debe:
  1. Crear/actualizar `lib/firebase_options.dart`
  2. Crear/actualizar `android/app/google-services.json`
  3. Configurar `android/build.gradle` y `android/app/build.gradle`

#### 2.6 Archivo google-services.json

- [ ] **Archivo existe**: `android/app/google-services.json`
  
  **Verificar:**
  ```bash
  dir "android\app\google-services.json"
  ```
  
  Si no existe, descargarlo desde Firebase Console:
  1. ?? Configuración del proyecto ? Tus apps ? Tu app Android
  2. **Descargar google-services.json**
  3. Copiar a `android/app/google-services.json`

#### 2.7 SHA-1 Agregado (CRÍTICO)

- [ ] **SHA-1 obtenido** del certificado de depuración:
  ```bash
  cd android
  .\gradlew signingReport
  ```
  
  **Buscar en la salida:**
  ```
  Variant: debug
  Config: debug
  SHA1: DA:39:A3:EE:5E:6B:4B:0D:32:55:BF:EF:95:60:18:90:AF:D8:07:09
  ```

- [ ] **SHA-1 agregado** en Firebase Console:
  1. Firebase Console ? ?? Configuración del proyecto
  2. Selecciona tu app Android
  3. **Agregar huella digital**
  4. Pegar el SHA-1
  5. Guardar
  
  **?? Sin esto, Google Sign-In NO funcionará**

#### 2.8 Firebase Inicializado en main.dart

- [ ] **Código de inicialización** en `lib/main.dart`:
  ```dart
  import 'package:flutter/material.dart';
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

#### 2.9 URL del Backend Configurada

- [ ] **URL apunta a la IP de la PC** (NO localhost):
  ```dart
  // ? CORRECTO para dispositivo físico:
  static const String BASE_URL = 'http://192.168.100.11:5151/api/v1';
  
  // ? INCORRECTO para dispositivo físico:
  // static const String BASE_URL = 'http://localhost:5151/api/v1';
  // static const String BASE_URL = 'http://127.0.0.1:5151/api/v1';
  ```

---

### ?? **Parte 3: Red y Conectividad**

#### 3.1 Red WiFi
- [ ] **PC y teléfono en la misma red WiFi**
  
  **Verificar en PC:**
  ```powershell
  ipconfig
  
  # Buscar tu IP:
  # Adaptador de LAN inalámbrica Wi-Fi:
  #    Dirección IPv4. . . . . . . . . : 192.168.100.11
  ```
  
  **Verificar en teléfono:**
  - Ajustes ? WiFi ? Red conectada ? Detalles
  - Debería ser: `192.168.100.xxx`

#### 3.2 Conectividad del Backend
- [ ] **Backend accesible desde el teléfono**
  
  **Probar desde el navegador del teléfono:**
  ```
  http://192.168.100.11:5151/swagger
  ```
  
  Si carga Swagger, el backend es accesible ?

#### 3.3 Depuración USB
- [ ] **Depuración USB habilitada** en el teléfono:
  1. Ajustes ? Acerca del teléfono
  2. Tocar **Número de compilación** 7 veces
  3. Ajustes ? Opciones de desarrollador
  4. Habilitar **Depuración USB**

- [ ] **Teléfono conectado por USB** a la PC

- [ ] **Autorización de depuración aceptada** en el teléfono

- [ ] **Flutter detecta el dispositivo**:
  ```bash
  flutter devices
  
  # Deberías ver:
  # SM G973F (mobile) • 1234567890ABCDEF • android-arm64 • Android 13
  ```

---

## ?? **Pruebas de Verificación**

### Prueba 1: Backend con Firebase

```bash
# Ejecutar backend
cd C:\Users\dell\source\repos\AdoPetsBKD\AdoPetsBKD
dotnet run

# Buscar en los logs:
# ? Firebase Admin SDK inicializado correctamente
# ? Now listening on: http://0.0.0.0:5151
```

### Prueba 2: Conectividad desde el Teléfono

Abrir navegador en el teléfono:
```
http://192.168.100.11:5151/swagger
```

**Resultado esperado:** Página de Swagger carga correctamente ?

### Prueba 3: Firebase en Flutter

Crear archivo `test_firebase.dart` en tu proyecto Flutter:

```dart
import 'package:flutter/material.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:google_sign_in/google_sign_in.dart';
import 'firebase_options.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );
  runApp(MaterialApp(home: TestScreen()));
}

class TestScreen extends StatelessWidget {
  final _googleSignIn = GoogleSignIn();

  Future<void> testAuth() async {
    try {
      print('?? Iniciando Google Sign In...');
      final googleUser = await _googleSignIn.signIn();
      
      if (googleUser != null) {
        print('? Usuario: ${googleUser.email}');
        print('? Firebase funcionando correctamente!');
      }
    } catch (e) {
      print('? Error: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Test Firebase')),
      body: Center(
        child: ElevatedButton(
          onPressed: testAuth,
          child: Text('Probar Google Sign In'),
        ),
      ),
    );
  }
}
```

**Ejecutar:**
```bash
flutter run test_firebase.dart
```

**Resultado esperado:** 
- Pantalla de selección de cuenta de Google
- Logs muestran: `? Firebase funcionando correctamente!`

### Prueba 4: Flujo Completo (Firebase ? Backend)

```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

Future<void> testFullFlow() async {
  try {
    // 1. Google Sign In
    final googleUser = await GoogleSignIn().signIn();
    final googleAuth = await googleUser!.authentication;
    
    // 2. Firebase Auth
    final credential = GoogleAuthProvider.credential(
      accessToken: googleAuth.accessToken,
      idToken: googleAuth.idToken,
    );
    
    final userCredential = await FirebaseAuth.instance.signInWithCredential(credential);
    
    // 3. Obtener Firebase Token
    final firebaseToken = await userCredential.user!.getIdToken();
    print('? Firebase Token: ${firebaseToken!.substring(0, 50)}...');
    
    // 4. Intercambiar por JWT de AdoPets
    final response = await http.post(
      Uri.parse('http://192.168.100.11:5151/api/v1/auth/firebase'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'idToken': firebaseToken,
        'deviceInfo': 'Test Android'
      }),
    );
    
    print('?? Status: ${response.statusCode}');
    
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      final adoPetsToken = data['data']['accessToken'];
      print('? AdoPets JWT: ${adoPetsToken.substring(0, 50)}...');
      print('? FLUJO COMPLETO FUNCIONANDO!');
    } else {
      print('? Error del backend: ${response.body}');
    }
    
  } catch (e) {
    print('? Error: $e');
  }
}
```

**Resultado esperado:**
```
? Firebase Token: eyJhbGciOiJSUzI1NiIsImtpZCI6IjFmOD...
?? Status: 200
? AdoPets JWT: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
? FLUJO COMPLETO FUNCIONANDO!
```

---

## ?? Errores Comunes y Soluciones

### Error 1: "MissingPluginException(No implementation found for method signIn)"

**Causa:** Firebase no está configurado correctamente

**Solución:**
```bash
flutterfire configure --project=adopets-c99db
flutter clean
flutter run
```

### Error 2: "PlatformException(sign_in_failed, ...)"

**Causa:** SHA-1 no está en Firebase Console

**Solución:**
```bash
cd android
.\gradlew signingReport
# Copiar SHA-1 y agregarlo en Firebase Console
```

### Error 3: "Connection refused" al llamar al backend

**Causa:** URL incorrecta o firewall bloqueando

**Solución:**
1. Verificar URL: `http://192.168.100.11:5151` (NO localhost)
2. Verificar firewall: Ejecutar como admin
   ```powershell
   New-NetFirewallRule -DisplayName "AdoPets" -Direction Inbound -Protocol TCP -LocalPort 5151 -Action Allow
   ```
3. Probar desde navegador del teléfono

### Error 4: "Firebase not initialized"

**Causa:** Falta `Firebase.initializeApp()` en `main.dart`

**Solución:**
```dart
void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );
  runApp(MyApp());
}
```

---

## ?? Resumen Final

Una vez completados todos los checkboxes:

### Backend
- [x] Credenciales Firebase configuradas
- [ ] Backend ejecutándose en `0.0.0.0:5151`
- [ ] Firebase Admin SDK inicializado
- [ ] Firewall permite conexiones

### Flutter
- [ ] Paquetes instalados
- [ ] `flutterfire configure` ejecutado
- [ ] `google-services.json` en su lugar
- [ ] SHA-1 agregado en Firebase
- [ ] Google Sign-In habilitado
- [ ] Firebase inicializado en `main.dart`
- [ ] URL del backend correcta

### Red
- [ ] Misma WiFi (192.168.100.x)
- [ ] Backend accesible desde teléfono
- [ ] Dispositivo detectado por Flutter

### Pruebas
- [ ] Backend responde en Swagger
- [ ] Google Sign-In funciona
- [ ] Firebase Token se obtiene
- [ ] Backend intercambia token por JWT
- [ ] JWT funciona en otros endpoints

**¡Cuando todos estén ?, tu app móvil estará lista para usar Firebase Authentication!**
