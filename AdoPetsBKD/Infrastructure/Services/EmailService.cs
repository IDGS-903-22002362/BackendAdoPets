using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Net.Mail;

namespace AdoPetsBKD.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarRecordatorioCitaAsync(
        string destinatario,
        string nombreDestinatario,
        Cita cita,
        TimeSpan tiempoRestante)
    {
        try
        {
            var asunto = "?? Recordatorio de Cita - AdoPets Veterinaria";
            var cuerpoHtml = GenerarCuerpoEmailRecordatorio(nombreDestinatario, cita, tiempoRestante);

            await EnviarEmailAsync(destinatario, asunto, cuerpoHtml);

            _logger.LogInformation(
                "Email de recordatorio enviado exitosamente a {Email} para cita {CitaId}",
                destinatario,
                cita.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error al enviar email de recordatorio a {Email} para cita {CitaId}",
                destinatario,
                cita.Id
            );
            throw;
        }
    }

    public async Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "AdoPets Veterinaria";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || 
                string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("Configuración de email incompleta. Email no enviado.");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email enviado exitosamente a {Email}", destinatario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {Email}", destinatario);
            throw;
        }
    }

    private static string GenerarCuerpoEmailRecordatorio(string nombreDestinatario, Cita cita, TimeSpan tiempoRestante)
    {
        var veterinario = $"{cita.Veterinario.Nombre} {cita.Veterinario.ApellidoPaterno}";
        var cultura = new CultureInfo("es-MX");
        var fecha = cita.StartAt.ToLocalTime().ToString("dddd, dd 'de' MMMM 'de' yyyy", cultura);
        var hora = cita.StartAt.ToLocalTime().ToString("hh:mm tt", cultura);
        var tiempo = FormatearTiempo(tiempoRestante);

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border: 1px solid #ddd; }}
        .cita-info {{ background-color: white; padding: 20px; margin: 20px 0; border-left: 4px solid #4CAF50; }}
        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
        .highlight {{ color: #4CAF50; font-weight: bold; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>?? Recordatorio de Cita</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{nombreDestinatario}</strong>,</p>
            
            <p>Este es un recordatorio de tu cita veterinaria <span class='highlight'>{tiempo}</span>.</p>
            
            <div class='cita-info'>
                <h3>?? Detalles de la Cita</h3>
                <p><strong>?? Fecha:</strong> {fecha}</p>
                <p><strong>?? Hora:</strong> {hora}</p>
                <p><strong>????? Veterinario:</strong> {veterinario}</p>
                {(cita.Sala != null ? $"<p><strong>?? Sala:</strong> {cita.Sala.Nombre}</p>" : "")}
                {(cita.Mascota != null ? $"<p><strong>?? Mascota:</strong> {cita.Mascota.Nombre}</p>" : "")}
                <p><strong>?? Duración:</strong> {cita.DuracionMin} minutos</p>
            </div>
            
            <p><strong>?? Importante:</strong></p>
            <ul>
                <li>Por favor llega 10 minutos antes de tu cita</li>
                <li>Trae el carnet de vacunación de tu mascota</li>
                <li>Si necesitas cancelar, comunícate con nosotros con anticipación</li>
            </ul>
            
            <p>¡Te esperamos!</p>
        </div>
        <div class='footer'>
            <p>Este es un mensaje automático, por favor no respondas a este correo.</p>
            <p>AdoPets Veterinaria - Cuidando a tus mejores amigos ??</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string FormatearTiempo(TimeSpan tiempo)
    {
        if (tiempo.TotalHours >= 23)
            return "en 24 horas";
        if (tiempo.TotalHours >= 1.5)
            return "en 2 horas";
        if (tiempo.TotalHours >= 0.5)
            return "en 1 hora";
        
        return $"en {(int)tiempo.TotalMinutes} minutos";
    }
}
