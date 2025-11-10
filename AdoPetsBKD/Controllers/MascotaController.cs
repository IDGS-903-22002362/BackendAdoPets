using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Mascotas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AdoPetsBKD.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MascotaController : ControllerBase
    {

        private readonly IUMascotaService _mascotaService;
        private readonly ILogger<MascotaController> _logger;

        public MascotaController(IUMascotaService mascotaService, ILogger<MascotaController> logger)
        {
            _mascotaService = mascotaService;
            _logger = logger;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var mascota = await _mascotaService.GetByIdAsync(id);
            if (mascota == null) return NotFound();

            // Asegurar que todas las fotos tengan URLs absolutas
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            foreach (var foto in mascota.Fotos ?? Enumerable.Empty<AddMascotaFotoDto>())
            {
                if (!string.IsNullOrEmpty(foto.StorageKey))
                {
                    // Si es una ruta relativa, convertir a absoluta
                    if (foto.StorageKey.StartsWith("/"))
                    {
                        foto.StorageKey = $"{baseUrl}{foto.StorageKey}";
                    }
                    // Si es una ruta con "uploads/" sin slash inicial
                    else if (foto.StorageKey.StartsWith("uploads/"))
                    {
                        foto.StorageKey = $"{baseUrl}/{foto.StorageKey}";
                    }
                    // Si ya es una URL completa, dejarla como está
                }
            }

            return Ok(mascota);
        }

        // Crear una nueva mascota
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMascotaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Guid createdBy = Guid.NewGuid(); 

                var mascotaCreada = await _mascotaService.CreateAsync(dto, createdBy);

                return CreatedAtAction(nameof(GetById), new { id = mascotaCreada.Id }, mascotaCreada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear mascota");
                return StatusCode(500, "Ocurrió un error al crear la mascota");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FiltroMascotaDto filtro)
        {
            var mascotas = await _mascotaService.GetAllAsync(filtro);
            if (mascotas == null) return NotFound();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            // Recorrer cada mascota
            foreach (var mascota in mascotas)
            {
                foreach (var foto in mascota.Fotos ?? Enumerable.Empty<AddMascotaFotoDto>())
                {
                    if (!string.IsNullOrEmpty(foto.StorageKey))
                    {
                        if (foto.StorageKey.StartsWith("/"))
                        {
                            foto.StorageKey = $"{baseUrl}{foto.StorageKey}";
                        }
                        else if (foto.StorageKey.StartsWith("uploads/"))
                        {
                            foto.StorageKey = $"{baseUrl}/{foto.StorageKey}";
                        }
                        // Si ya es una URL completa, no se cambia
                    }
                }
            }

            return Ok(mascotas);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMascotaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                Guid updatedBy = Guid.NewGuid(); 
                var mascotaActualizada = await _mascotaService.UpdateAsync(id, dto, updatedBy);
                if (mascotaActualizada == null)
                    return NotFound();
                return Ok(new {message = "Mascota eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar mascota");
                return StatusCode(500, "Ocurrió un error al actualizar la mascota");
            }
        }

        [HttpPatch("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteMascotaDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Datos inválidos.");

                var mascotaActualizada = await _mascotaService.DeleteAsync(dto);

                return Ok(new {message = "Mascota eliminada correctamente"});
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la mascota");
                return StatusCode(500, "Ocurrió un error al eliminar la mascota");
            }
        }



        // Agregar Foto
        [HttpPost("{id}/photos")]
        public async Task<IActionResult> AddPhotos(Guid id, [FromBody] List<CreatePhotoDto> fotosDto)
        {
            if (fotosDto == null || fotosDto.Count == 0)
                return BadRequest("No se recibió información de las fotos.");

            var mascotaActualizada = await _mascotaService.AddPhotosAsync(
                id,
                fotosDto,
                createdBy: Guid.NewGuid()
                );

            if (mascotaActualizada == null)
                return NotFound("La mascota no existe.");

            //return Ok(mascotaActualizada);
            return Ok(new { message = "Foto agregada correctamente" });
        }

        [HttpDelete("foto/{fotoId}")]
        public async Task<IActionResult> DeletePhoto(Guid fotoId)
        {
            try
            {
                var mensaje = await _mascotaService.DeletePhotoAsync(fotoId);
                return Ok(new { message = mensaje });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al eliminar la foto", detalle = ex.Message });
            }
        }

    }
}
