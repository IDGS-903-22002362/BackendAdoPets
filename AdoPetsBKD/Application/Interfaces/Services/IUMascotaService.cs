using AdoPetsBKD.Application.DTOs.Mascota;

namespace AdoPetsBKD.Application.Interfaces.Services
{
    public interface IUMascotaService
    {
        Task<MascotaDetailDto?> GetByIdAsync(Guid id);
        Task<MascotaDetailDto> CreateAsync(CreateMascotaDto dto, Guid createdBy);

        Task<MascotaDetailDto> UpdateAsync(Guid id, UpdateMascotaDto dto,  Guid updatedBy);

        Task<MascotaDetailDto> AddPhotosAsync(Guid idMascota, IEnumerable<CreatePhotoDto> fotosDto, Guid createdBy);

        Task<MascotaDetailDto> DeleteAsync(DeleteMascotaDto dto);

        Task<string> DeletePhotoAsync(Guid fotoId);

    }
}
