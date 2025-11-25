# ?? Guía de Integración del Chatbot AdoPets - Frontend (App Móvil)

## ?? Resumen

El chatbot de AdoPets permite a los usuarios adoptantes obtener información sobre adopciones, mascotas, cuidados y servicios del refugio mediante conversaciones con IA (Groq/LLaMA 3.3).

---

## ?? Endpoints Disponibles

### Base URL
```
https://tu-api.azurewebsites.net/api/v1/Chat
```

---

## ?? 1. Enviar Mensaje al Chatbot

### **POST** `/api/v1/Chat/ask`

Envía un mensaje al chatbot y recibe una respuesta. Puede crear una nueva conversación o continuar una existente.

#### Request Headers
```http
Content-Type: application/json
```

#### Request Body
```json
{
  "userId": "string",              // ID del usuario autenticado (GUID o string)
  "message": "string",             // Mensaje del usuario
  "conversationId": "guid|null"    // null para nueva conversación, guid para continuar
}
```

#### Response 200 (Éxito)
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "answer": "¡Hola! Para adoptar una mascota necesitas..."
}
```

#### Response 400 (Error de validación)
```json
{
  "error": "Los campos 'userId' y 'message' son obligatorios."
}
```

#### Response 500 (Error del servidor)
```json
{
  "error": "Error al comunicarse con el servicio de IA."
}
```

---

## ?? 2. Obtener Historial de una Conversación

### **GET** `/api/v1/Chat/conversation/{conversationId}?userId={userId}`

Obtiene todos los mensajes de una conversación específica.

#### Request Parameters
- **Path**: `conversationId` (GUID)
- **Query**: `userId` (string)

#### Request Example
```
GET /api/v1/Chat/conversation/3fa85f64-5717-4562-b3fc-2c963f66afa6?userId=user123
```

#### Response 200 (Éxito)
```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "user123",
  "createdAt": "2024-01-15T10:30:00Z",
  "messages": [
    {
      "id": "msg-001",
      "role": "user",
      "content": "¿Cómo adopto un perro?",
      "createdAt": "2024-01-15T10:30:00Z"
    },
    {
      "id": "msg-002",
      "role": "assistant",
      "content": "Para adoptar un perro necesitas...",
      "createdAt": "2024-01-15T10:30:05Z"
    }
  ]
}
```

#### Response 404 (No encontrada)
```json
{
  "error": "Conversación no encontrada."
}
```

---

## ?? 3. Obtener Todas las Conversaciones del Usuario

### **GET** `/api/v1/Chat/conversations?userId={userId}`

Lista todas las conversaciones del usuario ordenadas por fecha (más reciente primero).

#### Request Parameters
- **Query**: `userId` (string)

#### Request Example
```
GET /api/v1/Chat/conversations?userId=user123
```

#### Response 200 (Éxito)
```json
[
  {
    "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "createdAt": "2024-01-15T10:30:00Z",
    "messageCount": 8,
    "lastMessage": {
      "content": "¡Perfecto! Te esperamos.",
      "createdAt": "2024-01-15T10:45:00Z"
    }
  },
  {
    "conversationId": "7cb92e1a-8934-4123-a456-9d8e7f6c5b4a",
    "createdAt": "2024-01-14T15:20:00Z",
    "messageCount": 3,
    "lastMessage": {
      "content": "Tenemos varios gatos disponibles...",
      "createdAt": "2024-01-14T15:22:00Z"
    }
  }
]
```

---

## ?? Ejemplos de Implementación

### ?? Flutter/Dart

#### 1. Modelo de Datos
```dart
class ChatMessage {
  final String id;
  final String role;
  final String content;
  final DateTime createdAt;

  ChatMessage({
    required this.id,
    required this.role,
    required this.content,
    required this.createdAt,
  });

  factory ChatMessage.fromJson(Map<String, dynamic> json) {
    return ChatMessage(
      id: json['id'],
      role: json['role'],
      content: json['content'],
      createdAt: DateTime.parse(json['createdAt']),
    );
  }
}

class ChatConversation {
  final String conversationId;
  final String userId;
  final DateTime createdAt;
  final List<ChatMessage> messages;

  ChatConversation({
    required this.conversationId,
    required this.userId,
    required this.createdAt,
    required this.messages,
  });

  factory ChatConversation.fromJson(Map<String, dynamic> json) {
    return ChatConversation(
      conversationId: json['conversationId'],
      userId: json['userId'],
      createdAt: DateTime.parse(json['createdAt']),
      messages: (json['messages'] as List)
          .map((m) => ChatMessage.fromJson(m))
          .toList(),
    );
  }
}
```

#### 2. Servicio de Chat
```dart
import 'package:http/http.dart' as http;
import 'dart:convert';

class ChatService {
  final String baseUrl = 'https://tu-api.azurewebsites.net/api/v1/Chat';
  
  // Enviar mensaje al chatbot
  Future<Map<String, dynamic>> sendMessage({
    required String userId,
    required String message,
    String? conversationId,
  }) async {
    final url = Uri.parse('$baseUrl/ask');
    
    final response = await http.post(
      url,
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'userId': userId,
        'message': message,
        'conversationId': conversationId,
      }),
    );

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    } else {
      throw Exception('Error: ${response.body}');
    }
  }

  // Obtener historial de conversación
  Future<ChatConversation> getConversation({
    required String conversationId,
    required String userId,
  }) async {
    final url = Uri.parse('$baseUrl/conversation/$conversationId?userId=$userId');
    
    final response = await http.get(url);

    if (response.statusCode == 200) {
      return ChatConversation.fromJson(jsonDecode(response.body));
    } else {
      throw Exception('Error: ${response.body}');
    }
  }

  // Obtener todas las conversaciones del usuario
  Future<List<Map<String, dynamic>>> getUserConversations({
    required String userId,
  }) async {
    final url = Uri.parse('$baseUrl/conversations?userId=$userId');
    
    final response = await http.get(url);

    if (response.statusCode == 200) {
      return List<Map<String, dynamic>>.from(jsonDecode(response.body));
    } else {
      throw Exception('Error: ${response.body}');
    }
  }
}
```

#### 3. Pantalla de Chat (Ejemplo Básico)
```dart
class ChatScreen extends StatefulWidget {
  final String userId;
  final String? conversationId;

  const ChatScreen({
    required this.userId,
    this.conversationId,
  });

  @override
  _ChatScreenState createState() => _ChatScreenState();
}

class _ChatScreenState extends State<ChatScreen> {
  final ChatService _chatService = ChatService();
  final TextEditingController _messageController = TextEditingController();
  final List<ChatMessage> _messages = [];
  String? _currentConversationId;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _currentConversationId = widget.conversationId;
    
    // Si ya existe una conversación, cargar historial
    if (_currentConversationId != null) {
      _loadConversationHistory();
    }
  }

  Future<void> _loadConversationHistory() async {
    try {
      final conversation = await _chatService.getConversation(
        conversationId: _currentConversationId!,
        userId: widget.userId,
      );
      
      setState(() {
        _messages.clear();
        _messages.addAll(conversation.messages);
      });
    } catch (e) {
      print('Error loading history: $e');
    }
  }

  Future<void> _sendMessage() async {
    if (_messageController.text.trim().isEmpty) return;

    final userMessage = _messageController.text.trim();
    _messageController.clear();

    // Agregar mensaje del usuario a la UI
    setState(() {
      _messages.add(ChatMessage(
        id: DateTime.now().toString(),
        role: 'user',
        content: userMessage,
        createdAt: DateTime.now(),
      ));
      _isLoading = true;
    });

    try {
      // Enviar mensaje al backend
      final response = await _chatService.sendMessage(
        userId: widget.userId,
        message: userMessage,
        conversationId: _currentConversationId,
      );

      // Actualizar conversationId si es nueva
      if (_currentConversationId == null) {
        _currentConversationId = response['conversationId'];
      }

      // Agregar respuesta del bot a la UI
      setState(() {
        _messages.add(ChatMessage(
          id: DateTime.now().toString(),
          role: 'assistant',
          content: response['answer'],
          createdAt: DateTime.now(),
        ));
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      
      // Mostrar error
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error: $e')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Chat AdoPets'),
      ),
      body: Column(
        children: [
          // Lista de mensajes
          Expanded(
            child: ListView.builder(
              reverse: true,
              itemCount: _messages.length,
              itemBuilder: (context, index) {
                final message = _messages[_messages.length - 1 - index];
                final isUser = message.role == 'user';
                
                return Align(
                  alignment: isUser ? Alignment.centerRight : Alignment.centerLeft,
                  child: Container(
                    margin: EdgeInsets.all(8),
                    padding: EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: isUser ? Colors.blue[100] : Colors.grey[200],
                      borderRadius: BorderRadius.circular(12),
                    ),
                    child: Text(message.content),
                  ),
                );
              },
            ),
          ),
          
          // Indicador de carga
          if (_isLoading)
            Padding(
              padding: EdgeInsets.all(8),
              child: CircularProgressIndicator(),
            ),
          
          // Input de mensaje
          Padding(
            padding: EdgeInsets.all(8),
            child: Row(
              children: [
                Expanded(
                  child: TextField(
                    controller: _messageController,
                    decoration: InputDecoration(
                      hintText: 'Escribe tu mensaje...',
                      border: OutlineInputBorder(),
                    ),
                    onSubmitted: (_) => _sendMessage(),
                  ),
                ),
                SizedBox(width: 8),
                IconButton(
                  icon: Icon(Icons.send),
                  onPressed: _sendMessage,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
```

#### 4. Pantalla de Lista de Conversaciones
```dart
class ConversationsListScreen extends StatefulWidget {
  final String userId;

  const ConversationsListScreen({required this.userId});

  @override
  _ConversationsListScreenState createState() => _ConversationsListScreenState();
}

class _ConversationsListScreenState extends State<ConversationsListScreen> {
  final ChatService _chatService = ChatService();
  List<Map<String, dynamic>> _conversations = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadConversations();
  }

  Future<void> _loadConversations() async {
    try {
      final conversations = await _chatService.getUserConversations(
        userId: widget.userId,
      );
      
      setState(() {
        _conversations = conversations;
        _isLoading = false;
      });
    } catch (e) {
      setState(() {
        _isLoading = false;
      });
      print('Error: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Mis Conversaciones'),
      ),
      body: _isLoading
          ? Center(child: CircularProgressIndicator())
          : _conversations.isEmpty
              ? Center(child: Text('No hay conversaciones'))
              : ListView.builder(
                  itemCount: _conversations.length,
                  itemBuilder: (context, index) {
                    final conv = _conversations[index];
                    final lastMsg = conv['lastMessage'];
                    
                    return ListTile(
                      title: Text(
                        lastMsg != null ? lastMsg['content'] : 'Nueva conversación',
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      subtitle: Text(
                        '${conv['messageCount']} mensajes',
                      ),
                      trailing: Text(
                        _formatDate(DateTime.parse(conv['createdAt'])),
                      ),
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder: (context) => ChatScreen(
                              userId: widget.userId,
                              conversationId: conv['conversationId'],
                            ),
                          ),
                        );
                      },
                    );
                  },
                ),
      floatingActionButton: FloatingActionButton(
        child: Icon(Icons.add),
        onPressed: () {
          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => ChatScreen(
                userId: widget.userId,
                conversationId: null, // Nueva conversación
              ),
            ),
          );
        },
      ),
    );
  }

  String _formatDate(DateTime date) {
    final now = DateTime.now();
    final diff = now.difference(date);
    
    if (diff.inDays == 0) {
      return '${date.hour}:${date.minute.toString().padLeft(2, '0')}';
    } else if (diff.inDays == 1) {
      return 'Ayer';
    } else {
      return '${date.day}/${date.month}/${date.year}';
    }
  }
}
```

---

## ?? Seguridad y Mejores Prácticas

### 1. **Autenticación (Recomendado)**
En producción, deberías obtener el `userId` del token JWT en lugar de enviarlo en el body:

```dart
// Agregar header de autenticación
final response = await http.post(
  url,
  headers: {
    'Content-Type': 'application/json',
    'Authorization': 'Bearer $jwtToken',
  },
  body: jsonEncode({
    'message': message,
    'conversationId': conversationId,
  }),
);
```

### 2. **Manejo de Errores**
```dart
try {
  final response = await _chatService.sendMessage(...);
} on SocketException {
  // Error de conexión a internet
  showError('Verifica tu conexión a internet');
} on TimeoutException {
  // Timeout
  showError('La petición tardó demasiado');
} catch (e) {
  // Otros errores
  showError('Ocurrió un error inesperado');
}
```

### 3. **Guardar Estado Local (Opcional)**
Para mejor UX, puedes guardar conversaciones en almacenamiento local:

```dart
import 'package:shared_preferences/shared_preferences.dart';

// Guardar conversación ID
Future<void> saveLastConversationId(String id) async {
  final prefs = await SharedPreferences.getInstance();
  await prefs.setString('last_conversation_id', id);
}

// Recuperar conversación ID
Future<String?> getLastConversationId() async {
  final prefs = await SharedPreferences.getInstance();
  return prefs.getString('last_conversation_id');
}
```

---

## ?? Mejoras de UX Sugeridas

### 1. **Indicador de Escritura**
```dart
if (_isLoading)
  Row(
    children: [
      SizedBox(width: 12),
      Text('El asistente está escribiendo'),
      SizedBox(width: 8),
      SizedBox(
        width: 20,
        height: 20,
        child: CircularProgressIndicator(strokeWidth: 2),
      ),
    ],
  )
```

### 2. **Botones de Sugerencias**
```dart
Wrap(
  spacing: 8,
  children: [
    ActionChip(
      label: Text('¿Cómo adoptar?'),
      onPressed: () => _sendMessage('¿Cómo puedo adoptar una mascota?'),
    ),
    ActionChip(
      label: Text('Mascotas disponibles'),
      onPressed: () => _sendMessage('¿Qué mascotas están disponibles?'),
    ),
    ActionChip(
      label: Text('Requisitos'),
      onPressed: () => _sendMessage('¿Qué requisitos necesito?'),
    ),
  ],
)
```

### 3. **Scroll Automático**
```dart
final ScrollController _scrollController = ScrollController();

void _scrollToBottom() {
  WidgetsBinding.instance.addPostFrameCallback((_) {
    if (_scrollController.hasClients) {
      _scrollController.animateTo(
        0,
        duration: Duration(milliseconds: 300),
        curve: Curves.easeOut,
      );
    }
  });
}
```

---

## ?? Límites y Consideraciones

| Aspecto | Valor | Notas |
|---------|-------|-------|
| **Max tokens por respuesta** | 500 | Configurado en el backend |
| **Historial en contexto** | 10 mensajes | Últimos 10 mensajes |
| **Modelo IA** | LLaMA 3.3 70B | Via Groq API |
| **Timeout recomendado** | 30 segundos | Para llamadas HTTP |
| **Rate limiting** | Depende de Groq | Verificar límites de API |

---

## ?? Debugging

### Ver logs en el backend
```bash
# Logs de Azure App Service
az webapp log tail --name tu-app --resource-group tu-resource-group
```

### Logs útiles en la app
```dart
print('Sending message: $userMessage');
print('Current conversation: $_currentConversationId');
print('Response: ${response.body}');
```

---

## ? Checklist de Implementación

- [ ] Crear modelos de datos (`ChatMessage`, `ChatConversation`)
- [ ] Implementar servicio de chat (`ChatService`)
- [ ] Crear pantalla de chat con UI
- [ ] Implementar lista de conversaciones
- [ ] Agregar manejo de errores
- [ ] Implementar indicador de carga
- [ ] Guardar conversación ID localmente
- [ ] Agregar scroll automático
- [ ] Implementar sugerencias rápidas
- [ ] Probar flujo completo
- [ ] Agregar autenticación JWT (recomendado)

---

## ?? Soporte

Si tienes dudas sobre la integración, revisa:
- Swagger de la API: `https://tu-api.azurewebsites.net/`
- Logs del backend en Azure Portal
- Código fuente del controlador: `Controllers/ChatController.cs`

---

**¡Listo para integrar! ??**
