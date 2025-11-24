using AdoPetsBKD.Application.DTOs.Inventario;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Services;

public class ItemInventarioService : IItemInventarioService
{
    private readonly IItemInventarioRepository _repo;
    private readonly ILoteInventarioRepository _loteRepo;

    public ItemInventarioService(IItemInventarioRepository repo, ILoteInventarioRepository loteRepo)
    {
        _repo = repo;
        _loteRepo = loteRepo;
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
        var lotes = await _loteRepo.GetAllAsync(); // 🔥 YA CORREGIDO

        var resultado = new List<InventarioItemDTO>();

        foreach (var item in items)
        {
            var lotesItem = lotes
                .Where(l => l.ItemId == item.Id)
                .OrderBy(l => l.ExpDate)
                .ToList();

            decimal totalDisponible = lotesItem.Sum(l => l.QtyDisponible);

            var loteMasProximo = lotesItem.FirstOrDefault();

            resultado.Add(new InventarioItemDTO
            {
                ItemId = item.Id,
                Nombre = item.Nombre,
                MinQty = item.MinQty,
                Unidad = item.Unidad,   // 🔥 ESTO TAMBIÉN YA CORREGIDO

                TotalDisponible = totalDisponible,

                LoteMasProximo = loteMasProximo == null ? null : new LoteInventarioDTO
                {
                    LoteId = loteMasProximo.Id,
                    Lote = loteMasProximo.Lote,
                    ExpDate = loteMasProximo.ExpDate,
                    QtyDisponible = loteMasProximo.QtyDisponible
                }
            });
        }

        return resultado;
    }

}
