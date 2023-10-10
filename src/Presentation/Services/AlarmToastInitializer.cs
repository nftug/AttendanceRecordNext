using Microsoft.Toolkit.Uwp.Notifications;
using Presentation.Models;

namespace Presentation.Services;

public class AlarmToastInitializer
{
    private readonly IReadOnlyDictionary<string, IAlarmModel> _alarms;

    public AlarmToastInitializer(WorkTimeAlarmModel workTimeAlarmModel, RestTimeAlarmModel restTimeAlarmModel)
    {
        _alarms = new Dictionary<string, IAlarmModel>()
        {
            { typeof(WorkTimeAlarmModel).Name, workTimeAlarmModel },
            { typeof(RestTimeAlarmModel).Name,  restTimeAlarmModel}
        };
    }

    public void Init()
    {
        // Listen to notification activation
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

            // Need to dispatch to UI thread if performing UI operations
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (!args.TryGetValue("alarmType", out string alarmType)) return;
                if (!_alarms.TryGetValue(alarmType, out var alarmModel)) return;

                if (!args.TryGetValue("action", out var action)) return;

                switch (action)
                {
                    case "snooze":
                        alarmModel.InvokeSnooze();
                        break;
                }
            });
        };
    }
}
