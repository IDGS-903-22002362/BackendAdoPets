namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IRecordatorioService
{
    Task EnviarRecordatoriosPendientesAsync();
    Task ProgramarRecordatoriosParaCitaAsync(Guid citaId);
}
