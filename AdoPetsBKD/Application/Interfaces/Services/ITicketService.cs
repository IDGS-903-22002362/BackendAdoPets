using AdoPetsBKD.Application.DTOs.Clinica;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface ITicketService
{
    Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, Guid createdBy);
    Task<TicketDto?> GetTicketByIdAsync(Guid id);
    Task<TicketDto?> GetTicketByNumeroAsync(string numeroTicket);
    Task<List<TicketDto>> GetTicketsByClienteAsync(Guid clienteId);
    Task<List<TicketDto>> GetTicketsByCitaAsync(Guid citaId);
    Task<TicketDto> MarcarComoEntregadoAsync(Guid ticketId, Guid entregadoPorId);
    Task<byte[]> GenerarPdfTicketAsync(Guid ticketId);
}
