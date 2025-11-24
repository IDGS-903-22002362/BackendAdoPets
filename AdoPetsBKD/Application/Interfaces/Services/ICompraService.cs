using AdoPetsBKD.Application.DTOs.Inventario;


namespace AdoPetsBKD.Application.Interfaces.Services;


public interface ICompraService
{
    Task<Guid> SurtirAsync(CrearCompraDTO dto, Guid userId);
}