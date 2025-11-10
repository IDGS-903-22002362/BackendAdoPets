// ========================================
// PARCHE DE DEPURACIÓN PARA FLUTTER
// ========================================
// Copia y pega este código en tu api_service.dart

import 'dart:convert';
import 'dart:math';
import 'package:http/http.dart' as http;
import '../config/api_config.dart';
import '../models/api_response.dart';
import 'storage_service.dart';

class ApiService {
  final _storageService = StorageService();

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

    // ============ DEBUGGING - INICIO ============
    print('');
    print('?? ========================================');
    print('?? INICIO PETICIÓN HTTP POST');
    print('?? ========================================');
    print('?? URL completa: $url');
    print('?? requiresAuth: $requiresAuth');
    print('');

    if (requiresAuth) {
      print('?? Intentando obtener token...');
      final token = await _storageService.getAccessToken();
      
      print('   Token obtenido de storage:');
      print('   - Es null? ${token == null}');
      
      if (token != null) {
        print('   - Longitud: ${token.length} caracteres');
        print('   - Primeros 50 chars: ${token.substring(0, min(50, token.length))}...');
        print('   - Últimos 20 chars: ...${token.substring(token.length - 20)}');
        
        // Verificar formato
        if (token.startsWith('eyJ')) {
          print('   ? Parece un JWT válido (comienza con eyJ)');
        } else {
          print('   ?? NO parece un JWT válido (debería comenzar con eyJ)');
        }
        
        // Agregar header
        headers['Authorization'] = 'Bearer $token';
        print('   ? Header Authorization agregado');
      } else {
        print('   ? ERROR: Token es NULL');
        print('   ? NO se puede autenticar la petición');
        print('   ? El servidor responderá con 401');
      }
    } else {
      print('?? requiresAuth = false, NO se enviará token');
    }

    print('');
    print('?? Headers finales que se enviarán:');
    headers.forEach((key, value) {
      if (key == 'Authorization') {
        // Mostrar solo los primeros caracteres del token
        final preview = value.length > 50 ? '${value.substring(0, 50)}...' : value;
        print('   - $key: $preview');
      } else {
        print('   - $key: $value');
      }
    });

    print('');
    print('?? Body:');
    print(jsonEncode(body));
    print('');
    print('? Enviando petición...');
    print('');
    // ============ DEBUGGING - FIN ============

    try {
      final response = await http.post(
        url,
        headers: headers,
        body: jsonEncode(body),
      );

      // ============ DEBUGGING - RESPUESTA ============
      print('?? ========================================');
      print('?? RESPUESTA DEL SERVIDOR');
      print('?? ========================================');
      print('?? Status Code: ${response.statusCode}');
      print('?? Status: ${_getStatusMessage(response.statusCode)}');
      print('');
      print('?? Response Headers:');
      response.headers.forEach((key, value) {
        print('   - $key: $value');
      });
      print('');
      print('?? Response Body:');
      print(response.body);
      print('========================================');
      print('');
      // ============ DEBUGGING - FIN ============

      if (response.statusCode == 401) {
        print('??? ERROR 401 UNAUTHORIZED ???');
        print('');
        print('?? Diagnóstico:');
        print('   1. Verifica que el token se obtuvo correctamente arriba');
        print('   2. Verifica que el header Authorization se agregó');
        print('   3. Verifica que el token sea del backend (no de Firebase)');
        print('   4. Verifica que el token no haya expirado');
        print('');
        print('?? Soluciones:');
        print('   - Si el token es NULL: Haz login nuevamente');
        print('   - Si el token existe pero falla: Puede estar expirado, haz login nuevamente');
        print('   - Si acabas de hacer login: Verifica que guardes el accessToken del backend');
        print('');
      }

      final jsonResponse = jsonDecode(response.body);
      
      if (response.statusCode >= 200 && response.statusCode < 300) {
        return ApiResponse<T>(
          success: jsonResponse['success'] ?? true,
          data: fromJson(jsonResponse['data']),
          message: jsonResponse['message'] ?? 'Success',
        );
      } else {
        return ApiResponse<T>(
          success: false,
          message: jsonResponse['message'] ?? 'Error',
          errors: (jsonResponse['errors'] as List?)?.cast<String>() ?? [],
        );
      }
    } catch (e, stackTrace) {
      print('??? EXCEPTION CAPTURADA ???');
      print('Exception: $e');
      print('StackTrace: $stackTrace');
      print('========================================');
      
      return ApiResponse<T>(
        success: false,
        message: 'Error de conexión: $e',
        errors: [e.toString()],
      );
    }
  }

  // Método auxiliar para GET (agrega debugging similar)
  Future<ApiResponse<T>> get<T>({
    required String endpoint,
    required T Function(dynamic) fromJson,
    bool requiresAuth = true,
  }) async {
    final url = Uri.parse('${ApiConfig.baseUrl}$endpoint');
    
    final headers = <String, String>{
      'Content-Type': 'application/json',
    };

    print('');
    print('?? GET $url');
    print('?? requiresAuth: $requiresAuth');

    if (requiresAuth) {
      final token = await _storageService.getAccessToken();
      if (token != null) {
        headers['Authorization'] = 'Bearer $token';
        print('   ? Token agregado (${token.length} chars)');
      } else {
        print('   ? Token NULL');
      }
    }

    try {
      final response = await http.get(url, headers: headers);
      
      print('?? Status: ${response.statusCode}');
      
      if (response.statusCode == 401) {
        print('? 401 Unauthorized - Token inválido o expirado');
      }

      final jsonResponse = jsonDecode(response.body);
      
      if (response.statusCode >= 200 && response.statusCode < 300) {
        return ApiResponse<T>(
          success: jsonResponse['success'] ?? true,
          data: fromJson(jsonResponse['data']),
          message: jsonResponse['message'] ?? 'Success',
        );
      } else {
        return ApiResponse<T>(
          success: false,
          message: jsonResponse['message'] ?? 'Error',
          errors: (jsonResponse['errors'] as List?)?.cast<String>() ?? [],
        );
      }
    } catch (e) {
      print('? Exception: $e');
      return ApiResponse<T>(
        success: false,
        message: 'Error de conexión: $e',
        errors: [e.toString()],
      );
    }
  }

  String _getStatusMessage(int statusCode) {
    switch (statusCode) {
      case 200: return '? OK';
      case 201: return '? Created';
      case 400: return '? Bad Request';
      case 401: return '? Unauthorized (sin autenticación)';
      case 403: return '? Forbidden (sin permisos)';
      case 404: return '? Not Found';
      case 500: return '? Internal Server Error';
      default: return statusCode.toString();
    }
  }
}
