using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AdoPetsBKD.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GroqChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GroqChatController> _logger;

        public GroqChatController(
            IHttpClientFactory httpClientFactory,
            ILogger<GroqChatController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Endpoint de prueba para hablar con Groq (LLaMA 3.3).
        /// POST: /api/v1/GroqChat/ask
        /// </summary>
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new { error = "El campo 'message' es obligatorio." });
            }

            // Cliente configurado en Program.cs con nombre "GroqClient"
            var client = _httpClientFactory.CreateClient("GroqClient");

            var body = new
            {
                model = "llama-3.3-70b-versatile",   // puedes cambiar de modelo si quieres
                messages = new[]
                {
                    new { role = "user", content = request.Message }
                }
            };

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync("chat/completions", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a Groq.");
                return StatusCode(500, new { error = "Error al llamar a Groq." });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Groq devolvió {StatusCode}: {Body}", response.StatusCode, errorText);

                return StatusCode((int)response.StatusCode, new
                {
                    error = "Groq API error",
                    details = errorText
                });
            }

            // Mapear JSON -> clases tipadas
            var groqResponse = await response.Content.ReadFromJsonAsync<GroqChatResponse>();

            var answer = groqResponse?
                .choices?
                .FirstOrDefault()?
                .message?
                .content ?? "Sin respuesta";

            return Ok(new { answer });
        }
    }

    // ====== MODELOS ======

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class GroqChatResponse
    {
        public List<GroqChoice> choices { get; set; } = new();
    }

    public class GroqChoice
    {
        public GroqMessage message { get; set; } = new();
    }

    public class GroqMessage
    {
        public string role { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
    }
}
