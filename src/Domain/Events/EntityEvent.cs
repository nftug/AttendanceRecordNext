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

    public static EntityEvent<TEntity> AddedRange(IEnumerable<TEntity> entities)
        => new()
        {
            Entities = entities.Select(x => x.Recreate()).ToList(),
            Type = EntityEventType.AddedRange
        };

    public static EntityEvent<TEntity> UpdatedRange(IEnumerable<TEntity> entities)
        => new()
        {
            Entities = entities.Select(x => x.Recreate()).ToList(),
            Type = EntityEventType.UpdatedRange
        };

    public static EntityEvent<TEntity> DeletedRange(IEnumerable<TEntity> entities)
        => new()
        {
            Entities = entities.Select(x => x.Recreate()).ToList(),
            Type = EntityEventType.DeletedRange
        };
}

public enum EntityEventType
{
    Added,
    Updated,
    Deleted,
    AddedRange,
    UpdatedRange,
    DeletedRange
}