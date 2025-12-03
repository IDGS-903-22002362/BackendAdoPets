using AdoPetsBKD.Application.DTOs.Inventario;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Inventario;
using Microsoft.EntityFrameworkCore;
using AdoPetsBKD.Infrastructure.Data;

namespace AdoPetsBKD.Infrastructure.Services;

public class ItemInventarioService : IItemInventarioService
{
    private readonly IItemInventarioRepository _repo;
    private readonly ILoteInventarioRepository _loteRepo;
    private readonly AdoPetsDbContext _context;

    public ItemInventarioService(
        IItemInventarioRepository repo, 
        ILoteInventarioRepository loteRepo,
        AdoPetsDbContext context)
    {
        _repo = repo;
        _loteRepo = loteRepo;
        _context = context;
    }

    public async Task<ItemDTO> CrearItemAsync(CrearItemDTO dto)
    {
        var nuevo = new ItemInventario
        {
            Nombre = dto.Nombre,
            Unidad = dto.Unidad,
            Categoria = dto.Categoria,
            MinQty = dto.MinQty,
            Notas = dto.Notas,
            Descripcion = dto.Descripcion,
            Activo = true
        };

        await _repo.AddAsync(nuevo);
        await _repo.SaveChangesAsync();

        return new ItemDTO
        {
            Id = nuevo.Id,
            Nombre = nuevo.Nombre,
            Unidad = nuevo.Unidad,
            Categoria = nuevo.Categoria,
            MinQty = nuevo.MinQty,
            Activo = nuevo.Activo,
            StockTotal = 0
        };
    }

    public async Task<ItemDTO?> ObtenerPorIdAsync(Guid id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return null;

        return new ItemDTO
        {
            Id = item.Id,
            Nombre = item.Nombre,
            Unidad = item.Unidad,
            Categoria = item.Categoria,
            MinQty = item.MinQty,
            Activo = item.Activo,
            StockTotal = item.StockTotal
        };
    }

    public async Task<IEnumerable<ItemDTO>> ObtenerTodosAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(i => new ItemDTO
        {
            Id = i.Id,
            Nombre = i.Nombre,
            Unidad = i.Unidad,
            Categoria = i.Categoria,
            MinQty = i.MinQty,
            Activo = i.Activo,
            StockTotal = i.StockTotal
        });
    }

    public async Task<List<InventarioItemDTO>> GetInventarioAsync()
    {
        var items = await _repo.GetAllAsync();
        var lotes = await _loteRepo.GetAllAsync();

        var resultado = new List<InventarioItemDTO>();

        foreach (var item in items)
        {
            var lotesItem = lotes
                .Where(l => l.ItemId == item.Id)
                .OrderBy(l => l.ExpDate ?? DateTime.MaxValue) // FIFO: primero los que vencen antes
                .ThenBy(l => l.CreatedAt)
                .ToList();

            decimal totalDisponible = lotesItem.Sum(l => l.QtyDisponible);

            var loteMasProximo = lotesItem.FirstOrDefault();

            // Obtener precio unitario del lote desde DetalleCompra
            decimal? precioUnitario = null;
            decimal? precioUnitarioPorUnidad = null;
            LoteInventarioDTO? loteDto = null;

            if (loteMasProximo != null)
            {
                // Buscar el precio del lote en DetalleCompra usando el número de lote
                var detalleCompra = await _context.DetallesCompras
                    .Where(dc => dc.ItemId == item.Id && dc.Lote == loteMasProximo.Lote)
                    .OrderByDescending(dc => dc.Compra.FechaCompra)
                    .FirstOrDefaultAsync();

                if (detalleCompra != null)
                {
                    // Precio del lote completo (presentación)
                    precioUnitario = detalleCompra.PrecioUnitario;
                    
                    // Calcular precio por unidad individual
                    // PrecioUnitario del lote / MinQty (unidades por presentación)
                    if (item.MinQty > 0)
                    {
                        precioUnitarioPorUnidad = precioUnitario / item.MinQty;
                    }
                    else
                    {
                        // Si MinQty es 0, usar el precio del lote directamente
                        precioUnitarioPorUnidad = precioUnitario;
                    }
                }

                loteDto = new LoteInventarioDTO
                {
                    LoteId = loteMasProximo.Id,
                    Lote = loteMasProximo.Lote,
                    ExpDate = loteMasProximo.ExpDate,
                    QtyDisponible = loteMasProximo.QtyDisponible,
                    PrecioUnitario = precioUnitarioPorUnidad ?? 0m
                };
            }

            resultado.Add(new InventarioItemDTO
            {
                ItemId = item.Id,
                Nombre = item.Nombre,
                MinQty = item.MinQty,
                Unidad = item.Unidad,
                TotalDisponible = totalDisponible,
                PrecioUnitario = precioUnitarioPorUnidad, // Precio por unidad individual
                LoteMasProximo = loteDto
            });
        }

        return resultado;
    }
}
