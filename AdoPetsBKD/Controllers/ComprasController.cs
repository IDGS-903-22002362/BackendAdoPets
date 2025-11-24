using AdoPetsBKD.Application.DTOs.Inventario;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;


namespace AdoPetsBKD.Api.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class ComprasController : ControllerBase
{
    private readonly ICompraService _compraService;


    public ComprasController(ICompraService compraService)
    {
        _compraService = compraService;
    }


    [HttpPost("surtir")]
    public async Task<IActionResult> Surtir([FromBody] CrearCompraDTO dto)
    {
        Guid userId = Guid.NewGuid(); // Sustituir por el user real
        var id = await _compraService.SurtirAsync(dto, userId);
        return Ok(new { CompraId = id });
    }
}