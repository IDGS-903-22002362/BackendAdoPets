using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.DTOs.Mascota.MascotaUsuario;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Mascotas;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AdoPetsBKD.Infrastructure.Services;

public class MascotaUsuarioService : IMascotaUsuarioService
{
    private readonly IUMascotaRepositoty _mascotaRepository;

    public MascotaUsuarioService(IUMascotaRepositoty mascotaRepository)
    {
        _mascotaRepository = mascotaRepository;
    }

    public async Task<MascotaUsuarioDetailDto?> GetByIdAsync(Guid mascotaId, Guid usuarioId)
    {
        var mascota = await _mascotaRepository.GetByIdAsync(mascotaId, includeFotos: true);
        
        if (mascota == null || mascota.Tipo != TipoMascota.DeUsuario || mascota.PropietarioId != usuarioId)
            return null;

        return MapToDetailDto(mascota);
    }

    public async Task<IEnumerable<MascotaUsuarioDetailDto>> GetMascotasByUsuarioAsync(Guid usuarioId)
    {
        var mascotas = await _mascotaRepository.GetAllAsync(includeFotos: true);
        
        return mascotas
            .Where(m => m.Tipo == TipoMascota.DeUsuario && m.PropietarioId == usuarioId && !m.IsDeleted)
            .Select(MapToDetailDto)
            .ToList();
    }

    public async Task<MascotaUsuarioDetailDto> CreateAsync(CreateMascotaUsuarioDto dto, Guid usuarioId)
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
            Notas = dto.Notas,
            
            // Campos específicos para mascotas de usuario
            Tipo = TipoMascota.DeUsuario,
            PropietarioId = usuarioId,
            Estatus = EstatusMascota.NoAdoptable, // Las mascotas de usuario no son adoptables
            
            CreatedBy = usuarioId,
            CreatedAt = DateTime.UtcNow
        };

        await _mascotaRepository.CreateAsync(mascota);
        await _mascotaRepository.SaveChangesAsync();

        var mascotaCreada = await _mascotaRepository.GetByIdAsync(mascota.Id, includeFotos: true);
        return MapToDetailDto(mascotaCreada!);
    }

    public async Task<MascotaUsuarioDetailDto> UpdateAsync(Guid mascotaId, UpdateMascotaUsuarioDto dto, Guid usuarioId)
    {
        var mascota = await _mascotaRepository.GetByIdAsync(mascotaId);
        
        if (mascota == null || mascota.Tipo != TipoMascota.DeUsuario || mascota.PropietarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para editar esta mascota");

        mascota.Nombre = dto.Nombre;
        mascota.Especie = dto.Especie;
        mascota.Raza = dto.Raza;
        mascota.FechaNacimiento = dto.FechaNacimiento;
        mascota.Sexo = dto.Sexo;
        mascota.Personalidad = dto.Personalidad;
        mascota.EstadoSalud = dto.EstadoSalud;
        mascota.Notas = dto.Notas;
        mascota.UpdatedBy = usuarioId;
        mascota.UpdatedAt = DateTime.UtcNow;

        await _mascotaRepository.UpdateAsync(mascota);
        await _mascotaRepository.SaveChangesAsync();

        var mascotaActualizada = await _mascotaRepository.GetByIdAsync(mascotaId, includeFotos: true);
        return MapToDetailDto(mascotaActualizada!);
    }

    public async Task<bool> DeleteAsync(Guid mascotaId, Guid usuarioId)
    {
        var mascota = await _mascotaRepository.GetByIdAsync(mascotaId);
        
        if (mascota == null || mascota.Tipo != TipoMascota.DeUsuario || mascota.PropietarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para eliminar esta mascota");

        // SoftDelete: solo establecemos DeletedAt y DeletedBy
        mascota.DeletedAt = DateTime.UtcNow;
        mascota.DeletedBy = usuarioId;

        await _mascotaRepository.UpdateAsync(mascota);
        await _mascotaRepository.SaveChangesAsync();

        return true;
    }

    public async Task<MascotaUsuarioDetailDto> AddPhotosAsync(Guid mascotaId, IEnumerable<CreatePhotoDto> fotosDto, Guid usuarioId)
    {
        var mascota = await _mascotaRepository.GetByIdAsync(mascotaId, includeFotos: true);
        
        if (mascota == null || mascota.Tipo != TipoMascota.DeUsuario || mascota.PropietarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para agregar fotos a esta mascota");

        int ultimoOrden = mascota.Fotos?.Count > 0 ? mascota.Fotos.Max(f => f.Orden) : 0;
        bool yaHayPrincipal = mascota.Fotos?.Any(f => f.EsPrincipal) ?? false;

        var nuevasFotos = new List<MascotaFoto>();
        var index = 0;
        
        foreach (var f in fotosDto)
        {
            var incoming = f.StorageKey ?? string.Empty;
            if (string.IsNullOrWhiteSpace(incoming))
                throw new InvalidOperationException("Contenido de la imagen inválido.");

            bool isLikelyUrl = incoming.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                               || incoming.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase)
                               || incoming.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                               || incoming.Length < 200;

            string storageValue;
            if (isLikelyUrl)
            {
                storageValue = incoming;
            }
            else
            {
                storageValue = await SaveBase64ImageToLocalAsync(incoming, mascotaId);
            }

            nuevasFotos.Add(new MascotaFoto
            {
                Id = Guid.NewGuid(),
                MascotaId = mascotaId,
                StorageKey = storageValue,
                MimeType = f.MimeType ?? string.Empty,
                Orden = ultimoOrden + index + 1,
                EsPrincipal = !yaHayPrincipal && index == 0,
                UploadedAt = f.UploadedAt == default ? DateTime.UtcNow : f.UploadedAt,
            });

            index++;
        }

        await _mascotaRepository.AddPhotoAsync(nuevasFotos);
        await _mascotaRepository.SaveChangesAsync();

        var mascotaActualizada = await _mascotaRepository.GetByIdAsync(mascotaId, includeFotos: true);
        return MapToDetailDto(mascotaActualizada!);
    }

    public async Task<string> DeletePhotoAsync(Guid fotoId, Guid usuarioId)
    {
        var foto = await _mascotaRepository.DeletePhotoAsync(fotoId);
        if (foto == null)
            throw new InvalidOperationException("La foto no existe");

        // Verificar que la mascota pertenece al usuario
        var mascota = await _mascotaRepository.GetByIdAsync(foto.MascotaId);
        if (mascota == null || mascota.Tipo != TipoMascota.DeUsuario || mascota.PropietarioId != usuarioId)
            throw new UnauthorizedAccessException("No tienes permiso para eliminar esta foto");

        // Intentar eliminar el archivo físico
        if (!string.IsNullOrEmpty(foto.StorageKey))
        {
            try
            {
                string? relativePath = null;

                if (foto.StorageKey.StartsWith("/"))
                    relativePath = foto.StorageKey.TrimStart('/');
                else if (foto.StorageKey.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                    relativePath = foto.StorageKey;

                if (relativePath == null && Uri.TryCreate(foto.StorageKey, UriKind.Absolute, out var uri) && uri.AbsolutePath.Contains("/uploads/"))
                {
                    relativePath = uri.AbsolutePath.TrimStart('/');
                }

                if (!string.IsNullOrEmpty(relativePath))
                {
                    var localPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", localPath);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            catch
            {
                // Continuar aunque falle la eliminación del archivo
            }
        }

        await _mascotaRepository.SaveChangesAsync();
        return "Foto eliminada correctamente";
    }

    private static async Task<string> SaveBase64ImageToLocalAsync(string base64OrDataUri, Guid mascotaId, int maxWidth = 1600, int jpegQuality = 75)
    {
        var base64 = base64OrDataUri;
        var comma = base64.IndexOf(',');
        if (comma >= 0) base64 = base64[(comma + 1)..];

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Base64 inválido.");
        }

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "mascotas-usuario", mascotaId.ToString());
        Directory.CreateDirectory(uploadsDir);

        using var inMs = new MemoryStream(bytes);
        using Image image = await Image.LoadAsync(inMs);

        if (image.Width > maxWidth)
        {
            var ratio = maxWidth / (double)image.Width;
            var newWidth = maxWidth;
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(newWidth, newHeight), Mode = ResizeMode.Max }));
        }

        var fileName = $"{Guid.NewGuid()}.jpg";
        var fullPath = Path.Combine(uploadsDir, fileName);

        var encoder = new JpegEncoder { Quality = jpegQuality };
        await image.SaveAsJpegAsync(fullPath, encoder);

        return $"/uploads/mascotas-usuario/{mascotaId}/{fileName}";
    }

    private static MascotaUsuarioDetailDto MapToDetailDto(Mascota mascota)
    {
        return new MascotaUsuarioDetailDto
        {
            Id = mascota.Id,
            Nombre = mascota.Nombre,
            Especie = mascota.Especie,
            Raza = mascota.Raza,
            FechaNacimiento = mascota.FechaNacimiento,
            Sexo = mascota.Sexo,
            Personalidad = mascota.Personalidad,
            EstadoSalud = mascota.EstadoSalud,
            Notas = mascota.Notas,
            PropietarioId = mascota.PropietarioId!.Value,
            EdadEnAnios = mascota.FechaNacimiento.HasValue
                ? (int)((DateTime.Now - mascota.FechaNacimiento.Value).TotalDays / 365.25)
                : null,
            Fotos = mascota.Fotos?.Select(f => new AddMascotaFotoDto
            {
                StorageKey = f.StorageKey,
                MimeType = f.MimeType,
                Orden = f.Orden,
                EsPrincipal = f.EsPrincipal
            }).OrderBy(f => f.Orden).ToList() ?? new List<AddMascotaFotoDto>(),
            CreatedAt = mascota.CreatedAt,
            UpdatedAt = mascota.UpdatedAt
        };
    }
}
