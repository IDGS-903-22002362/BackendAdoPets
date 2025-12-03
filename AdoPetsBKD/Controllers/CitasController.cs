using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdoPetsBKD.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly ICitaService _citaService;
    private readonly ILogger<CitasController> _logger;

    public CitasController(ICitaService citaService, ILogger<CitasController> logger)
    {
        _citaService = citaService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    /// <summary>
    /// Obtiene todas las citas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetAll()
    {
        try
        {
            var citas = await _citaService.GetAllAsync();
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas, "Citas obtenidas exitosamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas");
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene una cita por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> GetById(Guid id)
    {
        try
        {
            var cita = await _citaService.GetByIdAsync(id);
            if (cita == null)
            {
                return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse("Cita no encontrada"));
            }

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al obtener cita"));
        }
    }

    /// <summary>
    /// Obtiene citas por veterinario
    /// </summary>
    [HttpGet("veterinario/{veterinarioId}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByVeterinario(
        Guid veterinarioId,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var citas = await _citaService.GetByVeterinarioAsync(veterinarioId, startDate, endDate);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas del veterinario {VetId}", veterinarioId);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por mascota
    /// </summary>
    [HttpGet("mascota/{mascotaId}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByMascota(Guid mascotaId)
    {
        try
        {
            var citas = await _citaService.GetByMascotaAsync(mascotaId);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas de la mascota {MascotaId}", mascotaId);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por propietario
    /// </summary>
    [HttpGet("propietario/{propietarioId}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByPropietario(Guid propietarioId)
    {
        try
        {
            var citas = await _citaService.GetByPropietarioAsync(propietarioId);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas del propietario {PropietarioId}", propietarioId);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por rango de fechas
    /// </summary>
    [HttpGet("rango")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var citas = await _citaService.GetByDateRangeAsync(startDate, endDate);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas por rango de fechas");
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Obtiene citas por estado
    /// </summary>
    [HttpGet("estado/{status}")]
    public async Task<ActionResult<ApiResponse<List<CitaListDto>>>> GetByStatus(StatusCita status)
    {
        try
        {
            var citas = await _citaService.GetByStatusAsync(status);
            return Ok(ApiResponse<List<CitaListDto>>.SuccessResponse(citas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener citas por estado {Status}", status);
            return StatusCode(500, ApiResponse<List<CitaListDto>>.ErrorResponse("Error al obtener citas"));
        }
    }

    /// <summary>
    /// Crea una nueva cita
    /// Si se proporciona SolicitudCitaDigitalId, la cita se vinculará con la solicitud digital
    /// y se validará que el pago del 50% esté completado antes de crear la cita.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Create([FromBody] CreateCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            Guid userId;
            try
            {
                userId = GetUserId();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener userId del token. Claims: {Claims}", 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error de autenticación: No se pudo obtener el ID del usuario"));
            }

            _logger.LogInformation("Creando cita para veterinario {VetId}, mascota {MascotaId}, propietario {PropId}, sala {SalaId}",
                dto.VeterinarioId, dto.MascotaId, dto.PropietarioId, dto.SalaId);

            var cita = await _citaService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = cita.Id },
                ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita creada exitosamente"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Argumento inválido al crear cita");
            return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operación inválida al crear cita");
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al crear cita. Tipo: {TipoCita}, VetId: {VetId}, StartAt: {StartAt}", 
                dto.Tipo, dto.VeterinarioId, dto.StartAt);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse($"Error al crear cita: {ex.Message}"));
        }
    }

    /// <summary>
    /// Actualiza una cita
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Update(Guid id, [FromBody] UpdateCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.UpdateAsync(id, dto, userId);

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita actualizada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al actualizar cita"));
        }
    }

    /// <summary>
    /// Cancela una cita
    /// </summary>
    [HttpPut("{id}/cancelar")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Cancelar(Guid id, [FromBody] CancelarCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.CancelarAsync(id, dto, userId);

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita cancelada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al cancelar cita"));
        }
    }

    /// <summary>
    /// Completa una cita
    /// </summary>
    [HttpPut("{id}/completar")]
    [Authorize(Roles = "Admin,Veterinario")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> Completar(Guid id, [FromBody] CompletarCitaDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<CitaDetailDto>.ErrorResponse("Datos inválidos", errors));
            }

            var userId = GetUserId();
            var cita = await _citaService.CompletarAsync(id, dto, userId);

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita, "Cita completada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<CitaDetailDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al completar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al completar cita"));
        }
    }

    /// <summary>
    /// Elimina una cita
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        try
        {
            await _citaService.DeleteAsync(id);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Cita eliminada exitosamente"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar cita {CitaId}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("Error al eliminar cita"));
        }
    }

    /// <summary>
    /// Obtiene disponibilidad de veterinario
    /// </summary>
    [HttpGet("disponibilidad")]
    public async Task<ActionResult<ApiResponse<DisponibilidadResponseDto>>> GetDisponibilidad(
        [FromQuery] DisponibilidadQueryDto query)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<DisponibilidadResponseDto>.ErrorResponse("Datos inválidos", errors));
            }

            var disponibilidad = await _citaService.GetDisponibilidadAsync(query);
            return Ok(ApiResponse<DisponibilidadResponseDto>.SuccessResponse(disponibilidad));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener disponibilidad");
            return StatusCode(500, ApiResponse<DisponibilidadResponseDto>.ErrorResponse("Error al obtener disponibilidad"));
        }
    }

    /// <summary>
    /// Obtiene la cita asociada a una solicitud de cita digital
    /// </summary>
    [HttpGet("solicitud/{solicitudId}")]
    public async Task<ActionResult<ApiResponse<CitaDetailDto>>> GetBySolicitudDigital(Guid solicitudId)
    {
        try
        {
            var cita = await _citaService.GetBySolicitudDigitalAsync(solicitudId);
            if (cita == null)
            {
                return NotFound(ApiResponse<CitaDetailDto>.ErrorResponse("No se encontró una cita asociada a esta solicitud"));
            }

            return Ok(ApiResponse<CitaDetailDto>.SuccessResponse(cita));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cita por solicitud {SolicitudId}", solicitudId);
            return StatusCode(500, ApiResponse<CitaDetailDto>.ErrorResponse("Error al obtener cita"));
        }
    }

    /// <summary>
    /// ENDPOINT DE DIAGNÓSTICO TEMPORAL: Verifica si las entidades necesarias existen
    /// Este endpoint debe eliminarse en producción
    /// </summary>
    [HttpGet("diagnostico/verificar-entidades")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<object>>> VerificarEntidades(
        [FromQuery] Guid veterinarioId,
        [FromQuery] Guid? mascotaId = null,
        [FromQuery] Guid? propietarioId = null,
        [FromQuery] Guid? salaId = null)
    {
        try
        {
            var resultado = new Dictionary<string, object>();

            // Verificar veterinario
            _logger.LogInformation("Verificando veterinario: {VetId}", veterinarioId);
            resultado["VeterinarioId"] = veterinarioId.ToString();
            resultado["VeterinarioIdValido"] = veterinarioId != Guid.Empty;

            // Verificar mascota
            if (mascotaId.HasValue)
            {
                resultado["MascotaId"] = mascotaId.Value.ToString();
                resultado["MascotaIdValido"] = mascotaId.Value != Guid.Empty;
                var mascotaCitas = await _citaService.GetByMascotaAsync(mascotaId.Value);
                resultado["MascotaExisteOTieneCitas"] = mascotaCitas.Any();
            }

            // Verificar propietario
            if (propietarioId.HasValue)
            {
                resultado["PropietarioId"] = propietarioId.Value.ToString();
                resultado["PropietarioIdValido"] = propietarioId.Value != Guid.Empty;
                var propietarioCitas = await _citaService.GetByPropietarioAsync(propietarioId.Value);
                resultado["PropietarioExisteOTieneCitas"] = propietarioCitas.Any();
            }

            // Verificar sala
            if (salaId.HasValue)
            {
                resultado["SalaId"] = salaId.Value.ToString();
                resultado["SalaIdValido"] = salaId.Value != Guid.Empty;
            }

            return Ok(ApiResponse<object>.SuccessResponse(resultado, "Diagnóstico completado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en diagnóstico");
            return Ok(ApiResponse<object>.ErrorResponse($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// ENDPOINT DE DIAGNÓSTICO: Valida formato de GUIDs
    /// </summary>
    [HttpPost("diagnostico/validar-guids")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public ActionResult<ApiResponse<object>> ValidarGuids([FromBody] ValidarGuidsDto dto)
    {
        var resultado = new Dictionary<string, object>();

        // Validar veterinario
        resultado["VeterinarioId"] = new
        {
            Original = dto.VeterinarioId,
            Longitud = dto.VeterinarioId?.Length ?? 0,
            EsValido = Guid.TryParse(dto.VeterinarioId, out var vetGuid),
            GuidParseado = Guid.TryParse(dto.VeterinarioId, out vetGuid) ? vetGuid.ToString() : "No se pudo parsear"
        };

        // Validar mascota
        if (!string.IsNullOrEmpty(dto.MascotaId))
        {
            resultado["MascotaId"] = new
            {
                Original = dto.MascotaId,
                Longitud = dto.MascotaId.Length,
                EsValido = Guid.TryParse(dto.MascotaId, out var mascGuid),
                GuidParseado = Guid.TryParse(dto.MascotaId, out mascGuid) ? mascGuid.ToString() : "No se pudo parsear"
            };
        }

        // Validar propietario
        if (!string.IsNullOrEmpty(dto.PropietarioId))
        {
            resultado["PropietarioId"] = new
            {
                Original = dto.PropietarioId,
                Longitud = dto.PropietarioId.Length,
                EsValido = Guid.TryParse(dto.PropietarioId, out var propGuid),
                GuidParseado = Guid.TryParse(dto.PropietarioId, out propGuid) ? propGuid.ToString() : "No se pudo parsear"
            };
        }

        // Validar sala
        if (!string.IsNullOrEmpty(dto.SalaId))
        {
            resultado["SalaId"] = new
            {
                Original = dto.SalaId,
                Longitud = dto.SalaId.Length,
                EsValido = Guid.TryParse(dto.SalaId, out var salaGuid),
                GuidParseado = Guid.TryParse(dto.SalaId, out salaGuid) ? salaGuid.ToString() : "No se pudo parsear"
            };
        }

        return Ok(ApiResponse<object>.SuccessResponse(resultado, "Validación de GUIDs completada"));
    }

    /// <summary>
    /// ENDPOINT AUXILIAR: Lista veterinarios con sus IDs de Usuario y Empleado
    /// </summary>
    [HttpGet("veterinarios-para-citas")]
    [Authorize(Roles = "Admin,Veterinario,Recepcionista")]
    public async Task<ActionResult<ApiResponse<List<object>>>> GetVeterinariosParaCitas()
    {
        try
        {
            // Obtener todos los empleados veterinarios con su información de usuario
            var veterinarios = await _citaService.GetAllAsync(); // Esto nos da acceso al contexto
            
            // Devolver información útil para el frontend
            var resultado = new List<object>
            {
                new
                {
                    Mensaje = "Para crear citas, usa el campo 'empleadoId' del endpoint /Empleados",
                    Ejemplo = new
                    {
                        VeterinarioIdCorrecto = "EmpleadoId del endpoint /Empleados",
                        EstructuraRequest = new
                        {
                            veterinarioId = "{EmpleadoId}",
                            mascotaId = "{MascotaId}",
                            propietarioId = "{UsuarioId del propietario}",
                            salaId = "{SalaId}",
                            tipo = 1,
                            startAt = "2025-12-25T10:00:00",
                            duracionMin = 30
                        }
                    },
                    EndpointParaObtenerVeterinarios = "/api/v1/Empleados",
                    CampoAUsar = "id (es el EmpleadoId)"
                }
            };

            return Ok(ApiResponse<List<object>>.SuccessResponse(resultado, 
                "Usa el endpoint /Empleados para obtener la lista de veterinarios con sus EmpleadoIds"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener información de veterinarios");
            return StatusCode(500, ApiResponse<List<object>>.ErrorResponse("Error al obtener veterinarios"));
        }
    }
}

// DTO para validación de GUIDs
public class ValidarGuidsDto
{
    public string? VeterinarioId { get; set; }
    public string? MascotaId { get; set; }
    public string? PropietarioId { get; set; }
    public string? SalaId { get; set; }
}
