using Microsoft.Toolkit.Uwp.Notifications;
using Presentation.Enums;
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

    public void ShowAlarmToastWithSnooze<T>(string title, string content, string actionLabel)
        where T : IAlarmModel
        => new ToastContentBuilder()
            .SetToastScenario(ToastScenario.Alarm)
            .AddText(title)
            .AddText(content)
            .AddButton(new ToastButtonDismiss())
            .AddButton(new ToastButton()
                .SetContent(actionLabel)
                .AddArgument(ToastArgumentParameter.Action.ToString(), ToastArgumentAction.Act.ToString()))
                .AddArgument(ToastArgumentParameter.Sender.ToString(), typeof(T).Name)
            .AddButton(new ToastButton()
                .SetContent("スヌーズ")
                .AddArgument(ToastArgumentParameter.Action.ToString(), ToastArgumentAction.Snooze.ToString())
                .AddArgument(ToastArgumentParameter.Sender.ToString(), typeof(T).Name)
            )
            .Show();
}
