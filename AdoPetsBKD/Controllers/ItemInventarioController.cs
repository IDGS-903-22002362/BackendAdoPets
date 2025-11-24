using AdoPetsBKD.Application.DTOs.Inventario;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/items")]
public class ItemInventarioController : ControllerBase
{
    private readonly IItemInventarioService _service;

    public ItemInventarioController(IItemInventarioService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CrearItem([FromBody] CrearItemDTO dto)
    {
        var result = await _service.CrearItemAsync(dto);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var item = await _service.ObtenerPorIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var items = await _service.ObtenerTodosAsync();
        return Ok(items);
    }

    [HttpGet("inventario")]
    public async Task<IActionResult> GetInventario()
    {
        var datos = await _service.GetInventarioAsync();
        return Ok(datos);
    }

}
