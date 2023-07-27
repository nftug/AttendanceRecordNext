namespace Domain.Events;

public class DomainEventPublisher
{
    private readonly List<IEvent> _events = new();
    private readonly List<IEventSubscriber> _subscribers = new();

    public DomainEventPublisher Subscribe(IEventSubscriber subscriber)
    {
        if (_subscribers.Any(x => x == subscriber))
            return this;

        _subscribers.Add(subscriber);
        return this;
    }

    public DomainEventPublisher Unsubscribe(IEventSubscriber subscriber)
    {
        if (!_subscribers.Any(x => x == subscriber))
            return this;

        _subscribers.Remove(subscriber);
        return this;
    }

    public DomainEventPublisher Publish(IEvent @event)
    {
        _events.Add(@event);
        return this;
    }

    public async Task DispatchAsync()
    {
        foreach (var @event in _events)
        {
            foreach (var subscriber in _subscribers)
            {
                await subscriber.HandleAsync(@event);
            }
        }

        _events.Clear();
    }
}
