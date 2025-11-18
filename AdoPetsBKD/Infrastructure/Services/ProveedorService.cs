using AdoPetsBKD.Application.DTOs.Proveedores;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Inventario;

namespace AdoPetsBKD.Infrastructure.Services
{
    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _proveedorRepository;

        public ProveedorService(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        // 🟢 Crear proveedor
        public async Task<ProveedorDto> CreateProveedorAsync(CreateProveedorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Email))
                throw new ArgumentException("Nombre y Email son obligatorios.");

            var proveedor = new Proveedor
            {
                Id = Guid.NewGuid(),
                Nombre = dto.Nombre,
                Email = dto.Email,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                RFC = dto.RFC,
                Contacto = dto.Contacto,
                Notas = dto.Notas,
                Estatus = EstatusProveedor.Activo, // ✅ Por defecto Activo
                CreatedAt = DateTime.UtcNow
            };

            await _proveedorRepository.CreateAsync(proveedor);
            await _proveedorRepository.SaveChangesAsync();

            return MapToProveedorDto(proveedor);
        }

        // 🔵 Obtener todos los proveedores
        public async Task<List<ProveedorDto>> GetAllProveedoresAsync()
        {
            var proveedores = await _proveedorRepository.GetAllAsync();
            return proveedores.Select(MapToProveedorDto).ToList();
        }

        // 🔵 Obtener proveedor por ID
        public async Task<ProveedorDto?> GetProveedorByIdAsync(Guid id)
        {
            var proveedor = await _proveedorRepository.GetByIdAsync(id);
            return proveedor == null ? null : MapToProveedorDto(proveedor);
        }

        // 🟠 Actualizar proveedor
        public async Task<ProveedorDto> UpdateProveedorAsync(Guid id, UpdateProveedorDto dto)
        {
            var proveedor = await _proveedorRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Proveedor con ID {id} no encontrado.");

            proveedor.Nombre = dto.Nombre ?? proveedor.Nombre;
            proveedor.Email = dto.Email ?? proveedor.Email;
            proveedor.Telefono = dto.Telefono ?? proveedor.Telefono;
            proveedor.Direccion = dto.Direccion ?? proveedor.Direccion;
            proveedor.RFC = dto.RFC ?? proveedor.RFC;
            proveedor.Contacto = dto.Contacto ?? proveedor.Contacto;
            proveedor.Notas = dto.Notas ?? proveedor.Notas;

            if (dto.Estatus.HasValue)
                proveedor.Estatus = (EstatusProveedor)dto.Estatus.Value; // ✅ Conversión explícita

            proveedor.UpdatedAt = DateTime.UtcNow;

            await _proveedorRepository.UpdateAsync(proveedor);
            await _proveedorRepository.SaveChangesAsync();

            return MapToProveedorDto(proveedor);
        }

        // 🔴 Desactivar proveedor (cambia estatus a Inactivo)
        public async Task DesactivarProveedorAsync(Guid id)
        {
            var proveedor = await _proveedorRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Proveedor con ID {id} no encontrado.");

            proveedor.Estatus = EstatusProveedor.Inactivo; // ✅ Enum directo
            proveedor.UpdatedAt = DateTime.UtcNow;

            await _proveedorRepository.UpdateAsync(proveedor);
            await _proveedorRepository.SaveChangesAsync();
        }

        // 🟣 Cambiar estatus de proveedor (Activo, Inactivo, Bloqueado)
        public async Task<ProveedorDto> CambiarEstatusProveedorAsync(Guid id, int nuevoEstatus)
        {
            if (nuevoEstatus < 1 || nuevoEstatus > 3)
                throw new ArgumentException("Estatus inválido. Debe ser 1 (Activo), 2 (Inactivo) o 3 (Bloqueado).");

            var proveedor = await _proveedorRepository.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Proveedor con ID {id} no encontrado.");

            proveedor.Estatus = (EstatusProveedor)nuevoEstatus; // ✅ Conversión explícita
            proveedor.UpdatedAt = DateTime.UtcNow;

            await _proveedorRepository.UpdateAsync(proveedor);
            await _proveedorRepository.SaveChangesAsync();

            return MapToProveedorDto(proveedor);
        }

        // --- Mapeo Interno ---
        private static ProveedorDto MapToProveedorDto(Proveedor proveedor)
        {
            return new ProveedorDto
            {
                Id = proveedor.Id,
                Nombre = proveedor.Nombre,
                Email = proveedor.Email,
                Telefono = proveedor.Telefono,
                Direccion = proveedor.Direccion,
                Estatus = (int)proveedor.Estatus, // ✅ Enum → int
                RFC = proveedor.RFC,
                Contacto = proveedor.Contacto,
                Notas = proveedor.Notas,
                CreatedAt = proveedor.CreatedAt,
                UpdatedAt = proveedor.UpdatedAt
            };
        }
    }
}
