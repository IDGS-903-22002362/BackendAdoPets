using AdoPetsBKD.Application.DTOs;
using AdoPetsBKD.Application.Interfaces.Repositories;
using AdoPetsBKD.Domain.Entities.Servicios;
using AdoPetsBKD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdoPetsBKD.Infrastructure.Repositories
{
    public class EspecialidadRepository : IEspecialidadRepositoy
    {
        private readonly AdoPetsDbContext _context; 

        public EspecialidadRepository(AdoPetsDbContext context)
        {
            _context = context;
        }

        public async Task<Especialidad> GetByIdAsync (string codigo)
        {
            return await _context.Especialidades
                .FirstOrDefaultAsync(e => e.Codigo == codigo);
        }
        
        public async Task<Especialidad> CreateAsync (Especialidad especialidad)
        {
            _context.Especialidades.Add (especialidad);
            await _context.SaveChangesAsync();
            return especialidad;
        }

        public async Task<List<Especialidad>> GetAllAsync()
        {
            return await _context.Especialidades
                .OrderBy(e => e.Descripcion)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
