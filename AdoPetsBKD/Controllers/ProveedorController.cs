using AdoPetsBKD.Application.DTOs.Proveedores;
using AdoPetsBKD.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdoPetsBKD.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorService _proveedorService;
        private readonly ILogger<ProveedorController> _logger;

        public ProveedorController(IProveedorService proveedorService, ILogger<ProveedorController> logger)
        {
            _proveedorService = proveedorService;
            _logger = logger;
        }

        // 🟢 Crear proveedor
        [HttpPost("crear")]
        public async Task<IActionResult> CrearProveedor([FromBody] CreateProveedorDto dto)
        {
            try
            {
                var result = await _proveedorService.CreateProveedorAsync(dto);
                return Ok(new { message = "Proveedor creado exitosamente", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el proveedor");
                return StatusCode(500, new { message = "Error interno al crear el proveedor" });
            }
        }

        // 🔵 Obtener todos los proveedores
        [HttpGet("listar")]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var proveedores = await _proveedorService.GetAllProveedoresAsync();
                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los proveedores");
                return StatusCode(500, new { message = "Error interno al obtener los proveedores" });
            }
        }

        // 🔵 Obtener proveedor por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(Guid id)
        {
            try
            {
                var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
                if (proveedor == null)
                    return NotFound(new { message = "Proveedor no encontrado" });

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener el proveedor con ID {id}");
                return StatusCode(500, new { message = "Error interno al obtener el proveedor" });
            }
        }

        // 🟠 Actualizar proveedor
        [HttpPut("actualizar/{id}")]
        public async Task<IActionResult> ActualizarProveedor(Guid id, [FromBody] UpdateProveedorDto dto)
        {
            try
            {
                var proveedor = await _proveedorService.UpdateProveedorAsync(id, dto);
                return Ok(new { message = "Proveedor actualizado exitosamente", data = proveedor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar el proveedor con ID {id}");
                return StatusCode(500, new { message = "Error interno al actualizar el proveedor" });
            }
        }

        // 🔴 Desactivar proveedor
        [HttpPut("desactivar/{id}")]
        public async Task<IActionResult> DesactivarProveedor(Guid id)
        {
            try
            {
                await _proveedorService.DesactivarProveedorAsync(id);
                return Ok(new { message = "Proveedor desactivado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al desactivar el proveedor con ID {id}");
                return StatusCode(500, new { message = "Error interno al desactivar el proveedor" });
            }
        }

        // 🟣 Cambiar estatus (Activo, Inactivo, Bloqueado)
        [HttpPut("estatus/{id}")]
        public async Task<IActionResult> CambiarEstatus(Guid id, [FromQuery] int nuevoEstatus)
        {
            try
            {
                var proveedor = await _proveedorService.CambiarEstatusProveedorAsync(id, nuevoEstatus);
                return Ok(new { message = "Estatus actualizado correctamente", data = proveedor });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar el estatus del proveedor con ID {id}");
                return StatusCode(500, new { message = "Error interno al cambiar el estatus del proveedor" });
            }
        }
    }
}
