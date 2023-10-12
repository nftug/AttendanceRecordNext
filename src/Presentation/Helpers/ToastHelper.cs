using Microsoft.Toolkit.Uwp.Notifications;
using System.Text.Json;

namespace Presentation.Helpers;

public class ToastHelper : IToastHelper
{
    public void ShowToast(
        string title,
        string content,
        ToastType type = ToastType.Default,
        bool enableDismiss = false,
        params ToastAction[] toastActions
    )
    {
        ToastScenario scenario = type switch
        {
            ToastType.Default => ToastScenario.Default,
            ToastType.Reminder => ToastScenario.Reminder,
            ToastType.Alarm => ToastScenario.Alarm,
            _ => ToastScenario.Default
        };

        var toast = new ToastContentBuilder()
            .SetToastScenario(scenario)
            .AddText(title)
            .AddText(content);

        if (enableDismiss)
            toast.AddButton(new ToastButtonDismiss("閉じる"));

        foreach (var toastAction in toastActions)
        {
            var messageJson = JsonSerializer.Serialize(toastAction.Message);

            toast.AddButton(new ToastButton()
                .SetContent(toastAction.Caption)
                .AddArgument("Message", messageJson)
            );
        }

        toast.Show();
    }
}
