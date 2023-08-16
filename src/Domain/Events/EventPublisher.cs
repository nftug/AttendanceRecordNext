namespace Domain.Events;

public class EventPublisher
{
    private readonly List<IEvent> _events = new();
    private readonly List<IEventSubscriber> _subscribers = new();

    public EventPublisher Subscribe(IEventSubscriber subscriber)
    {
        if (_subscribers.Any(x => x == subscriber))
            return this;

        _subscribers.Add(subscriber);
        return this;
    }

    public EventPublisher Subscribe(params IEventSubscriber[] subscribers)
    {
        foreach (var subscriber in subscribers)
            Subscribe(subscriber);
        return this;
    }

    public EventPublisher Unsubscribe(IEventSubscriber subscriber)
    {
        if (!_subscribers.Any(x => x == subscriber))
            return this;

        _subscribers.Remove(subscriber);
        return this;
    }

    public EventPublisher Unsubscribe(params IEventSubscriber[] subscribers)
    {
        foreach (var subscriber in subscribers)
            Unsubscribe(subscriber);
        return this;
    }

    public EventPublisher Publish(IEvent @event)
    {
        _events.Add(@event);
        return this;
    }

    public async Task CommitAsync()
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
