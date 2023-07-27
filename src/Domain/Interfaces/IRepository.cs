using Domain.Entities;

namespace Domain.Interfaces;

public interface IRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    Task CreateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid entity);
    Task<TEntity?> FindByIdAsync(Guid id);
}
