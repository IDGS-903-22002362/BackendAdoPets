using AdoPetsBKD.Application.Common;
using AdoPetsBKD.Application.DTOs.Especialidades;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Application.Interfaces.Services;
using AdoPetsBKD.Domain.Entities.Servicios;
using System.Linq;

namespace AdoPetsBKD.Infrastructure.Services
{
    public class EspecialidadService : IEspecialidadService
    {
        private readonly IEspecialidadRepositoy _especialidadRepositoy;

        public EspecialidadService(IEspecialidadRepositoy especialidadRepositoy)
        {
            _especialidadRepositoy = especialidadRepositoy;
        }

        public async Task<PagedResponse<EspecialidadListDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var especialidades = await _especialidadRepositoy.GetAllAsync(pageNumber, pageSize);
            var totalCount = await _especialidadRepositoy.GetTotalCountAsync();

            var especialidadesDto = especialidades.Select(e => new EspecialidadListDto
            {
                Id = e.Id,
                Codigo = e.Codigo ?? string.Empty,
                Descripcion = e.Descripcion
            }).ToList();

            return new PagedResponse<EspecialidadListDto>
            {
                Items = especialidadesDto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<EspecialidadDetailDto?> GetByIdAsync(string codigo)
        {
            var especialidad = await _especialidadRepositoy.GetByIdAsync(codigo);

            if (especialidad == null)
            {
                return null;
            }

            return new EspecialidadDetailDto
            {
                Id = especialidad.Id,
                Codigo = especialidad.Codigo ?? string.Empty,
                Descripcion = especialidad.Descripcion
            };
        }

        public async Task<EspecialidadDetailDto> CreateAsync(CreateEspecialidadDto dto)
        {
            // Validar que el código no exista
            var especialidadExistente = await _especialidadRepositoy.GetByIdAsync(dto.Codigo);
            if (especialidadExistente != null)
            {
                throw new InvalidOperationException("Ya existe una especialidad con este código");
            }

            // Crear la especialidad
            var especialidad = new Especialidad
            {
                Codigo = dto.Codigo,
                Descripcion = dto.Descripcion
            };

            await _especialidadRepositoy.CreateAsync(especialidad);
            await _especialidadRepositoy.SaveChangesAsync();

            return (await GetByIdAsync(dto.Codigo))!;
        }
    }
}
