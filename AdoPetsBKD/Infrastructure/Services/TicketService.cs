using Microsoft.EntityFrameworkCore;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Infrastructure.Data;

namespace AdoPetsBKD.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly AdoPetsDbContext _context;

    public TicketService(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, Guid createdBy)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            CitaId = dto.CitaId,
            MascotaId = dto.MascotaId,
            ClienteId = dto.ClienteId,
            VeterinarioId = dto.VeterinarioId,
            FechaProcedimiento = dto.FechaProcedimiento,
            NombreProcedimiento = dto.NombreProcedimiento,
            DescripcionProcedimiento = dto.DescripcionProcedimiento,
            CostoProcedimiento = dto.CostoProcedimiento,
            CostoInsumos = dto.CostoInsumos,
            CostoAdicional = dto.CostoAdicional,
            Descuento = dto.Descuento,
            Observaciones = dto.Observaciones,
            Diagnostico = dto.Diagnostico,
            Tratamiento = dto.Tratamiento,
            MedicacionPrescrita = dto.MedicacionPrescrita,
            CreatedBy = createdBy
        };

        ticket.NumeroTicket = ticket.GenerarNumeroTicket();
        ticket.CalcularTotal();

        // Agregar detalles
        foreach (var detalleDto in dto.Detalles)
        {
            var detalle = new TicketDetalle
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                Descripcion = detalleDto.Descripcion,
                Cantidad = detalleDto.Cantidad,
                Unidad = detalleDto.Unidad,
                PrecioUnitario = detalleDto.PrecioUnitario,
                ItemInventarioId = detalleDto.ItemInventarioId,
                Tipo = (TipoDetalleTicket)detalleDto.Tipo
            };
            detalle.CalcularSubtotal();
            ticket.Detalles.Add(detalle);
        }

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticket.Id) ?? throw new Exception("Error al crear ticket");
    }

    public async Task<TicketDto?> GetTicketByIdAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.Mascota)
            .Include(t => t.Cliente)
            .Include(t => t.Veterinario)
            .Include(t => t.Detalles)
            .Where(t => t.Id == id)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                NumeroTicket = t.NumeroTicket,
                CitaId = t.CitaId,
                MascotaId = t.MascotaId,
                NombreMascota = t.Mascota != null ? t.Mascota.Nombre : null,
                ClienteId = t.ClienteId,
                NombreCliente = t.Cliente.NombreCompleto,
                VeterinarioId = t.VeterinarioId,
                NombreVeterinario = t.Veterinario.NombreCompleto,
                FechaProcedimiento = t.FechaProcedimiento,
                NombreProcedimiento = t.NombreProcedimiento,
                DescripcionProcedimiento = t.DescripcionProcedimiento,
                CostoProcedimiento = t.CostoProcedimiento,
                CostoInsumos = t.CostoInsumos,
                CostoAdicional = t.CostoAdicional,
                Subtotal = t.Subtotal,
                Descuento = t.Descuento,
                IVA = t.IVA,
                Total = t.Total,
                Observaciones = t.Observaciones,
                Diagnostico = t.Diagnostico,
                Tratamiento = t.Tratamiento,
                MedicacionPrescrita = t.MedicacionPrescrita,
                Estado = (int)t.Estado,
                EstadoNombre = t.Estado.ToString(),
                FechaEntrega = t.FechaEntrega,
                PagoId = t.PagoId,
                CreatedAt = t.CreatedAt,
                Detalles = t.Detalles.Select(d => new TicketDetalleDto
                {
                    Id = d.Id,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    Unidad = d.Unidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Tipo = (int)d.Tipo,
                    TipoNombre = d.Tipo.ToString()
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TicketDto?> GetTicketByNumeroAsync(string numeroTicket)
    {
        return await _context.Tickets
            .Include(t => t.Mascota)
            .Include(t => t.Cliente)
            .Include(t => t.Veterinario)
            .Include(t => t.Detalles)
            .Where(t => t.NumeroTicket == numeroTicket)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                NumeroTicket = t.NumeroTicket,
                CitaId = t.CitaId,
                MascotaId = t.MascotaId,
                NombreMascota = t.Mascota != null ? t.Mascota.Nombre : null,
                ClienteId = t.ClienteId,
                NombreCliente = t.Cliente.NombreCompleto,
                VeterinarioId = t.VeterinarioId,
                NombreVeterinario = t.Veterinario.NombreCompleto,
                FechaProcedimiento = t.FechaProcedimiento,
                NombreProcedimiento = t.NombreProcedimiento,
                DescripcionProcedimiento = t.DescripcionProcedimiento,
                CostoProcedimiento = t.CostoProcedimiento,
                CostoInsumos = t.CostoInsumos,
                CostoAdicional = t.CostoAdicional,
                Subtotal = t.Subtotal,
                Descuento = t.Descuento,
                IVA = t.IVA,
                Total = t.Total,
                Observaciones = t.Observaciones,
                Diagnostico = t.Diagnostico,
                Tratamiento = t.Tratamiento,
                MedicacionPrescrita = t.MedicacionPrescrita,
                Estado = (int)t.Estado,
                EstadoNombre = t.Estado.ToString(),
                FechaEntrega = t.FechaEntrega,
                PagoId = t.PagoId,
                CreatedAt = t.CreatedAt,
                Detalles = t.Detalles.Select(d => new TicketDetalleDto
                {
                    Id = d.Id,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    Unidad = d.Unidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Tipo = (int)d.Tipo,
                    TipoNombre = d.Tipo.ToString()
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<TicketDto>> GetTicketsByClienteAsync(Guid clienteId)
    {
        return await _context.Tickets
            .Include(t => t.Mascota)
            .Include(t => t.Cliente)
            .Include(t => t.Veterinario)
            .Include(t => t.Detalles)
            .Where(t => t.ClienteId == clienteId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                NumeroTicket = t.NumeroTicket,
                CitaId = t.CitaId,
                MascotaId = t.MascotaId,
                NombreMascota = t.Mascota != null ? t.Mascota.Nombre : null,
                ClienteId = t.ClienteId,
                NombreCliente = t.Cliente.NombreCompleto,
                VeterinarioId = t.VeterinarioId,
                NombreVeterinario = t.Veterinario.NombreCompleto,
                FechaProcedimiento = t.FechaProcedimiento,
                NombreProcedimiento = t.NombreProcedimiento,
                Total = t.Total,
                Estado = (int)t.Estado,
                EstadoNombre = t.Estado.ToString(),
                CreatedAt = t.CreatedAt,
                Detalles = t.Detalles.Select(d => new TicketDetalleDto
                {
                    Id = d.Id,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    Unidad = d.Unidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Tipo = (int)d.Tipo,
                    TipoNombre = d.Tipo.ToString()
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<TicketDto>> GetTicketsByCitaAsync(Guid citaId)
    {
        return await _context.Tickets
            .Include(t => t.Mascota)
            .Include(t => t.Cliente)
            .Include(t => t.Veterinario)
            .Include(t => t.Detalles)
            .Where(t => t.CitaId == citaId)
            .Select(t => new TicketDto
            {
                Id = t.Id,
                NumeroTicket = t.NumeroTicket,
                CitaId = t.CitaId,
                MascotaId = t.MascotaId,
                NombreMascota = t.Mascota != null ? t.Mascota.Nombre : null,
                ClienteId = t.ClienteId,
                NombreCliente = t.Cliente.NombreCompleto,
                VeterinarioId = t.VeterinarioId,
                NombreVeterinario = t.Veterinario.NombreCompleto,
                FechaProcedimiento = t.FechaProcedimiento,
                NombreProcedimiento = t.NombreProcedimiento,
                Total = t.Total,
                Estado = (int)t.Estado,
                EstadoNombre = t.Estado.ToString(),
                CreatedAt = t.CreatedAt,
                Detalles = t.Detalles.Select(d => new TicketDetalleDto
                {
                    Id = d.Id,
                    Descripcion = d.Descripcion,
                    Cantidad = d.Cantidad,
                    Unidad = d.Unidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal,
                    Tipo = (int)d.Tipo,
                    TipoNombre = d.Tipo.ToString()
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<TicketDto> MarcarComoEntregadoAsync(Guid ticketId, Guid entregadoPorId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId)
            ?? throw new Exception("Ticket no encontrado");

        ticket.MarcarComoEntregado(entregadoPorId);
        await _context.SaveChangesAsync();

        return await GetTicketByIdAsync(ticketId) ?? throw new Exception("Error al actualizar ticket");
    }

    public async Task<byte[]> GenerarPdfTicketAsync(Guid ticketId)
    {
        // TODO: Implementar generación de PDF usando una librería como QuestPDF o iTextSharp
        // Por ahora retornamos un array vacío
        await Task.CompletedTask;
        return Array.Empty<byte>();
    }
}
