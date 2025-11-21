using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IEmailService
{
    Task EnviarRecordatorioCitaAsync(string destinatario, string nombreDestinatario, Cita cita, TimeSpan tiempoRestante);
    Task EnviarEmailAsync(string destinatario, string asunto, string cuerpoHtml);
}
