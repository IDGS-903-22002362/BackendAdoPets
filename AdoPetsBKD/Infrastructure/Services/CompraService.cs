using AdoPetsBKD.Application.DTOs.Inventario;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Services;

public class CompraService : ICompraService
{
    private readonly ICompraRepository _compraRepo;
    private readonly IDetalleCompraRepository _detalleRepo;
    private readonly ILoteInventarioRepository _loteRepo;
    private readonly IMovimientoInventarioRepository _movRepo;
    private readonly IItemInventarioRepository _itemRepo;

    public CompraService(
        ICompraRepository compraRepo,
        IDetalleCompraRepository detalleRepo,
        ILoteInventarioRepository loteRepo,
        IMovimientoInventarioRepository movRepo,
        IItemInventarioRepository itemRepo)
    {
        _compraRepo = compraRepo;
        _detalleRepo = detalleRepo;
        _loteRepo = loteRepo;
        _movRepo = movRepo;
        _itemRepo = itemRepo;
    }

    public async Task<Guid> SurtirAsync(CrearCompraDTO dto, Guid userId)
    {
        var now = DateTime.Now;
        var random = new Random();
        string numeroFactura = $"FAC-{random.Next(1000, 9999)}";

        // Crear compra inicialmente sin total
        var compra = new Compra
        {
            ProveedorId = dto.ProveedorId,
            NumeroFactura = numeroFactura,
            FechaCompra = now,
            Notas = dto.Notas,
            Total = 0
        };

        await _compraRepo.AddAsync(compra);

        // Crear detalles
        int contadorLote = 1;
        var detalles = dto.Detalles.Select(d => new DetalleCompra
        {
            CompraId = compra.Id,
            ItemId = d.ItemId,
            Lote = $"LOTE-{contadorLote++}",
            ExpDate = d.ExpDate,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Notas = d.Notas
        }).ToList();

        await _detalleRepo.AddRangeAsync(detalles);

        // Calcular total
        decimal total = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
        compra.Total = total;

        await _compraRepo.UpdateAsync(compra);

        // Crear lotes multiplicando MinQty * unidades
        var lotes = new List<LoteInventario>();

        foreach (var d in detalles)
        {
            var item = await _itemRepo.GetByIdAsync(d.ItemId)
                       ?? throw new Exception($"El Item {d.ItemId} no existe.");

            decimal cantidadReal = item.MinQty * d.Cantidad;

            var lote = new LoteInventario
            {
                ItemId = d.ItemId,
                Lote = d.Lote,
                ExpDate = d.ExpDate,
                QtyInicial = cantidadReal,
                QtyDisponible = cantidadReal,
                Notas = d.Notas
            };

            lotes.Add(lote);
        }

        await _loteRepo.AddRangeAsync(lotes);

        // Registrar movimientos
        var movimientos = lotes.Select(l => new MovimientoInventario
        {
            ItemId = l.ItemId,
            BatchId = l.Id,
            Tipo = TipoMovimiento.Entrada,
            Qty = l.QtyInicial,
            Reason = "SURTIDO DE COMPRA",
            PerformedBy = userId
        }).ToList();

        await _movRepo.AddRangeAsync(movimientos);

        return compra.Id;
    }
}
