using AdoPetsBKD.Domain.Entities.Inventario;


namespace AdoPetsBKD.Application.Interfaces.Repositories;


public interface ICompraRepository
{
    Task AddAsync(Compra compra);
   
        Task UpdateAsync(Compra compra);
        Task<Compra?> GetByIdAsync(Guid id);
    

}