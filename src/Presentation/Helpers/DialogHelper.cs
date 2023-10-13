using ModernWpf.Controls;

namespace Presentation.Helpers;

public class DialogHelper : IDialogHelper
{
    public async Task<DialogResult> ShowDialogAsync(string message, string caption, DialogButton button, DialogImage image)
    {
        var dialog = new ContentDialog
        {
            Title = caption,
            Content = message,
            PrimaryButtonText = button switch
            {
                DialogButton.OK or DialogButton.OKCancel => "OK",
                DialogButton.YesNo or DialogButton.YesNoCancel => "はい",
                _ => string.Empty
            },
            SecondaryButtonText = button switch
            {
                DialogButton.YesNo or DialogButton.YesNoCancel => "いいえ",
                _ => string.Empty
            },
            CloseButtonText = button switch
            {
                DialogButton.OKCancel or DialogButton.YesNoCancel => "キャンセル",
                _ => string.Empty
            }
        };

        var result = await CreateDialogAsync(dialog);

        return result switch
        {
            ContentDialogResult.Primary => button switch
            {
                DialogButton.YesNo or DialogButton.YesNoCancel => DialogResult.Yes,
                _ => DialogResult.OK
            },
            ContentDialogResult.Secondary => DialogResult.No,
            _ => DialogResult.Cancel
        };
    }

    public async Task<DialogResult> ShowCustomDialogAsync(
        string message,
        string caption,
        string primaryButtonText,
        string? secondaryButtonText = null,
        string? closeButtonText = null,
        DialogResult defaultResult = DialogResult.None
    )
    {
        var dialog = new ContentDialog
        {
            Title = caption,
            Content = message,
            PrimaryButtonText = primaryButtonText,
            SecondaryButtonText = secondaryButtonText,
            CloseButtonText = closeButtonText,
            DefaultButton = defaultResult switch
            {
                DialogResult.Primary => ContentDialogButton.Primary,
                DialogResult.Secondary => ContentDialogButton.Secondary,
                _ => ContentDialogButton.None
            }
        };

        var result = await CreateDialogAsync(dialog);

        return result switch
        {
            ContentDialogResult.Primary => DialogResult.Primary,
            ContentDialogResult.Secondary => DialogResult.Secondary,
            _ => DialogResult.Cancel
        };
    }

    // Reference: https://stackoverflow.com/questions/33018346/only-a-single-contentdialog-can-be-open-at-any-time-error-while-opening-anoth
    private static async Task<ContentDialogResult> CreateDialogAsync(ContentDialog dialog)
    {
        if (ActiveDialog != null)
        {
            await DialogAwaiter.Task;
            DialogAwaiter = new TaskCompletionSource<bool>();
        }

        ActiveDialog = dialog;
        ActiveDialog.Closed += ActiveDialog_Closed;
        var result = await ActiveDialog.ShowAsync();
        ActiveDialog.Closed -= ActiveDialog_Closed;

        return result;
    }

    public static ContentDialog? ActiveDialog;
    private static TaskCompletionSource<bool> DialogAwaiter = new();
    private static void ActiveDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args) =>
        DialogAwaiter.SetResult(true);
}