# ?? Implementación de Push Notifications en Flutter - AdoPets

## ?? Índice
1. [Configuración de Firebase en Flutter](#1-configuración-de-firebase-en-flutter)
2. [Configuración Android](#2-configuración-android)
3. [Configuración iOS](#3-configuración-ios)
4. [Implementación en Flutter](#4-implementación-en-flutter)
5. [Integración con Backend](#5-integración-con-backend)
6. [Manejo de Notificaciones](#6-manejo-de-notificaciones)
7. [Testing](#7-testing)

---

## 1. Configuración de Firebase en Flutter

### Paso 1.1: Agregar Firebase CLI

```bash
# Instalar Firebase CLI
npm install -g firebase-tools

# Login
firebase login

# Instalar FlutterFire CLI
dart pub global activate flutterfire_cli
```

### Paso 1.2: Configurar Firebase en el Proyecto

```bash
# En el directorio raíz de tu proyecto Flutter
flutterfire configure
```

Esto creará automáticamente:
- `firebase_options.dart`
- Configuración para Android (`google-services.json`)
- Configuración para iOS (`GoogleService-Info.plist`)

### Paso 1.3: Agregar Dependencias en `pubspec.yaml`

```yaml
name: adopets_app
description: AdoPets Mobile Application

dependencies:
  flutter:
    sdk: flutter
  
  # Firebase Core (Requerido)
  firebase_core: ^2.24.0
  
  # Firebase Cloud Messaging
  firebase_messaging: ^14.7.6
  
  # Local Notifications (Para mostrar notificaciones)
  flutter_local_notifications: ^16.3.0
  
  # HTTP para comunicación con backend
  http: ^1.1.2
  
  # SharedPreferences para guardar token
  shared_preferences: ^2.2.2
  
  # Permisos
  permission_handler: ^11.1.0
```

```bash
flutter pub get
```

---

## 2. Configuración Android

### Paso 2.1: Actualizar `android/app/build.gradle`

```gradle
android {
    compileSdkVersion 34
    
    defaultConfig {
        applicationId "com.adopets.app"
        minSdkVersion 21  // Mínimo para FCM
        targetSdkVersion 34
        versionCode 1
        versionName "1.0.0"
        
        // Multidex para evitar problemas con Firebase
        multiDexEnabled true
    }
}

dependencies {
    // ... otras dependencias
    
    // Google Services
    implementation 'com.google.firebase:firebase-messaging:23.4.0'
    implementation 'com.android.support:multidex:1.0.3'
}

// Al final del archivo
apply plugin: 'com.google.gms.google-services'
```

### Paso 2.2: Actualizar `android/build.gradle`

```gradle
buildscript {
    dependencies {
        classpath 'com.android.tools.build:gradle:8.1.0'
        classpath 'com.google.gms:google-services:4.4.0'  // Agregar esta línea
    }
}
```

### Paso 2.3: Configurar `android/app/src/main/AndroidManifest.xml`

```xml
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    
    <!-- Permisos -->
    <uses-permission android:name="android.permission.INTERNET"/>
    <uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED"/>
    <uses-permission android:name="android.permission.VIBRATE"/>
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>

    <application
        android:label="AdoPets"
        android:name="${applicationName}"
        android:icon="@mipmap/ic_launcher">
        
        <!-- ... MainActivity ... -->

        <!-- Firebase Messaging Service -->
        <service
            android:name="com.google.firebase.messaging.FirebaseMessagingService"
            android:exported="false">
            <intent-filter>
                <action android:name="com.google.firebase.MESSAGING_EVENT" />
            </intent-filter>
        </service>

        <!-- Canal de Notificaciones -->
        <meta-data
            android:name="com.google.firebase.messaging.default_notification_channel_id"
            android:value="citas_recordatorios" />
            
        <!-- Color de Notificaciones -->
        <meta-data
            android:name="com.google.firebase.messaging.default_notification_color"
            android:resource="@color/notification_color" />
            
        <!-- Icono de Notificaciones -->
        <meta-data
            android:name="com.google.firebase.messaging.default_notification_icon"
            android:resource="@drawable/ic_notification" />
    </application>
</manifest>
```

### Paso 2.4: Agregar `google-services.json`

1. Descargar de Firebase Console: https://console.firebase.google.com/
2. Proyecto `adopets-c99db`
3. Configuración del proyecto ? Android
4. Descargar `google-services.json`
5. Colocar en: `android/app/google-services.json`

---

## 3. Configuración iOS

### Paso 3.1: Actualizar `ios/Runner/Info.plist`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <!-- ... otras configuraciones ... -->
    
    <!-- Permisos de Notificaciones -->
    <key>UIBackgroundModes</key>
    <array>
        <string>fetch</string>
        <string>remote-notification</string>
    </array>
    
    <!-- Firebase Config File -->
    <key>FirebaseAppDelegateProxyEnabled</key>
    <false/>
</dict>
</plist>
```

### Paso 3.2: Actualizar `ios/Runner/AppDelegate.swift`

```swift
import UIKit
import Flutter
import FirebaseCore
import FirebaseMessaging
import UserNotifications

@UIApplicationMain
@objc class AppDelegate: FlutterAppDelegate {
  override func application(
    _ application: UIApplication,
    didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?
  ) -> Bool {
    
    // Inicializar Firebase
    FirebaseApp.configure()
    
    // Configurar notificaciones
    if #available(iOS 10.0, *) {
      UNUserNotificationCenter.current().delegate = self
      
      let authOptions: UNAuthorizationOptions = [.alert, .badge, .sound]
      UNUserNotificationCenter.current().requestAuthorization(
        options: authOptions,
        completionHandler: { _, _ in }
      )
    } else {
      let settings: UIUserNotificationSettings =
        UIUserNotificationSettings(types: [.alert, .badge, .sound], categories: nil)
      application.registerUserNotificationSettings(settings)
    }
    
    application.registerForRemoteNotifications()
    
    GeneratedPluginRegistrant.register(with: self)
    return super.application(application, didFinishLaunchingWithOptions: launchOptions)
  }
  
  // Manejar token de APNs
  override func application(_ application: UIApplication,
                            didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
    Messaging.messaging().apnsToken = deviceToken
  }
}
```

### Paso 3.3: Agregar `GoogleService-Info.plist`

1. Descargar de Firebase Console
2. Proyecto `adopets-c99db`
3. Configuración del proyecto ? iOS
4. Descargar `GoogleService-Info.plist`
5. Arrastrar a Xcode en `Runner/Runner`
6. ? Marcar "Copy items if needed"

### Paso 3.4: Configurar Capacidades en Xcode

1. Abrir `ios/Runner.xcworkspace` en Xcode
2. Seleccionar Target "Runner"
3. "Signing & Capabilities"
4. Agregar capacidad: **Push Notifications**
5. Agregar capacidad: **Background Modes**
   - ? Remote notifications

---

## 4. Implementación en Flutter

### Paso 4.1: Crear Servicio de Notificaciones

Crear archivo: `lib/services/notification_service.dart`

```dart
import 'dart:async';
import 'dart:io';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:permission_handler/permission_handler.dart';

// Handler para mensajes en background
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  print('?? Mensaje en background: ${message.messageId}');
  print('Título: ${message.notification?.title}');
  print('Cuerpo: ${message.notification?.body}');
  print('Data: ${message.data}');
}

class NotificationService {
  static final NotificationService _instance = NotificationService._internal();
  factory NotificationService() => _instance;
  NotificationService._internal();

  final FirebaseMessaging _firebaseMessaging = FirebaseMessaging.instance;
  final FlutterLocalNotificationsPlugin _localNotifications = 
      FlutterLocalNotificationsPlugin();

  String? _fcmToken;
  String? get fcmToken => _fcmToken;

  // Stream controller para notificaciones
  final _notificationStreamController = StreamController<RemoteMessage>.broadcast();
  Stream<RemoteMessage> get notificationStream => _notificationStreamController.stream;

  /// Inicializar servicio de notificaciones
  Future<void> initialize() async {
    print('?? Inicializando servicio de notificaciones...');

    // 1. Solicitar permisos
    await _requestPermissions();

    // 2. Configurar notificaciones locales
    await _initializeLocalNotifications();

    // 3. Configurar Firebase Messaging
    await _configureFirebaseMessaging();

    // 4. Obtener token FCM
    await _getFCMToken();

    // 5. Configurar listeners
    _setupMessageHandlers();

    print('? Servicio de notificaciones inicializado');
  }

  /// Solicitar permisos de notificaciones
  Future<void> _requestPermissions() async {
    if (Platform.isIOS) {
      await Permission.notification.request();
    }

    // Firebase Messaging permissions
    NotificationSettings settings = await _firebaseMessaging.requestPermission(
      alert: true,
      announcement: false,
      badge: true,
      carPlay: false,
      criticalAlert: false,
      provisional: false,
      sound: true,
    );

    print('?? Permisos de notificación: ${settings.authorizationStatus}');
  }

  /// Inicializar notificaciones locales
  Future<void> _initializeLocalNotifications() async {
    // Android
    const AndroidInitializationSettings initializationSettingsAndroid =
        AndroidInitializationSettings('@drawable/ic_notification');

    // iOS
    final DarwinInitializationSettings initializationSettingsIOS =
        DarwinInitializationSettings(
      requestAlertPermission: true,
      requestBadgePermission: true,
      requestSoundPermission: true,
      onDidReceiveLocalNotification: _onDidReceiveLocalNotification,
    );

    final InitializationSettings initializationSettings = InitializationSettings(
      android: initializationSettingsAndroid,
      iOS: initializationSettingsIOS,
    );

    await _localNotifications.initialize(
      initializationSettings,
      onDidReceiveNotificationResponse: _onNotificationTapped,
    );

    // Crear canal de notificaciones para Android
    if (Platform.isAndroid) {
      await _createNotificationChannel();
    }
  }

  /// Crear canal de notificaciones (Android)
  Future<void> _createNotificationChannel() async {
    const AndroidNotificationChannel channel = AndroidNotificationChannel(
      'citas_recordatorios', // ID (debe coincidir con backend)
      'Recordatorios de Citas', // Nombre
      description: 'Notificaciones de recordatorios de citas veterinarias',
      importance: Importance.high,
      playSound: true,
      enableVibration: true,
    );

    await _localNotifications
        .resolvePlatformSpecificImplementation<
            AndroidFlutterLocalNotificationsPlugin>()
        ?.createNotificationChannel(channel);
  }

  /// Configurar Firebase Messaging
  Future<void> _configureFirebaseMessaging() async {
    // Configurar opciones de presentación (iOS)
    await _firebaseMessaging.setForegroundNotificationPresentationOptions(
      alert: true,
      badge: true,
      sound: true,
    );
  }

  /// Obtener token FCM
  Future<String?> _getFCMToken() async {
    try {
      _fcmToken = await _firebaseMessaging.getToken();
      print('?? FCM Token: $_fcmToken');
      
      // Guardar token localmente
      if (_fcmToken != null) {
        final prefs = await SharedPreferences.getInstance();
        await prefs.setString('fcm_token', _fcmToken!);
      }
      
      return _fcmToken;
    } catch (e) {
      print('? Error al obtener FCM token: $e');
      return null;
    }
  }

  /// Configurar listeners de mensajes
  void _setupMessageHandlers() {
    // 1. Mensaje recibido cuando la app está en FOREGROUND
    FirebaseMessaging.onMessage.listen((RemoteMessage message) {
      print('?? Mensaje recibido en foreground:');
      print('Título: ${message.notification?.title}');
      print('Cuerpo: ${message.notification?.body}');
      print('Data: ${message.data}');

      // Mostrar notificación local
      _showLocalNotification(message);

      // Emitir evento
      _notificationStreamController.add(message);
    });

    // 2. Usuario TAP en notificación (app en background o terminada)
    FirebaseMessaging.onMessageOpenedApp.listen((RemoteMessage message) {
      print('?? Notificación abierta desde background:');
      print('Data: ${message.data}');
      
      // Navegar según el tipo de notificación
      _handleNotificationTap(message);
    });

    // 3. Verificar si la app se abrió desde una notificación
    _checkInitialMessage();

    // 4. Token refresh listener
    _firebaseMessaging.onTokenRefresh.listen((newToken) {
      print('?? Token FCM actualizado: $newToken');
      _fcmToken = newToken;
      // TODO: Actualizar token en el backend
      _updateTokenOnBackend(newToken);
    });
  }

  /// Verificar mensaje inicial (app abierta desde notificación)
  Future<void> _checkInitialMessage() async {
    RemoteMessage? initialMessage = await _firebaseMessaging.getInitialMessage();
    
    if (initialMessage != null) {
      print('?? App abierta desde notificación:');
      print('Data: ${initialMessage.data}');
      
      // Navegar según el tipo
      _handleNotificationTap(initialMessage);
    }
  }

  /// Mostrar notificación local
  Future<void> _showLocalNotification(RemoteMessage message) async {
    final notification = message.notification;
    if (notification == null) return;

    const AndroidNotificationDetails androidDetails = AndroidNotificationDetails(
      'citas_recordatorios',
      'Recordatorios de Citas',
      channelDescription: 'Notificaciones de recordatorios de citas veterinarias',
      importance: Importance.high,
      priority: Priority.high,
      color: Color(0xFF4CAF50),
      playSound: true,
      enableVibration: true,
      icon: '@drawable/ic_notification',
    );

    const DarwinNotificationDetails iOSDetails = DarwinNotificationDetails(
      presentAlert: true,
      presentBadge: true,
      presentSound: true,
    );

    const NotificationDetails notificationDetails = NotificationDetails(
      android: androidDetails,
      iOS: iOSDetails,
    );

    await _localNotifications.show(
      notification.hashCode,
      notification.title,
      notification.body,
      notificationDetails,
      payload: message.data.toString(),
    );
  }

  /// Manejar tap en notificación
  void _handleNotificationTap(RemoteMessage message) {
    final tipo = message.data['tipo'];
    
    switch (tipo) {
      case 'recordatorio_cita':
        final citaId = message.data['citaId'];
        // TODO: Navegar a detalles de cita
        print('Navegar a cita: $citaId');
        break;
      
      case 'cita_confirmada':
        final citaId = message.data['citaId'];
        // TODO: Navegar a cita confirmada
        print('Navegar a cita confirmada: $citaId');
        break;
        
      default:
        print('Tipo de notificación desconocido: $tipo');
    }
  }

  /// Callback para notificaciones locales (iOS)
  void _onDidReceiveLocalNotification(
      int id, String? title, String? body, String? payload) {
    print('Notificación local recibida en iOS');
  }

  /// Callback para tap en notificación local
  void _onNotificationTapped(NotificationResponse response) {
    print('Notificación local tapeada: ${response.payload}');
  }

  /// Actualizar token en backend
  Future<void> _updateTokenOnBackend(String token) async {
    // TODO: Implementar llamada al backend
    print('TODO: Actualizar token en backend: $token');
  }

  /// Suscribirse a un topic
  Future<void> subscribeToTopic(String topic) async {
    try {
      await _firebaseMessaging.subscribeToTopic(topic);
      print('? Suscrito al topic: $topic');
    } catch (e) {
      print('? Error al suscribirse al topic $topic: $e');
    }
  }

  /// Desuscribirse de un topic
  Future<void> unsubscribeFromTopic(String topic) async {
    try {
      await _firebaseMessaging.unsubscribeFromTopic(topic);
      print('? Desuscrito del topic: $topic');
    } catch (e) {
      print('? Error al desuscribirse del topic $topic: $e');
    }
  }

  /// Limpiar
  void dispose() {
    _notificationStreamController.close();
  }
}
```

### Paso 4.2: Inicializar en `main.dart`

```dart
import 'package:flutter/material.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'firebase_options.dart';
import 'services/notification_service.dart';

// Background message handler (debe estar fuera de la clase)
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  await Firebase.initializeApp(options: DefaultFirebaseOptions.currentPlatform);
  print('?? Handling background message: ${message.messageId}');
}

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  
  // Inicializar Firebase
  await Firebase.initializeApp(
    options: DefaultFirebaseOptions.currentPlatform,
  );
  
  // Configurar background message handler
  FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);
  
  // Inicializar servicio de notificaciones
  await NotificationService().initialize();
  
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'AdoPets',
      theme: ThemeData(
        primarySwatch: Colors.green,
        useMaterial3: true,
      ),
      home: const HomePage(),
    );
  }
}

class HomePage extends StatefulWidget {
  const HomePage({Key? key}) : super(key: key);

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  final NotificationService _notificationService = NotificationService();

  @override
  void initState() {
    super.initState();
    _setupNotificationListener();
  }

  void _setupNotificationListener() {
    // Escuchar notificaciones en foreground
    _notificationService.notificationStream.listen((message) {
      print('Nueva notificación recibida en UI');
      
      // Mostrar snackbar o diálogo
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(message.notification?.body ?? 'Nueva notificación'),
          action: SnackBarAction(
            label: 'Ver',
            onPressed: () {
              // Navegar a la pantalla correspondiente
            },
          ),
        ),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('AdoPets'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.pets, size: 100, color: Colors.green),
            const SizedBox(height: 20),
            const Text(
              'Bienvenido a AdoPets',
              style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 40),
            ElevatedButton.icon(
              onPressed: () {
                print('FCM Token: ${_notificationService.fcmToken}');
              },
              icon: const Icon(Icons.info),
              label: const Text('Ver Token FCM'),
            ),
          ],
        ),
      ),
    );
  }
}
```

---

## 5. Integración con Backend

### Paso 5.1: Crear Servicio de API

Crear archivo: `lib/services/api_service.dart`

```dart
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class ApiService {
  static const String baseUrl = 'https://api.adopets.com/api';
  
  /// Registrar dispositivo en el backend
  static Future<bool> registrarDispositivo(String fcmToken) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final jwtToken = prefs.getString('jwt_token');
      
      if (jwtToken == null) {
        print('? No hay JWT token');
        return false;
      }

      final response = await http.post(
        Uri.parse('$baseUrl/dispositivos'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer $jwtToken',
        },
        body: jsonEncode({
          'token': fcmToken,
          'plataforma': 2, // 2 = Android, 3 = iOS
          'appVersion': '1.0.0',
        }),
      );

      if (response.statusCode == 200) {
        print('? Dispositivo registrado en backend');
        return true;
      } else {
        print('? Error al registrar dispositivo: ${response.body}');
        return false;
      }
    } catch (e) {
      print('? Error de red: $e');
      return false;
    }
  }

  /// Obtener dispositivos registrados
  static Future<List<dynamic>> obtenerDispositivos() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final jwtToken = prefs.getString('jwt_token');
      
      if (jwtToken == null) {
        return [];
      }

      final response = await http.get(
        Uri.parse('$baseUrl/dispositivos'),
        headers: {
          'Authorization': 'Bearer $jwtToken',
        },
      );

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return data['data'] ?? [];
      }
      
      return [];
    } catch (e) {
      print('? Error: $e');
      return [];
    }
  }

  /// Deshabilitar dispositivo
  static Future<bool> deshabilitarDispositivo(String dispositivoId) async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final jwtToken = prefs.getString('jwt_token');
      
      if (jwtToken == null) {
        return false;
      }

      final response = await http.put(
        Uri.parse('$baseUrl/dispositivos/$dispositivoId/deshabilitar'),
        headers: {
          'Authorization': 'Bearer $jwtToken',
        },
      );

      return response.statusCode == 200;
    } catch (e) {
      print('? Error: $e');
      return false;
    }
  }
}
```

### Paso 5.2: Registrar Dispositivo al Autenticarse

```dart
// En tu pantalla de login, después de autenticación exitosa:
import 'services/notification_service.dart';
import 'services/api_service.dart';

Future<void> _onLoginSuccess() async {
  // Obtener token FCM
  final fcmToken = NotificationService().fcmToken;
  
  if (fcmToken != null) {
    // Registrar en backend
    await ApiService.registrarDispositivo(fcmToken);
  }
  
  // Navegar a home
  Navigator.pushReplacement(
    context,
    MaterialPageRoute(builder: (context) => const HomePage()),
  );
}
```

---

## 6. Manejo de Notificaciones

### Navegación según Tipo de Notificación

```dart
void _handleNotificationTap(RemoteMessage message) {
  final tipo = message.data['tipo'];
  final citaId = message.data['citaId'];
  
  switch (tipo) {
    case 'recordatorio_cita':
      // Navegar a detalles de cita
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => CitaDetallesScreen(citaId: citaId),
        ),
      );
      break;
      
    case 'cita_confirmada':
      // Navegar a citas confirmadas
      Navigator.push(
        context,
        MaterialPageRoute(
          builder: (context) => MisCitasScreen(),
        ),
      );
      break;
      
    case 'cita_cancelada':
      // Mostrar diálogo
      showDialog(
        context: context,
        builder: (context) => AlertDialog(
          title: const Text('Cita Cancelada'),
          content: Text(message.notification?.body ?? ''),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(context),
              child: const Text('OK'),
            ),
          ],
        ),
      );
      break;
  }
}
```

---

## 7. Testing

### Paso 7.1: Testing en Desarrollo

```bash
# Ejecutar en Android
flutter run

# Ejecutar en iOS
flutter run

# Ver logs
flutter logs
```

### Paso 7.2: Enviar Notificación de Prueba desde Firebase Console

1. Ir a: https://console.firebase.google.com/
2. Proyecto `adopets-c99db`
3. Cloud Messaging
4. "Send your first message"
5. Título: "?? Recordatorio de Prueba"
6. Mensaje: "Tu cita es en 24 horas"
7. Seleccionar app: `com.adopets.app`
8. Target: "Single device"
9. Token FCM: (Copiar de los logs de tu app)
10. Enviar

### Paso 7.3: Testing desde Backend

```bash
# Ejecutar job de recordatorios manualmente
curl -X POST https://api.adopets.com/api/recordatorios/ejecutar-ahora \
  -H "Authorization: Bearer {tu-jwt-token}"
```

### Paso 7.4: Verificar Logs

```dart
// En tu app, deberías ver:
print('?? FCM Token: abc123...');
print('? Dispositivo registrado en backend');
print('?? Mensaje recibido en foreground: Recordatorio de Cita');
```

---

## ?? Checklist de Implementación

- [ ] Firebase CLI instalado
- [ ] FlutterFire configurado (`flutterfire configure`)
- [ ] Dependencias agregadas en `pubspec.yaml`
- [ ] `google-services.json` en `android/app/`
- [ ] `GoogleService-Info.plist` en `ios/Runner/`
- [ ] Configuración Android (`AndroidManifest.xml`)
- [ ] Configuración iOS (`AppDelegate.swift`, `Info.plist`)
- [ ] Servicio de notificaciones creado
- [ ] Inicialización en `main.dart`
- [ ] Integración con backend (registro de dispositivo)
- [ ] Manejo de navegación por tipo de notificación
- [ ] Testing con Firebase Console
- [ ] Testing con backend real

---

## ?? Troubleshooting

### Problema: No recibo notificaciones en Android

**Solución**:
1. Verificar que `google-services.json` esté en `android/app/`
2. Verificar permisos en `AndroidManifest.xml`
3. Verificar que el token FCM se obtuvo correctamente
4. Revisar logs: `flutter logs`

### Problema: No recibo notificaciones en iOS

**Solución**:
1. Verificar que `GoogleService-Info.plist` esté en `ios/Runner/`
2. Verificar capacidades en Xcode (Push Notifications, Background Modes)
3. Verificar certificado APNs en Firebase Console
4. Verificar que el dispositivo físico esté siendo usado (no simulador)

### Problema: Token FCM es null

**Solución**:
```dart
// Forzar obtención del token
final token = await FirebaseMessaging.instance.getToken(
  vapidKey: 'YOUR_VAPID_KEY', // Solo para web
);
print('Token: $token');
```

### Problema: Notificaciones no aparecen en foreground

**Solución**:
- Verificar que `_showLocalNotification()` esté siendo llamado
- Verificar permisos de notificaciones
- Usar `flutter_local_notifications` para mostrar notificaciones locales

---

## ?? Recursos

- [Firebase Flutter Setup](https://firebase.flutter.dev/docs/overview)
- [Flutter Local Notifications](https://pub.dev/packages/flutter_local_notifications)
- [Firebase Cloud Messaging](https://firebase.google.com/docs/cloud-messaging/flutter/client)
- [Testing Firebase Messaging](https://firebase.google.com/docs/cloud-messaging/testing-and-troubleshooting)

---

## ? Estado Final

| Componente | Estado |
|------------|--------|
| Firebase configurado | ? Listo |
| Android setup | ? Completo |
| iOS setup | ? Completo |
| NotificationService | ? Implementado |
| Integración con backend | ? Completo |
| Navegación | ? Implementado |
| Testing | ? Documentado |

---

**¡Push Notifications listas en la app móvil! ????**

**Fecha**: 2024-01-15  
**Plataformas**: Android & iOS  
**Framework**: Flutter
