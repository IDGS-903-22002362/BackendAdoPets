using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.Interfaces.Repositories
{
    public interface IUMascotaRepositoty
    {
        
        Task<Mascota?> GetByIdAsync(Guid id, bool includeFotos = true);

        Task<IEnumerable<Mascota>> GetAllAsync(bool includeFotos = true);

        Task<Mascota> CreateAsync(Mascota mascota);

        Task<Mascota> UpdateAsync(Mascota mascota);

        Task<Mascota> DeleteAsync(Guid id);

        Task<MascotaFoto> DeletePhotoAsync(Guid Id);

        Task<IEnumerable<MascotaFoto>> AddPhotoAsync(IEnumerable<MascotaFoto> foto);


        // Solicitude de adopcion 

        // Obtener solicitud de adopción por Id
        Task<SolicitudAdopcion?> GetSolicitudByIdAsync(Guid solicitudId);

        Task<SolicitudAdopcion> CreateSolicitudAdopcionAsync(SolicitudAdopcion solicitud);

        Task<IEnumerable<SolicitudAdopcion>> GetAllSolicitudesAdopcionAsync();

        Task <SolicitudAdopcion> UpdateStatusSolicitudAdopcionAsync(SolicitudAdopcion solicitud);

        Task <SolicitudAdopcion> UpdateSolicitudAdopcionAceptadaAsync(SolicitudAdopcion solicitud);

        Task<SolicitudAdopcion> UpdateSolicitudAdopcionRechazadaAsync(SolicitudAdopcion solicitud);

        Task <SolicitudAdopcion> UpdateSolicitudAdopcionCanceladaAsync(SolicitudAdopcion solicitud);

        Task<IEnumerable<SolicitudAdopcion>> GetSolicitudbyUsuarioIdAsync(Guid usuarioId);
        Task<AdopcionLog> AddAdopcionLogAsync(AdopcionLog log);

  
        Task SaveChangesAsync();
    }
}
