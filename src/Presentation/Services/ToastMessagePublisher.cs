using Microsoft.Toolkit.Uwp.Notifications;
using Presentation.Helpers;
using System.Text.Json;
using Application = System.Windows.Application;

namespace Presentation.Services;

public class ToastMessagePublisher
{
    private readonly HashSet<KeyValuePair<string, IToastMessageSubscriber>> _subscribers = new();

    public ToastMessagePublisher()
    {
        // Listen to notification activation
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;
    }

    public ToastMessagePublisher Subscribe<T>(T subscriber) where T : IToastMessageSubscriber
    {
        _subscribers.Add(new(typeof(T).Name, subscriber));
        return this;
    }

    private void OnToastActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
    {
        // Obtain the arguments from the notification
        ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

        if (!args.TryGetValue("Message", out var messageJson)) return;
        var message = JsonSerializer.Deserialize<ToastMessage>(messageJson);
        if (message is null) return;

        foreach (var subscriber in _subscribers.Where(x => x.Key == message.Target))
        {
            Application.Current.Dispatcher.Invoke(() => subscriber.Value.HandleToastMessage(message.Message));
        }
    }
}

public interface IToastMessageSubscriber
{
    void HandleToastMessage(string message);
}