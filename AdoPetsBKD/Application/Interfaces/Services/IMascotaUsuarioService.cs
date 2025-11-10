using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.DTOs.Mascota.MascotaUsuario;

namespace AdoPetsBKD.Application.Interfaces.Services;

/// <summary>
/// Servicio para gestionar las mascotas de los usuarios (no del refugio)
/// </summary>
public interface IMascotaUsuarioService
{
    /// <summary>
    /// Obtiene una mascota de usuario por ID, verificando que pertenezca al usuario
    /// </summary>
    Task<MascotaUsuarioDetailDto?> GetByIdAsync(Guid mascotaId, Guid usuarioId);
    
    /// <summary>
    /// Obtiene todas las mascotas de un usuario
    /// </summary>
    Task<IEnumerable<MascotaUsuarioDetailDto>> GetMascotasByUsuarioAsync(Guid usuarioId);
    
    /// <summary>
    /// Crea una nueva mascota para un usuario
    /// </summary>
    Task<MascotaUsuarioDetailDto> CreateAsync(CreateMascotaUsuarioDto dto, Guid usuarioId);
    
    /// <summary>
    /// Actualiza una mascota de usuario, verificando que pertenezca al usuario
    /// </summary>
    Task<MascotaUsuarioDetailDto> UpdateAsync(Guid mascotaId, UpdateMascotaUsuarioDto dto, Guid usuarioId);
    
    /// <summary>
    /// Elimina (soft delete) una mascota de usuario
    /// </summary>
    Task<bool> DeleteAsync(Guid mascotaId, Guid usuarioId);
    
    /// <summary>
    /// Agrega fotos a una mascota de usuario
    /// </summary>
    Task<MascotaUsuarioDetailDto> AddPhotosAsync(Guid mascotaId, IEnumerable<CreatePhotoDto> fotosDto, Guid usuarioId);
    
    /// <summary>
    /// Elimina una foto de una mascota de usuario
    /// </summary>
    Task<string> DeletePhotoAsync(Guid fotoId, Guid usuarioId);
}
