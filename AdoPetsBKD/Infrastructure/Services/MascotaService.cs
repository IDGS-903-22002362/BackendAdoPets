using AdoPetsBKD.Application.DTOs.Mascota;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Mascotas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

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
                    Id = f.Id,
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

        public async Task<IEnumerable<MascotaDetailDto>> GetAllAsync(FiltroMascotaDto? dto = null)
        {
            var mascotas = await _mascotaRepository.GetAllAsync();

            // Aplicar filtros si los hay
            if (dto != null)
            {
                if (!string.IsNullOrEmpty(dto.Nombre)) 
                    mascotas = mascotas.Where(m => m.Nombre.ToLower() == dto.Nombre.ToLower());

                if (!string.IsNullOrEmpty(dto.Especie))
                    mascotas = mascotas.Where(m => m.Especie.ToLower() == dto.Especie.ToLower());

                if (!string.IsNullOrEmpty(dto.Raza))
                    mascotas = mascotas.Where(m => m.Raza != null && m.Raza.ToLower() == dto.Raza.ToLower());

                if (dto.Sexo.HasValue)
                    mascotas = mascotas.Where(m => m.Sexo == dto.Sexo.Value);

                if (dto.Estatus.HasValue)
                    mascotas = mascotas.Where(m => m.Estatus == dto.Estatus.Value);

                if (dto.EdadEnAnios.HasValue)
                    mascotas = mascotas.Where(m => (m.EdadEnMeses / 12) == dto.EdadEnAnios.Value);

            }

            // Convertir a DTO
            return mascotas.Select(mascota => new MascotaDetailDto
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
                RequisitoAdopcion = mascota.RequisitoAdopcion,
                EdadEnAnio = mascota.FechaNacimiento.HasValue
         ? (int)((DateTime.Now - mascota.FechaNacimiento.Value).TotalDays / 365.25)
         : 0,
                Fotos = mascota.Fotos?.Select(f => new AddMascotaFotoDto
                {
                    StorageKey = f.StorageKey,
                    MimeType = f.MimeType,
                    Orden = f.Orden,
                    EsPrincipal = f.EsPrincipal
                }).ToList() ?? new List<AddMascotaFotoDto>(),
                CreatedAt = mascota.CreatedAt
            }).ToList();

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

            var mascotaActualizada = await _mascotaRepository.GetByIdAsync(mascotaId)
                ?? throw new InvalidOperationException("No se pudo recuperar la mascota después de agregar fotos.");

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
                Fotos = mascotaActualizada.Fotos?.Select(fi => new AddMascotaFotoDto
                {
                    StorageKey = fi.StorageKey,
                    MimeType = fi.MimeType,
                    Orden = fi.Orden,
                    EsPrincipal = fi.EsPrincipal
                }).OrderBy(fi => fi.Orden).ToList() ?? new List<AddMascotaFotoDto>(),
                CreatedAt = mascotaActualizada.CreatedAt,
                UpdatedAt = mascotaActualizada.UpdatedAt
            };
        }

        // Guarda base64/data-uri como archivo en disco y devuelve la ruta relativa (/uploads/mascotas/{id}/{file})
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
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "mascotas", mascotaId.ToString());
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

            var relative = $"/uploads/mascotas/{mascotaId}/{fileName}";
            return relative;
        }
        public async Task<string> DeletePhotoAsync(Guid fotoId)
        {
            var foto = await _mascotaRepository.DeletePhotoAsync(fotoId);
            if (foto == null)
                throw new InvalidOperationException("La foto no existe");

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
                catch (Exception ex)
                {
                   

                }
            }

            await _mascotaRepository.SaveChangesAsync();
            return "Foto eliminada correctamente de la base de datos y del almacenamiento";
        }


        // Metodos para la solicitud de adopcion
        public async Task<string> CrearSolicitudAdopcionAsync(CreateSolicitudeAdopcionDto dto)
        {
            // 1️⃣ Obtener mascota
            var mascota = await _mascotaRepository.GetByIdAsync(dto.MascotaId);

            if (mascota == null)
                throw new Exception("La mascota no existe.");

            // 2️⃣ Validar que esté disponible
            if (!mascota.EstaDisponibleParaAdopcion())
                throw new Exception("La mascota no está disponible para adopción.");

            // 3️⃣ Crear solicitud
            var solicitud = new SolicitudAdopcion
            {
                UsuarioId = dto.UsuarioId,
                MascotaId = dto.MascotaId,
                Vivienda = dto.Vivienda,
                NumNinios = dto.NumNinios,
                OtrasMascotas = dto.OtrasMascotas,
                HorasDisponibilidad = dto.HorasDisponibilidad,
                Direccion = dto.Direccion,
                IngresosMensuales = dto.IngresosMensuales,
                MotivoAdopcion = dto.MotivoAdopcion,
                FechaSolicitud = DateTime.UtcNow,
                Estado = EstadoSolicitudAdopcion.Pendiente,
                CreatedBy = dto.UsuarioId
            };

            await _mascotaRepository.CreateSolicitudAdopcionAsync(solicitud);
            await _mascotaRepository.SaveChangesAsync();

            return $"Solicitud de adopción creada con éxito. ID: {solicitud.Id}";
        }


        private string CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            int años = hoy.Year - fechaNacimiento.Year;

            if (fechaNacimiento.Date > hoy.AddYears(-años))
                años--;

            if (años < 1)
            {
                int meses = ((hoy.Year - fechaNacimiento.Year) * 12) + hoy.Month - fechaNacimiento.Month;
                if (fechaNacimiento.Day > hoy.Day) meses--;
                return $"{meses} meses";
            }

            return $"{años} años";
        }


        public async Task<IEnumerable<SolicitudeAdopcionDetailDto>> GetAllSolicitudesAdopcionAsync()
        {
            var solicitudes = await _mascotaRepository.GetAllSolicitudesAdopcionAsync();

            return solicitudes.Select(s => new SolicitudeAdopcionDetailDto
            {
                Id = s.Id,
                UsuarioId = s.UsuarioId,
                UsuarioNombre = s.Usuario?.Nombre ?? "N/A",
                Email = s.Usuario?.Email ?? "N/A",
                Telefono = s.Usuario?.Telefono ??  "N/A",


                MascotaId = s.MascotaId,
                MascotaNombre = s.Mascota?.Nombre ?? "N/A",

                Especie = s.Mascota?.Especie ?? "N/A",
                Raza = s.Mascota?.Raza ?? "N/A",
                Sexo = s.Mascota?.Sexo ?? 0,

                FechaNacimiento = s.Mascota?.FechaNacimiento ?? DateTime.MinValue,
                Edad = s.Mascota?.FechaNacimiento != null
                    ? CalcularEdad(s.Mascota.FechaNacimiento.Value)
                    : "N/A",

                Vivienda = s.Vivienda,
                NumNinios = s.NumNinios,
                OtrasMascotas = s.OtrasMascotas,
                HorasDisponibilidad = s.HorasDisponibilidad,
                Direccion = s.Direccion,
                IngresosMensuales = s.IngresosMensuales,
                MotivoAdopcion = s.MotivoAdopcion,
                Estado = s.Estado,
                FechaSolicitud = s.FechaSolicitud,
                FechaRevision = s.FechaRevision,
                FechaAprobacion = s.FechaAprobacion,
                MotivoRechazo = s.MotivoRechazo,

                MascotaFotos = s.Mascota?.Fotos?.Select(f => new AddMascotaFotoDto
                {
                    StorageKey = f.StorageKey,
                    MimeType = f.MimeType,
                    Orden = f.Orden,
                    EsPrincipal = f.EsPrincipal,
                }).ToList() ?? new List<AddMascotaFotoDto>()
            }).ToList();
        }

        public async Task<SolicitudeAdopcionDetailDto?> GetSolicitudAdopcionByIdAsync(Guid solicitudId)
        {
            var solicitud = await _mascotaRepository.GetSolicitudByIdAsync(solicitudId);
            if (solicitud == null) return null;
            return new SolicitudeAdopcionDetailDto
            {
                Id = solicitud.Id,
                UsuarioId = solicitud.UsuarioId,
                UsuarioNombre = solicitud.Usuario?.Nombre ?? "N/A",
                MascotaId = solicitud.MascotaId,
                MascotaNombre = solicitud.Mascota?.Nombre ?? "N/A",
                Vivienda = solicitud.Vivienda,
                NumNinios = solicitud.NumNinios,
                OtrasMascotas = solicitud.OtrasMascotas,
                HorasDisponibilidad = solicitud.HorasDisponibilidad,
                Direccion = solicitud.Direccion,
                IngresosMensuales = solicitud.IngresosMensuales,
                MotivoAdopcion = solicitud.MotivoAdopcion,
                Estado = solicitud.Estado,
                FechaSolicitud = solicitud.FechaSolicitud,
                FechaRevision = solicitud.FechaRevision,
                FechaAprobacion = solicitud.FechaAprobacion,
                MotivoRechazo = solicitud.MotivoRechazo,
                MascotaFotos = solicitud.Mascota?.Fotos?.Select(f => new AddMascotaFotoDto
                {
                    StorageKey = f.StorageKey,
                    MimeType = f.MimeType,
                    Orden = f.Orden,
                    EsPrincipal = f.EsPrincipal,
                }).ToList() ?? new List<AddMascotaFotoDto>()
            };
        }



        public async Task<CambiarEstadoSolicitudEnRevisionDto> UpdateStatusSolicitudAdopcionAsync(CambiarEstadoSolicitudEnRevisionDto dto)
        {
            var solicitud = await _mascotaRepository.GetSolicitudByIdAsync(dto.Id);
            if (solicitud == null)
                throw new InvalidOperationException("Solicitud no encontrada");

            var estadoAnterior = solicitud.Estado;

            // Crear log antes de cambiar el estado
            var log = new AdopcionLog
            {
                Id = Guid.NewGuid(),
                SolicitudId = solicitud.Id,
                FromEstado = estadoAnterior,
                ToEstado = dto.Estado,
                ChangedBy = dto.RevisadoPor,
                ChangedAt = DateTime.UtcNow
            };

            // Actualizar la solicitud
            solicitud.Estado = dto.Estado;
            solicitud.RevisadoPor = dto.RevisadoPor;
            solicitud.FechaRevision = DateTime.UtcNow;

            var mascota = await _mascotaRepository.GetByIdAsync(solicitud.MascotaId);
            if (mascota != null)
            {
                mascota.Estatus = EstatusMascota.Reservada;
                mascota.UpdatedAt = DateTime.UtcNow;

                await _mascotaRepository.UpdateAsync(mascota);
            }


            try
            {
                // Guardar cambios en la solicitud
                await _mascotaRepository.UpdateStatusSolicitudAdopcionAsync(solicitud);

                // Guardar el log
                await _mascotaRepository.AddAdopcionLogAsync(log);

                // Persistir todos los cambios
                await _mascotaRepository.SaveChangesAsync();

                return new CambiarEstadoSolicitudEnRevisionDto
                {
                    Id = solicitud.Id,
                    Estado = solicitud.Estado,
                    RevisadoPor = (Guid)solicitud.RevisadoPor,
                   
                };
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("La solicitud fue modificada por otro proceso. Vuelve a cargar y reintenta.");
            }
        }



        public async Task<EstadoSolicitudeAceptadaDto> UpdateStatusSolicitudAprobadaAsync(EstadoSolicitudeAceptadaDto dto)
        {
            var solicitud = await _mascotaRepository.GetSolicitudByIdAsync(dto.Id);
            if (solicitud == null)
                throw new InvalidOperationException("Solicitud no encontrada");

            var estadoAnterior = solicitud.Estado;

            var log = new AdopcionLog
            {
                Id = Guid.NewGuid(),
                SolicitudId = solicitud.Id,
                FromEstado = estadoAnterior,
                ToEstado = dto.Estado,
                ChangedAt = DateTime.UtcNow
            };

            solicitud.Estado = dto.Estado;
            solicitud.FechaAprobacion = DateTime.UtcNow;

            var mascota = await _mascotaRepository.GetByIdAsync(solicitud.MascotaId);
            if (mascota != null)
            {
                mascota.Estatus = EstatusMascota.Adoptada;
                mascota.UpdatedAt = DateTime.UtcNow;

                await _mascotaRepository.UpdateAsync(mascota);
            }

            try
            {
                await _mascotaRepository.UpdateStatusSolicitudAdopcionAsync(solicitud);

            
                await _mascotaRepository.AddAdopcionLogAsync(log);

                await _mascotaRepository.SaveChangesAsync();

                return new EstadoSolicitudeAceptadaDto
                {
                    Id = solicitud.Id,
                    Estado = solicitud.Estado                                                                    ,

                };
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("La solicitud fue modificada por otro proceso. Vuelve a cargar y reintenta.");
            }
        }


        public async Task<SolicirudRechasadaDto> UpdateStatusSolicitudRechazadaAsync( SolicirudRechasadaDto dto)
        {
            var solicitud = await _mascotaRepository.GetSolicitudByIdAsync(dto.Id);
            if (solicitud == null)
                throw new InvalidOperationException("Solicitud no encontrada");

            var estadoAnterior = solicitud.Estado;

            var log = new AdopcionLog
            {
                Id = Guid.NewGuid(),
                SolicitudId = solicitud.Id,
                FromEstado = estadoAnterior,
                ToEstado = dto.Estado,
                Reason = dto.MotivoRechazo,
                ChangedBy = dto.UsuarioId,
                ChangedAt = DateTime.UtcNow
            };

            solicitud.Estado = dto.Estado;
            solicitud.FechaAprobacion = DateTime.UtcNow;
            solicitud.MotivoRechazo = dto.MotivoRechazo;

            var mascota = await _mascotaRepository.GetByIdAsync(solicitud.MascotaId);
            if (mascota != null)
            {
                mascota.Estatus = EstatusMascota.Disponible;
                mascota.UpdatedAt = DateTime.UtcNow;

                await _mascotaRepository.UpdateAsync(mascota);
            }

            try
            {
                await _mascotaRepository.UpdateStatusSolicitudAdopcionAsync(solicitud);


                await _mascotaRepository.AddAdopcionLogAsync(log);

                await _mascotaRepository.SaveChangesAsync();

                return new SolicirudRechasadaDto
                {
                    Id = solicitud.Id,
                    Estado = solicitud.Estado,
                    MotivoRechazo = solicitud.MotivoRechazo

                };
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("La solicitud fue modificada por otro proceso. Vuelve a cargar y reintenta.");
            }
        }



        public async Task<CancelarSolicitudDto> UpdateStatusSolicitudCanceladaAsync(CancelarSolicitudDto dto)
        {
            var solicitud = await _mascotaRepository.GetSolicitudByIdAsync(dto.Id);
            if (solicitud == null)
                throw new InvalidOperationException("Solicitud no encontrada");

            var estadoAnterior = solicitud.Estado;

            var log = new AdopcionLog
            {
                Id = Guid.NewGuid(),
                SolicitudId = solicitud.Id,
                FromEstado = estadoAnterior,
                ToEstado = dto.Estado,
                ChangedBy = dto.UsuarioId,
                ChangedAt = DateTime.UtcNow
            };

            solicitud.Estado = dto.Estado;
            solicitud.FechaAprobacion = DateTime.UtcNow;



            try
            {
                await _mascotaRepository.UpdateStatusSolicitudAdopcionAsync(solicitud);


                await _mascotaRepository.AddAdopcionLogAsync(log);

                await _mascotaRepository.SaveChangesAsync();

                return new CancelarSolicitudDto
                {
                    Id = solicitud.Id,
                    Estado = solicitud.Estado,

                };
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException("La solicitud fue modificada por otro proceso. Vuelve a cargar y reintenta.");
            }
        }


        public async Task<IEnumerable<SolicitudeAdopcionDetailDto>> GetSolicitudbyUsuarioIdAsync(Guid usuarioId)
        {
            var solicitudes = await _mascotaRepository.GetSolicitudbyUsuarioIdAsync(usuarioId);

            return solicitudes.Select(s => new SolicitudeAdopcionDetailDto
            {
                Id = s.Id,
                UsuarioId = s.UsuarioId,
                UsuarioNombre = s.Usuario?.Nombre ?? "N/A",
                MascotaId = s.MascotaId,
                MascotaNombre = s.Mascota?.Nombre ?? "N/A",
                Vivienda = s.Vivienda,
                NumNinios = s.NumNinios,
                OtrasMascotas = s.OtrasMascotas,
                HorasDisponibilidad = s.HorasDisponibilidad,
                Direccion = s.Direccion,
                IngresosMensuales = s.IngresosMensuales,
                MotivoAdopcion = s.MotivoAdopcion,
                Estado = s.Estado,
                FechaSolicitud = s.FechaSolicitud,
                FechaRevision = s.FechaRevision,
                FechaAprobacion = s.FechaAprobacion,
                MotivoRechazo = s.MotivoRechazo,
                MascotaFotos = s.Mascota?.Fotos?
            .Select(f => new AddMascotaFotoDto
            {
                StorageKey = f.StorageKey,
                MimeType = f.MimeType,
                Orden = f.Orden,
                EsPrincipal = f.EsPrincipal,
            })
            .ToList() ?? new List<AddMascotaFotoDto>()
            }).ToList();
        }
        public async Task<AdopcionLogDto> AddAdopcionLogAsync(AdopcionLogDto dto)
        {
            var log = new AdopcionLog
            {
               
            };
            await _mascotaRepository.AddAdopcionLogAsync(log);
            await _mascotaRepository.SaveChangesAsync();
            dto.Id = log.Id;
            return dto;

        }
    }

}
