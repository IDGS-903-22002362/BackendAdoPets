using Microsoft.EntityFrameworkCore;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Clinica;
using AdoPetsBKD.Domain.Entities.Inventario;
using AdoPetsBKD.Infrastructure.Data;

namespace AdoPetsBKD.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly AdoPetsDbContext _context;
    private readonly IItemInventarioRepository _itemRepo;
    private readonly ILoteInventarioRepository _loteRepo;
    private readonly IMovimientoInventarioRepository _movimientoRepo;

    public TicketService(
        AdoPetsDbContext context,
        IItemInventarioRepository itemRepo,
        ILoteInventarioRepository loteRepo,
        IMovimientoInventarioRepository movimientoRepo)
    {
        _context = context;
        _itemRepo = itemRepo;
        _loteRepo = loteRepo;
        _movimientoRepo = movimientoRepo;
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, Guid createdBy)
    {
        // Usar la estrategia de ejecución configurada para manejar transacciones con reintentos
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Iniciar transacción dentro de la estrategia de ejecución
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                // Agregar detalles y procesar descuento de inventario
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

                    // Si el detalle tiene asociado un item de inventario, descontar del stock
                    if (detalleDto.ItemInventarioId.HasValue && detalleDto.Cantidad > 0)
                    {
                        await DescontarInsumoDelInventarioAsync(
                            detalleDto.ItemInventarioId.Value,
                            detalleDto.Cantidad,
                            ticket.Id,
                            ticket.CitaId,
                            createdBy
                        );
                    }
                }

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                // Commit de la transacción
                await transaction.CommitAsync();

                return await GetTicketByIdAsync(ticket.Id) ?? throw new Exception("Error al crear ticket");
            }
            catch (Exception)
            {
                // Rollback en caso de error
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Descuenta insumos del inventario aplicando lógica FIFO
    /// </summary>
    private async Task DescontarInsumoDelInventarioAsync(
        Guid itemId,
        decimal cantidadRequerida,
        Guid ticketId,
        Guid citaId,
        Guid performedBy)
    {
        // Validar que el item existe
        var item = await _itemRepo.GetByIdAsync(itemId);
        if (item == null)
            throw new Exception($"El item de inventario {itemId} no existe");

        // Obtener lotes disponibles con FIFO (los que vencen primero)
        var lotesDisponibles = await _loteRepo.GetLotesDisponiblesByItemIdAsync(itemId);

        if (!lotesDisponibles.Any())
            throw new Exception($"No hay stock disponible del item '{item.Nombre}'");

        // Verificar stock total disponible
        var stockTotalDisponible = lotesDisponibles.Sum(l => l.QtyDisponible);
        if (stockTotalDisponible < cantidadRequerida)
        {
            throw new Exception(
                $"Stock insuficiente para '{item.Nombre}'. " +
                $"Requerido: {cantidadRequerida} {item.Unidad}, " +
                $"Disponible: {stockTotalDisponible} {item.Unidad}"
            );
        }

        // Descontar de lotes aplicando FIFO
        decimal cantidadRestante = cantidadRequerida;

        foreach (var lote in lotesDisponibles)
        {
            if (cantidadRestante <= 0)
                break;

            // Verificar si el lote está vencido
            if (lote.EstaVencido())
            {
                throw new Exception(
                    $"El lote '{lote.Lote}' del item '{item.Nombre}' está vencido. " +
                    $"Fecha de vencimiento: {lote.ExpDate:dd/MM/yyyy}"
                );
            }

            decimal cantidadADescontar = Math.Min(cantidadRestante, lote.QtyDisponible);

            // Descontar del lote
            lote.DescontarStock(cantidadADescontar);
            await _loteRepo.UpdateAsync(lote);

            // Registrar movimiento de inventario
            var movimiento = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                BatchId = lote.Id,
                Tipo = TipoMovimiento.Salida,
                Qty = cantidadADescontar,
                Reason = $"Consumo en ticket - Cita {citaId}",
                PerformedBy = performedBy,
                RelatedAppointmentId = citaId,
                Observaciones = $"Ticket: {ticketId}, Lote: {lote.Lote}"
            };

            await _movimientoRepo.AddAsync(movimiento);

            cantidadRestante -= cantidadADescontar;
        }

        await _loteRepo.SaveChangesAsync();
        await _movimientoRepo.SaveChangesAsync();
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

    public async Task<List<TicketDto>> GetAllTicketsAsync()
    {
        return await _context.Tickets
            .Include(t => t.Mascota)
            .Include(t => t.Cliente)
            .Include(t => t.Veterinario)
            .Include(t => t.Detalles)
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
