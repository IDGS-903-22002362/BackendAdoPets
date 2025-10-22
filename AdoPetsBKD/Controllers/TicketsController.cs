using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Common;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TicketDto>>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var ticket = await _ticketService.CreateTicketAsync(dto, userId);
            return Ok(ApiResponse<TicketDto>.SuccessResponse(ticket, "Ticket creado exitosamente"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TicketDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetTicketById(Guid id)
    {
        try
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound(ApiResponse<TicketDto>.ErrorResponse("Ticket no encontrado"));

            return Ok(ApiResponse<TicketDto>.SuccessResponse(ticket));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TicketDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("numero/{numeroTicket}")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetTicketByNumero(string numeroTicket)
    {
        try
        {
            var ticket = await _ticketService.GetTicketByNumeroAsync(numeroTicket);
            if (ticket == null)
                return NotFound(ApiResponse<TicketDto>.ErrorResponse("Ticket no encontrado"));

            return Ok(ApiResponse<TicketDto>.SuccessResponse(ticket));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TicketDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetTicketsByCliente(Guid clienteId)
    {
        try
        {
            var tickets = await _ticketService.GetTicketsByClienteAsync(clienteId);
            return Ok(ApiResponse<List<TicketDto>>.SuccessResponse(tickets));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TicketDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("cita/{citaId}")]
    public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetTicketsByCita(Guid citaId)
    {
        try
        {
            var tickets = await _ticketService.GetTicketsByCitaAsync(citaId);
            return Ok(ApiResponse<List<TicketDto>>.SuccessResponse(tickets));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<TicketDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("{id}/entregar")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> MarcarComoEntregado(Guid id)
    {
        try
        {
            // TODO: Obtener ID del usuario autenticado del token JWT
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Temporal
            
            var ticket = await _ticketService.MarcarComoEntregadoAsync(id, userId);
            return Ok(ApiResponse<TicketDto>.SuccessResponse(ticket, "Ticket marcado como entregado"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TicketDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GenerarPdf(Guid id)
    {
        try
        {
            var pdfBytes = await _ticketService.GenerarPdfTicketAsync(id);
            return File(pdfBytes, "application/pdf", $"ticket-{id}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
