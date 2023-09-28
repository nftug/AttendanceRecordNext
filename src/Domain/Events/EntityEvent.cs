using Domain.Entities;

namespace Domain.Events;

public record EntityEvent<TEntity>(TEntity Entity, EntityEventType Type) : IEvent
     where TEntity : IEntity<TEntity>
{
    public static EntityEvent<TEntity> Saved(TEntity entity)
        => new(entity.Recreate(), EntityEventType.Saved);

    public static EntityEvent<TEntity> Deleted(TEntity entity)
        => new(entity.Recreate(), EntityEventType.Deleted);
}

public enum EntityEventType
{
    Saved,
    Deleted,
}