namespace Presentation.Helpers;

public interface IToastHelper
{
    void ShowToast(
        string title,
        string content,
        ToastType type = ToastType.Default,
        bool enableDismiss = false,
        params ToastAction[] toastActions
   );
}

public record ToastMessage(string Target, string Message)
{
    public static ToastMessage Create(Type targetType, string message) =>
        new(targetType.Name, message);
}

public record ToastAction(ToastMessage Message, string Caption);

public enum ToastType
{
    Default,
    Reminder,
    Alarm
}