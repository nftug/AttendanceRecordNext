using Microsoft.Toolkit.Uwp.Notifications;
using Presentation.Models;

namespace Presentation.Helpers;

public class ToastHelper : IToastHelper
{
    public void ShowAlarmToast(string title, string content) =>
        new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Alarm)
            .AddText(title)
            .AddText(content)
            .AddButton(new ToastButtonDismiss())
            .Show();

    public void ShowAlarmToastWithSnooze<T>(string title, string content)
        where T : IAlarmModel
        => new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Alarm)
            .AddText(title)
            .AddText(content)
            .AddButton(new ToastButtonDismiss())
            .AddButton(new ToastButton()
                .SetContent("スヌーズ")
                .AddArgument("action", "snooze")
                .AddArgument("alarmType", typeof(T).Name)
            )
            .Show();
}
