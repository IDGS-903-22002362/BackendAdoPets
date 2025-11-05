using AdoPetsBKD.Application.DTOs.Clinica;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Clinica;

namespace AdoPetsBKD.Infrastructure.Services;

public class SalaService : ISalaService
{
    private readonly ISalaRepository _salaRepository;

    public SalaService(ISalaRepository salaRepository)
    {
        _salaRepository = salaRepository;
    }

    public async Task<SalaDetailDto?> GetByIdAsync(Guid id)
    {
        var sala = await _salaRepository.GetByIdAsync(id);
        return sala == null ? null : MapToDetailDto(sala);
    }

    public async Task<List<SalaListDto>> GetAllAsync()
    {
        var salas = await _salaRepository.GetAllAsync();
        return salas.Select(MapToListDto).ToList();
    }

    public async Task<List<SalaListDto>> GetActiveAsync()
    {
        var salas = await _salaRepository.GetActiveAsync();
        return salas.Select(MapToListDto).ToList();
    }

    public async Task<SalaDetailDto> CreateAsync(CreateSalaDto dto, Guid userId)
    {
        // Validar que el nombre no exista
        var exists = await _salaRepository.ExistsByNombreAsync(dto.Nombre);
        if (exists)
        {
            throw new InvalidOperationException($"Ya existe una sala con el nombre '{dto.Nombre}'");
        }

        var sala = new Sala
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            CapacidadMaxima = dto.Capacidad,
            Activa = true
        };

        await _salaRepository.AddAsync(sala);

        var createdSala = await _salaRepository.GetByIdAsync(sala.Id);
        return MapToDetailDto(createdSala!);
    }

    public async Task<SalaDetailDto> UpdateAsync(Guid id, UpdateSalaDto dto, Guid userId)
    {
        var sala = await _salaRepository.GetByIdAsync(id);
        if (sala == null)
        {
            throw new KeyNotFoundException("Sala no encontrada");
        }

        // Si se está cambiando el nombre, validar que no exista
        if (dto.Nombre != null && dto.Nombre != sala.Nombre)
        {
            var exists = await _salaRepository.ExistsByNombreAsync(dto.Nombre, id);
            if (exists)
            {
                throw new InvalidOperationException($"Ya existe una sala con el nombre '{dto.Nombre}'");
            }
            sala.Nombre = dto.Nombre;
        }

        if (dto.Descripcion != null)
            sala.Descripcion = dto.Descripcion;

        if (dto.Capacidad.HasValue)
            sala.CapacidadMaxima = dto.Capacidad.Value;

        if (dto.Activa.HasValue)
            sala.Activa = dto.Activa.Value;

        await _salaRepository.UpdateAsync(sala);

        var updatedSala = await _salaRepository.GetByIdAsync(id);
        return MapToDetailDto(updatedSala!);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var sala = await _salaRepository.GetByIdAsync(id);
        if (sala == null)
        {
            throw new KeyNotFoundException("Sala no encontrada");
        }

        await _salaRepository.DeleteAsync(sala);
    }

    // Mappers
    private static SalaListDto MapToListDto(Sala sala)
    {
        return new SalaListDto
        {
            Id = sala.Id,
            Nombre = sala.Nombre,
            Descripcion = sala.Descripcion,
            Capacidad = sala.CapacidadMaxima ?? 1,
            Activa = sala.Activa
        };
    }

    private static SalaDetailDto MapToDetailDto(Sala sala)
    {
        return new SalaDetailDto
        {
            Id = sala.Id,
            Nombre = sala.Nombre,
            Descripcion = sala.Descripcion,
            Capacidad = sala.CapacidadMaxima ?? 1,
            Activa = sala.Activa,
            CreatedAt = DateTime.UtcNow, // BaseEntity no tiene CreatedAt
            UpdatedAt = null,
            DeletedAt = null
        };
    }
}
