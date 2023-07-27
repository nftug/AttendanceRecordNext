using Domain.Entities;

namespace Infrastructure.Shared;

public interface IDataModel<TEntity, TSelf>
    where TEntity : class, IEntity<TEntity>
    where TSelf : IDataModel<TEntity, TSelf>
{
    Guid Id { get; set; }
    TEntity ToEntity();
    TSelf Transfer(TEntity entity);
}
