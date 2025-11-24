using AdoPetsBKD.Application.DTOs.Inventario;

namespace AdoPetsBKD.Application.Interfaces.Services;

public interface IItemInventarioService
{
    Task<ItemDTO> CrearItemAsync(CrearItemDTO dto);
    Task<ItemDTO?> ObtenerPorIdAsync(Guid id);
    Task<IEnumerable<ItemDTO>> ObtenerTodosAsync();
    Task<List<InventarioItemDTO>> GetInventarioAsync();


}
