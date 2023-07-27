namespace Domain.Events;

public interface IEventSubscriber
{
    Task HandleAsync(IEvent @event);
}
