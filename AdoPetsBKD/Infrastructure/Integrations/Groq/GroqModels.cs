namespace AdoPetsBKD.Infrastructure.Integrations.Groq;

/// <summary>
/// Respuesta de la API de Groq para chat completions
/// </summary>
public class GroqChatResponse
{
    public List<GroqChoice> choices { get; set; } = new();
}

/// <summary>
/// Opción de respuesta de Groq
/// </summary>
public class GroqChoice
{
    public GroqMessage message { get; set; } = new();
}

/// <summary>
/// Mensaje en el formato de Groq
/// </summary>
public class GroqMessage
{
    public string role { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
}
