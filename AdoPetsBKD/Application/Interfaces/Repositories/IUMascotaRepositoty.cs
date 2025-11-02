using AdoPetsBKD.Domain.Entities.Mascotas;

namespace AdoPetsBKD.Application.Interfaces.Repositories
{
    public interface IUMascotaRepositoty
    {
        
        Task<Mascota?> GetByIdAsync(Guid id, bool includeFotos = true);

        Task<Mascota> CreateAsync(Mascota mascota);

        Task<Mascota> UpdateAsync(Mascota mascota);

        Task<Mascota> DeleteAsync(Guid id);

        Task<MascotaFoto> DeletePhotoAsync(Guid Id);

        Task<IEnumerable<MascotaFoto>> AddPhotoAsync(IEnumerable<MascotaFoto> foto);

        Task SaveChangesAsync();
    }
}
