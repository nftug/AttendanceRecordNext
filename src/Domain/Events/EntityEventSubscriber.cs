using Domain.Entities;
using Domain.Interfaces;

namespace Domain.Events;

public class EntityEventSubscriber<T> : IEventSubscriber
    where T : IEntity<T>
{
    private readonly IRepository<T> _repository;

    public EntityEventSubscriber(IRepository<T> repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(IEvent value)
    {
        if (value is not EntityEvent<T> @event) return;

        switch (@event.Type)
        {
            case EntityEventType.Added:
                await _repository.CreateAsync(@event.Entity);
                break;
            case EntityEventType.Updated:
                await _repository.UpdateAsync(@event.Entity);
                break;
            case EntityEventType.Deleted:
                await _repository.DeleteAsync(@event.Entity.Id);
                break;
        }
    }
}
