using Domain.Entities;

namespace Domain.Events;

public record EntityEvent<TEntity>(TEntity Entity, EntityEventType Type) : IEvent
     where TEntity : IEntity<TEntity>
{
    public static EntityEvent<TEntity> Added(TEntity entity)
        => new(entity.Recreate(), EntityEventType.Added);

    public static EntityEvent<TEntity> Updated(TEntity entity)
        => new(entity.Recreate(), EntityEventType.Updated);

    public static EntityEvent<TEntity> Deleted(TEntity entity)
        => new(entity.Recreate(), EntityEventType.Deleted);
}

public enum EntityEventType
{
    Added,
    Updated,
    Deleted
}