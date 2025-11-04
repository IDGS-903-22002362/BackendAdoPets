using AdoPetsBKD.Application.DTOs.Mascota;

namespace AdoPetsBKD.Application.Interfaces.Services
{
    public interface IUMascotaService
    {
        Task<MascotaDetailDto?> GetByIdAsync(Guid id);
        Task<MascotaDetailDto> CreateAsync(CreateMascotaDto dto, Guid createdBy);

        Task<IEnumerable<MascotaDetailDto>> GetAllAsync(FiltroMascotaDto? filtro = null);
        Task<MascotaDetailDto> UpdateAsync(Guid id, UpdateMascotaDto dto,  Guid updatedBy);

        Task<MascotaDetailDto> AddPhotosAsync(Guid idMascota, IEnumerable<CreatePhotoDto> fotosDto, Guid createdBy);

        Task<MascotaDetailDto> DeleteAsync(DeleteMascotaDto dto);

        Task<string> DeletePhotoAsync(Guid fotoId);
        // Solicitud de adopcion
        Task<IEnumerable<SolicitudeAdopcionDetailDto>> GetAllSolicitudesAdopcionAsync();

        Task<string> CrearSolicitudAdopcionAsync(CreateSolicitudeAdopcionDto dto);

        Task<SolicitudeAdopcionDetailDto?> GetSolicitudAdopcionByIdAsync(Guid solicitudId);


        Task<CambiarEstadoSolicitudEnRevisionDto> UpdateStatusSolicitudAdopcionAsync(CambiarEstadoSolicitudEnRevisionDto dto);

        Task<EstadoSolicitudeAceptadaDto> UpdateStatusSolicitudAprobadaAsync(EstadoSolicitudeAceptadaDto dto);

        Task<SolicirudRechasadaDto> UpdateStatusSolicitudRechazadaAsync(SolicirudRechasadaDto dto);

        Task<CancelarSolicitudDto> UpdateStatusSolicitudCanceladaAsync(CancelarSolicitudDto dto);

        Task<IEnumerable<SolicitudeAdopcionDetailDto>> GetSolicitudbyUsuarioIdAsync(Guid usuarioId);

        
        Task<AdopcionLogDto> AddAdopcionLogAsync(AdopcionLogDto dto);

    }
}
