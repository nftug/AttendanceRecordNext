using Microsoft.Toolkit.Uwp.Notifications;
using Presentation.Enums;
using Presentation.Models;

namespace Presentation.Services;

public class AlarmToastInitializer
{
    private readonly IReadOnlyDictionary<string, IAlarmModel> _alarms;
    private readonly MainWindowModel _mainWindowModel;

    public AlarmToastInitializer(
        WorkTimeAlarmModel workTimeAlarmModel,
        RestTimeAlarmModel restTimeAlarmModel,
        MainWindowModel mainWindowModel
    )
    {
        _alarms = new Dictionary<string, IAlarmModel>()
        {
            { typeof(WorkTimeAlarmModel).Name, workTimeAlarmModel },
            { typeof(RestTimeAlarmModel).Name,  restTimeAlarmModel}
        };

        _mainWindowModel = mainWindowModel;
    }

    public void Init()
    {
        // Listen to notification activation
        ToastNotificationManagerCompat.OnActivated += OnToastActivated;
    }

    private void OnToastActivated(ToastNotificationActivatedEventArgsCompat toastArgs)
    {
        // Obtain the arguments from the notification
        ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

        if (!args.TryGetValue(ToastArgumentParameter.Sender.ToString(), out var sender)) return;
        if (!_alarms.TryGetValue(sender, out var alarmModel)) return;

        if (!args.TryGetValue(ToastArgumentParameter.Action.ToString(), out var actionName)) return;
        if (!Enum.TryParse(actionName, out ToastArgumentAction action)) return;

        // Need to dispatch to UI thread if performing UI operations
        System.Windows.Application.Current.Dispatcher.Invoke(async () =>
        {
            switch (action)
            {
                case ToastArgumentAction.Snooze:
                    alarmModel.InvokeSnooze();
                    break;

                case ToastArgumentAction.Act:
                    if (_mainWindowModel.Visibility.Value == System.Windows.Visibility.Hidden)
                    {
                        // NOTE: ウィンドウが最小化されるのを防ぐため、Delayを設ける
                        await Task.Delay(1000);
                        _mainWindowModel.Reopen();
                    }

                    await alarmModel.DoActionAsync();
                    break;
            }
        });
    }
}
