using Domain.Entities;

namespace Infrastructure.Shared;

public interface IDataModel<TEntity, TSelf>
    where TEntity : class, IEntity
    where TSelf : IDataModel<TEntity, TSelf>
{
    TEntity ToEntity();
    TSelf Transfer(TEntity entity);
}
