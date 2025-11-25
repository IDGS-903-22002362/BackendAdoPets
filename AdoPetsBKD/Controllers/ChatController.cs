using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using AdoPetsBKD.Infrastructure.Data;
using AdoPetsBKD.Domain.Entities.Chat;
using AdoPetsBKD.Application.DTOs.Chat;
using AdoPetsBKD.Infrastructure.Integrations.Groq;

namespace AdoPetsBKD.Controllers;

/// <summary>
/// Controlador del chatbot para usuarios adoptantes.
/// Maneja conversaciones con historial persistente usando Groq AI.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AdoPetsDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ChatController> _logger;

    private const int MAX_MESSAGES_IN_CONTEXT = 10;
    private const string GROQ_MODEL = "llama-3.3-70b-versatile";

    // Prompt del sistema con reglas estrictas de seguridad
    private const string SYSTEM_PROMPT = @"Eres el asistente virtual oficial de AdoPets, un refugio de animales y clínica veterinaria.

RESPONDE SIEMPRE EN ESPAÑOL.

Tu única función es ayudar a usuarios adoptantes con:
- Proceso de adopción y requisitos
- Información general sobre mascotas disponibles (especies, razas, características, cuidados básicos)
- Recomendaciones para elegir una mascota adecuada
- Dudas sobre cómo usar la plataforma
- Servicios veterinarios disponibles para mascotas adoptadas
- Donaciones y cómo apoyar al refugio

REGLAS ESTRICTAS DE SEGURIDAD:
- USA SOLO la información proporcionada en 'INFORMACIÓN RELEVANTE DEL SISTEMA' o el historial de esta conversación
- Si la pregunta NO está relacionada con los temas permitidos, responde: ""Solo puedo ayudarte con dudas sobre adopción de mascotas, cuidados básicos y servicios del refugio.""
- Si no encuentras la respuesta en la información disponible, responde: ""No tengo esa información en este momento. Te sugiero contactar directamente al refugio.""
- NUNCA inventes información que no esté en el contexto proporcionado
- Sé breve y directo (máximo 4-5 oraciones por respuesta)

INFORMACIÓN PROHIBIDA (NUNCA des información sobre):
- Datos personales de empleados, veterinarios o staff
- Información financiera interna del refugio
- Sistemas de inventario, proveedores o compras
- Credenciales de acceso o datos técnicos del sistema
- Historiales clínicos completos de mascotas
- Información confidencial de otros usuarios adoptantes
- Precios internos, costos operativos o presupuestos
- Horarios específicos de empleados
- Consejos médicos o diagnósticos veterinarios (solo información general)

Si te preguntan sobre temas prohibidos, responde amablemente: ""Por motivos de seguridad y privacidad, no puedo proporcionar esa información. Por favor, contacta directamente con el refugio.""

Mantén un tono amigable, profesional y útil.";

    public ChatController(
        AdoPetsDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<ChatController> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint principal del chatbot.
    /// POST: /api/v1/Chat/ask
    /// </summary>
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatAskRequestDto request)
    {
        // 1. Validar entrada
        if (request == null || string.IsNullOrWhiteSpace(request.Message) || string.IsNullOrWhiteSpace(request.UserId))
        {
            return BadRequest(new { error = "Los campos 'userId' y 'message' son obligatorios." });
        }

        // 2. Obtener o crear conversación
        ChatConversation conversation;
        
        if (request.ConversationId.HasValue)
        {
            // Buscar conversación existente con sus mensajes
            conversation = await _context.ChatConversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value);

            if (conversation == null)
            {
                // Conversación no encontrada, crear una nueva
                _logger.LogWarning("Conversación {ConversationId} no encontrada. Creando nueva conversación.", request.ConversationId.Value);
                conversation = new ChatConversation
                {
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ChatConversations.Add(conversation);
            }
            else if (conversation.UserId != request.UserId)
            {
                // Validar que la conversación pertenezca al usuario
                return Forbid("Esta conversación no te pertenece.");
            }
        }
        else
        {
            // Crear nueva conversación
            conversation = new ChatConversation
            {
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatConversations.Add(conversation);
        }

        // 3. Agregar mensaje del usuario
        var userMessage = new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(userMessage);

        // Guardar para obtener IDs
        await _context.SaveChangesAsync();

        // 4. Obtener contexto relevante de la base de conocimiento
        var contextText = await GetRelevantContextAsync(request.Message);

        // 5. Preparar historial de mensajes (últimos N mensajes)
        var recentMessages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderByDescending(m => m.CreatedAt)
            .Take(MAX_MESSAGES_IN_CONTEXT)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new { m.Role, m.Content })
            .ToListAsync();

        // 6. Construir el payload para Groq con el sistema de prompts
        var groqMessages = new List<object>();

        // Mensaje 1: Sistema principal con reglas
        groqMessages.Add(new
        {
            role = "system",
            content = SYSTEM_PROMPT
        });

        // Mensaje 2: Contexto relevante de la base de conocimiento
        if (!string.IsNullOrWhiteSpace(contextText))
        {
            groqMessages.Add(new
            {
                role = "system",
                content = $"INFORMACIÓN RELEVANTE DEL SISTEMA (solo puedes usar esto para responder):\n\n{contextText}"
            });
        }

        // Mensaje 3+: Historial de la conversación (excluyendo el mensaje actual que ya está en recentMessages)
        groqMessages.AddRange(recentMessages.Select(m => new { role = m.Role, content = m.Content }));

        var requestBody = new
        {
            model = GROQ_MODEL,
            messages = groqMessages,
            temperature = 0.7,
            max_tokens = 500
        };

        // 7. Llamar a Groq API
        var client = _httpClientFactory.CreateClient("GroqClient");
        HttpResponseMessage response;

        try
        {
            response = await client.PostAsJsonAsync("chat/completions", requestBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al llamar a Groq API.");
            return StatusCode(500, new { error = "Error al comunicarse con el servicio de IA." });
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            _logger.LogError("Groq API devolvió {StatusCode}: {Body}", response.StatusCode, errorText);

            return StatusCode((int)response.StatusCode, new
            {
                error = "Error en el servicio de IA",
                details = errorText
            });
        }

        // 8. Parsear respuesta de Groq
        GroqChatResponse? groqResponse;
        try
        {
            groqResponse = await response.Content.ReadFromJsonAsync<GroqChatResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al parsear respuesta de Groq.");
            return StatusCode(500, new { error = "Error al procesar respuesta del servicio de IA." });
        }

        var answer = groqResponse?
            .choices?
            .FirstOrDefault()?
            .message?
            .content ?? "Lo siento, no pude generar una respuesta en este momento.";

        // 9. Guardar respuesta del asistente
        var assistantMessage = new ChatMessage
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = answer,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChatMessages.Add(assistantMessage);

        await _context.SaveChangesAsync();

        // 10. Devolver respuesta
        return Ok(new ChatAskResponseDto
        {
            ConversationId = conversation.Id,
            Answer = answer
        });
    }

    /// <summary>
    /// Obtiene información relevante de la base de conocimiento basada en la pregunta del usuario.
    /// Esta información se usa como contexto para que el bot responda solo con datos verificados.
    /// </summary>
    private async Task<string> GetRelevantContextAsync(string userMessage)
    {
        var contextParts = new List<string>();

        // Convertir a minúsculas para búsqueda case-insensitive
        var messageLower = userMessage.ToLower();

        // CONTEXTO 1: Información sobre proceso de adopción
        if (messageLower.Contains("adopc") || messageLower.Contains("adoptar") || messageLower.Contains("requisito"))
        {
            contextParts.Add(@"PROCESO DE ADOPCIÓN:
- Requisitos: Ser mayor de edad, tener identificación oficial, comprobante de domicilio
- Pasos: 1) Llenar solicitud en línea, 2) Entrevista, 3) Visita domiciliaria (opcional), 4) Firma de contrato
- Tiempo aproximado: 5-7 días hábiles
- Costo: La adopción es gratuita, solo se paga una cuota de esterilización si la mascota no está esterilizada ($500-800 MXN)");
        }

        // CONTEXTO 2: Información sobre mascotas disponibles
        if (messageLower.Contains("mascota") || messageLower.Contains("perro") || messageLower.Contains("gato") || 
            messageLower.Contains("disponible") || messageLower.Contains("adoptar"))
        {
            // Obtener información general de mascotas disponibles (sin datos sensibles)
            var mascotasDisponibles = await _context.Mascotas
                .Where(m => m.Estatus == Domain.Entities.Mascotas.EstatusMascota.Disponible && m.DeletedAt == null)
                .Select(m => new { m.Nombre, m.Especie, m.Raza, m.Sexo, m.FechaNacimiento })
                .Take(10)
                .ToListAsync();

            if (mascotasDisponibles.Any())
            {
                var mascotasInfo = string.Join("\n", mascotasDisponibles.Select(m => 
                    $"- {m.Nombre}: {m.Especie}, {m.Raza ?? "Mestizo"}, {m.Sexo}, edad aproximada: {CalcularEdad(m.FechaNacimiento)}"));
                
                contextParts.Add($"MASCOTAS DISPONIBLES:\n{mascotasInfo}");
            }
        }

        // CONTEXTO 3: Cuidados básicos
        if (messageLower.Contains("cuidado") || messageLower.Contains("alimenta") || messageLower.Contains("vacuna") || 
            messageLower.Contains("salud") || messageLower.Contains("veterinari"))
        {
            contextParts.Add(@"CUIDADOS BÁSICOS:
- Alimentación: 2-3 veces al día con alimento de calidad según edad y tamaño
- Vacunación: Plan completo de vacunas (consultar con veterinario)
- Desparasitación: Cada 3-6 meses
- Consultas veterinarias: Al menos 1 vez al año
- Ejercicio: Mínimo 30 minutos diarios para perros
- Higiene: Baño mensual, cepillado regular");
        }

        // CONTEXTO 4: Servicios del refugio
        if (messageLower.Contains("servicio") || messageLower.Contains("clínica") || messageLower.Contains("consulta") ||
            messageLower.Contains("veterinari") || messageLower.Contains("cita"))
        {
            contextParts.Add(@"SERVICIOS DISPONIBLES:
- Consultas veterinarias generales
- Vacunación y desparasitación
- Esterilización y castración
- Cirugías básicas
- Valoraciones médicas
- Seguimiento post-adopción (primeros 30 días gratuito)
Nota: Para agendar citas, usa la sección de 'Citas' en la plataforma.");
        }

        // CONTEXTO 5: Donaciones
        if (messageLower.Contains("donar") || messageLower.Contains("donación") || messageLower.Contains("ayudar") ||
            messageLower.Contains("apoyar") || messageLower.Contains("contribuir"))
        {
            contextParts.Add(@"CÓMO APOYAR AL REFUGIO:
- Donaciones monetarias a través de la plataforma (PayPal)
- Donación de alimento para mascotas
- Donación de medicamentos y material veterinario
- Voluntariado (contactar al refugio)
- Difusión en redes sociales
Toda donación es deducible de impuestos.");
        }

        // CONTEXTO 6: Uso de la plataforma
        if (messageLower.Contains("plataforma") || messageLower.Contains("registro") || messageLower.Contains("cuenta") ||
            messageLower.Contains("cómo") || messageLower.Contains("usar"))
        {
            contextParts.Add(@"USO DE LA PLATAFORMA:
- Registro: Crea tu cuenta con email y contraseña
- Explorar mascotas: Navega por las mascotas disponibles en la sección principal
- Solicitar adopción: Haz clic en 'Adoptar' en la ficha de la mascota
- Seguimiento: Revisa el estado de tu solicitud en 'Mis Solicitudes'
- Agendar citas: Usa la sección 'Citas' para servicios veterinarios");
        }

        // Si no hay contexto relevante, dar información general
        if (!contextParts.Any())
        {
            contextParts.Add(@"INFORMACIÓN GENERAL:
AdoPets es un refugio de animales que busca dar hogar a mascotas rescatadas. 
Ofrecemos servicios de adopción, atención veterinaria y seguimiento post-adopción.
Pregúntame sobre: proceso de adopción, mascotas disponibles, cuidados básicos, servicios o cómo apoyar al refugio.");
        }

        return string.Join("\n\n", contextParts);
    }

    /// <summary>
    /// Calcula la edad aproximada de una mascota
    /// </summary>
    private string CalcularEdad(DateTime? fechaNacimiento)
    {
        if (!fechaNacimiento.HasValue)
            return "Edad desconocida";

        var edad = DateTime.UtcNow.Year - fechaNacimiento.Value.Year;
        if (DateTime.UtcNow < fechaNacimiento.Value.AddYears(edad))
            edad--;

        if (edad == 0)
            return "Menos de 1 año";
        else if (edad == 1)
            return "1 año";
        else
            return $"{edad} años";
    }

    /// <summary>
    /// Obtiene el historial de una conversación
    /// GET: /api/v1/Chat/conversation/{conversationId}
    /// </summary>
    [HttpGet("conversation/{conversationId}")]
    public async Task<IActionResult> GetConversation(Guid conversationId, [FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "El parámetro 'userId' es obligatorio." });
        }

        var conversation = await _context.ChatConversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation == null)
        {
            return NotFound(new { error = "Conversación no encontrada." });
        }

        if (conversation.UserId != userId)
        {
            return Forbid("Esta conversación no te pertenece.");
        }

        return Ok(new
        {
            conversationId = conversation.Id,
            userId = conversation.UserId,
            createdAt = conversation.CreatedAt,
            messages = conversation.Messages.Select(m => new
            {
                id = m.Id,
                role = m.Role,
                content = m.Content,
                createdAt = m.CreatedAt
            })
        });
    }

    /// <summary>
    /// Obtiene todas las conversaciones de un usuario
    /// GET: /api/v1/Chat/conversations
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetUserConversations([FromQuery] string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new { error = "El parámetro 'userId' es obligatorio." });
        }

        var conversations = await _context.ChatConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                conversationId = c.Id,
                createdAt = c.CreatedAt,
                messageCount = c.Messages.Count,
                lastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => new { m.Content, m.CreatedAt })
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(conversations);
    }
}
