using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Mascotas;
using Microsoft.AspNetCore.Mvc;


namespace AdoPetsBKD.Infrastructure.Services
{
    public class MascotaService : IUMascotaService
    {

        private readonly IUMascotaRepositoty _mascotaRepository;


        public MascotaService(IUMascotaRepositoty mascotaRepository)
        {
            _mascotaRepository = mascotaRepository;
        }

        public async Task<MascotaDetailDto?> GetByIdAsync(Guid id)
        {
            var mascota = await _mascotaRepository.GetByIdAsync(id);
            if (mascota == null) return null;
            return new MascotaDetailDto
            {
                Id = mascota.Id,
                Nombre = mascota.Nombre,
                Especie = mascota.Especie,
                Raza = mascota.Raza,
                FechaNacimiento = mascota.FechaNacimiento,
                Sexo = mascota.Sexo,
                Estatus = mascota.Estatus,
                Personalidad = mascota.Personalidad,
                EstadoSalud = mascota.EstadoSalud,
                Origen = mascota.Origen,
                Notas = mascota.Notas,
                Fotos = mascota.Fotos?.Select(f => new AddMascotaFotoDto
                {
                    StorageKey = f.StorageKey,
                    MimeType = f.MimeType,
                    Orden = f.Orden,
                    EsPrincipal = f.EsPrincipal
                }).ToList() ?? new List<AddMascotaFotoDto>(),
                RequisitoAdopcion = mascota.RequisitoAdopcion,
                CreatedAt = mascota.CreatedAt
            };
        }
        public async Task<MascotaDetailDto> CreateAsync(CreateMascotaDto dto, Guid createdBy)
        {
            var mascota = new Mascota
            {
                Nombre = dto.Nombre,
                Especie = dto.Especie,
                Raza = dto.Raza,
                FechaNacimiento = dto.FechaNacimiento,
                Sexo = dto.Sexo,
                Personalidad = dto.Personalidad,
                EstadoSalud = dto.EstadoSalud,
                RequisitoAdopcion = dto.RequisitoAdopcion,
                Origen = dto.Origen,
                Notas = dto.Notas,
                CreatedBy = createdBy,
            };
            await _mascotaRepository.CreateAsync(mascota);
            await _mascotaRepository.SaveChangesAsync();
            return (await GetByIdAsync(mascota.Id));
        }


    public async Task<MascotaDetailDto> UpdateAsync(Guid id, UpdateMascotaDto dto, Guid updatedBy)
        {
            var mascota = await _mascotaRepository.GetByIdAsync(id);
            if (mascota == null)
            {
                throw new InvalidOperationException("La mascota no existe");

            }
            mascota.Nombre = dto.Nombre;
            mascota.Especie = dto.Especie;
            mascota.Raza = dto.Raza;
            mascota.FechaNacimiento = dto.FechaNacimiento;
            mascota.Sexo = dto.Sexo;
            mascota.Estatus = dto.Estatus;
            mascota.Personalidad = dto.Personalidad;
            mascota.EstadoSalud = dto.EstadoSalud;
            mascota.RequisitoAdopcion = dto.RequisitoAdopcion;
            mascota.Origen = dto.Origen;
            mascota.Notas = dto.Notas;
            mascota.UpdatedBy = updatedBy;
            await _mascotaRepository.UpdateAsync(mascota);
            await _mascotaRepository.SaveChangesAsync();
            return (await GetByIdAsync(mascota.Id));
        }

        public async Task<MascotaDetailDto> DeleteAsync(DeleteMascotaDto dto)
        {
            var mascota = await _mascotaRepository.GetByIdAsync(dto.Id);
            if (mascota == null)
                throw new InvalidOperationException("La mascota no existe");

            mascota.Estatus = EstatusMascota.NoAdoptable; 
            mascota.UpdatedAt = DateTime.UtcNow;

            await _mascotaRepository.UpdateAsync(mascota);
            await _mascotaRepository.SaveChangesAsync();

            return await GetByIdAsync(mascota.Id);
        }



        // Metodos para las Fotos de la mascota
        public async Task<MascotaDetailDto> AddPhotosAsync(Guid mascotaId, IEnumerable<CreatePhotoDto> fotosDto, Guid createdBy)
        {
            var mascota = await _mascotaRepository.GetByIdAsync(mascotaId);
            if (mascota == null)
                throw new InvalidOperationException("La mascota no existe");

            int ultimoOrden = mascota.Fotos?.Count > 0 ? mascota.Fotos.Max(f => f.Orden) : 0;

            bool yaHayPrincipal = mascota.Fotos?.Any(f => f.EsPrincipal) ?? false;

            
            var nuevasFotos = fotosDto.Select((f, index) => new MascotaFoto
            {
                Id = Guid.NewGuid(),
                MascotaId = mascotaId,
                StorageKey = f.StorageKey,
                MimeType = f.MimeType,
                Orden = ultimoOrden + index + 1,
                EsPrincipal = !yaHayPrincipal && index == 0, 
                UploadedAt = DateTime.UtcNow,
            }).ToList();

            await _mascotaRepository.AddPhotoAsync(nuevasFotos);
            await _mascotaRepository.SaveChangesAsync();

            var mascotaActualizada = await _mascotaRepository.GetByIdAsync(mascotaId);

            return new MascotaDetailDto
            {
                Id = mascotaActualizada.Id,
                Nombre = mascotaActualizada.Nombre,
                Especie = mascotaActualizada.Especie,
                Raza = mascotaActualizada.Raza,
                FechaNacimiento = mascotaActualizada.FechaNacimiento,
                Sexo = mascotaActualizada.Sexo,
                Estatus = mascotaActualizada.Estatus,
                Personalidad = mascotaActualizada.Personalidad,
                EstadoSalud = mascotaActualizada.EstadoSalud,
                Origen = mascotaActualizada.Origen,
                Notas = mascotaActualizada.Notas,
                RequisitoAdopcion = mascotaActualizada.RequisitoAdopcion,
                Fotos = mascotaActualizada.Fotos?.Select(f => new AddMascotaFotoDto
                {
                    StorageKey = f.StorageKey,
                    MimeType = f.MimeType,
                    Orden = f.Orden,
                    EsPrincipal = f.EsPrincipal
                }).OrderBy(f => f.Orden).ToList() ?? new List<AddMascotaFotoDto>(),
                CreatedAt = mascotaActualizada.CreatedAt,
                UpdatedAt = mascotaActualizada.UpdatedAt
            };
        }

        public async Task<string> DeletePhotoAsync(Guid fotoId)
        {
            var foto = await _mascotaRepository.DeletePhotoAsync(fotoId);
            if (foto == null)
                throw new InvalidOperationException("La foto no existe");

            await _mascotaRepository.SaveChangesAsync();
            return "Foto eliminada correctamente";
        }

    }
}
