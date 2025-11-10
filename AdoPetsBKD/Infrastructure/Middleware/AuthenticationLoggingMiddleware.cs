using Microsoft.Extensions.Primitives;

namespace AdoPetsBKD.Infrastructure.Middleware;

/// <summary>
/// Middleware para loggear detalles de autenticación y debugging de JWT
/// </summary>
public class AuthenticationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationLoggingMiddleware> _logger;

    public AuthenticationLoggingMiddleware(RequestDelegate next, ILogger<AuthenticationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var path = context.Request.Path.Value ?? string.Empty;
        
        // Solo loggear para endpoints que requieren autenticación
        if (endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null)
        {
            _logger.LogWarning("?? === INICIO AUTH DEBUG ===");
            _logger.LogWarning($"?? Endpoint: {context.Request.Method} {path}");
            _logger.LogWarning($"?? Host: {context.Request.Host}");
            _logger.LogWarning($"?? User-Agent: {context.Request.Headers["User-Agent"]}");
            
            // Verificar header Authorization
            if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
            {
                var headerValue = authHeader.ToString();
                _logger.LogWarning($"? Authorization Header Presente");
                _logger.LogWarning($"   Longitud: {headerValue.Length} caracteres");
                _logger.LogWarning($"   Comienza con 'Bearer': {headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)}");
                
                if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = headerValue.Substring(7);
                    _logger.LogWarning($"   Token extraído, longitud: {token.Length} caracteres");
                    _logger.LogWarning($"   Primeros 20 chars: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else
                {
                    _logger.LogWarning($"   ?? Header NO comienza con 'Bearer '");
                    _logger.LogWarning($"   Valor: {headerValue.Substring(0, Math.Min(50, headerValue.Length))}");
                }
            }
            else
            {
                _logger.LogWarning("? Authorization Header NO presente");
                _logger.LogWarning("?? Headers disponibles:");
                foreach (var header in context.Request.Headers)
                {
                    _logger.LogWarning($"   - {header.Key}: {(header.Key.Contains("Auth", StringComparison.OrdinalIgnoreCase) ? header.Value.ToString() : "[valor oculto]")}");
                }
            }
            
            _logger.LogWarning("?? === FIN AUTH DEBUG ===");
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method para agregar el middleware
/// </summary>
public static class AuthenticationLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationLoggingMiddleware>();
    }
}
