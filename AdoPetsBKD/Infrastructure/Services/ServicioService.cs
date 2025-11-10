using AdoPetsBKD.Application.DTOs.Servicios;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Services;

public class ServicioService : IServicioService
{
    private readonly AdoPetsDbContext _context;

    public ServicioService(AdoPetsDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServicioDto>> GetAllAsync(bool incluirInactivos = false)
    {
        var query = _context.Servicios.AsQueryable();

        if (!incluirInactivos)
        {
            query = query.Where(s => s.Activo);
        }

        var servicios = await query
            .OrderBy(s => s.Categoria)
            .ThenBy(s => s.Descripcion)
            .ToListAsync();

        return servicios.Select(MapToDto).ToList();
    }

    public async Task<List<ServicioDto>> GetActivosAsync()
    {
        var servicios = await _context.Servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.Categoria)
            .ThenBy(s => s.Descripcion)
            .ToListAsync();

        return servicios.Select(MapToDto).ToList();
    }

    public async Task<ServicioDto?> GetByIdAsync(Guid id)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        return servicio == null ? null : MapToDto(servicio);
    }

    public async Task<ServicioDto> CreateAsync(CreateServicioDto dto, Guid createdBy)
    {
        var servicio = new Servicio
        {
            Descripcion = dto.Descripcion,
            Categoria = dto.Categoria,
            DuracionMinDefault = dto.DuracionMinDefault,
            PrecioSugerido = dto.PrecioSugerido,
            Notas = dto.Notas,
            Activo = true
        };

        _context.Servicios.Add(servicio);
        await _context.SaveChangesAsync();

        return MapToDto(servicio);
    }

    public async Task<ServicioDto> UpdateAsync(Guid id, UpdateServicioDto dto, Guid updatedBy)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null)
        {
            throw new InvalidOperationException("Servicio no encontrado");
        }

        servicio.Descripcion = dto.Descripcion;
        servicio.Categoria = dto.Categoria;
        servicio.DuracionMinDefault = dto.DuracionMinDefault;
        servicio.PrecioSugerido = dto.PrecioSugerido;
        servicio.Notas = dto.Notas;
        servicio.Activo = dto.Activo;

        await _context.SaveChangesAsync();

        return MapToDto(servicio);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid deletedBy)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null)
        {
            return false;
        }

        servicio.Desactivar();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivarAsync(Guid id, Guid updatedBy)
    {
        var servicio = await _context.Servicios.FindAsync(id);
        if (servicio == null)
        {
            return false;
        }

        servicio.Activar();
        await _context.SaveChangesAsync();
        return true;
    }

    private static ServicioDto MapToDto(Servicio servicio)
    {
        return new ServicioDto
        {
            Id = servicio.Id,
            Descripcion = servicio.Descripcion,
            Categoria = servicio.Categoria,
            CategoriaNombre = servicio.Categoria.ToString(),
            DuracionMinDefault = servicio.DuracionMinDefault,
            PrecioSugerido = servicio.PrecioSugerido ?? 0,
            Notas = servicio.Notas,
            Activo = servicio.Activo,
            CreatedAt = DateTime.UtcNow // BaseEntity no expone CreatedAt públicamente
        };
    }
}
