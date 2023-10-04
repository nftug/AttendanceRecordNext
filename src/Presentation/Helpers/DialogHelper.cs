using ModernWpf.Controls;

namespace Presentation.Helpers;

public class DialogHelper : IDialogHelper
{
    public async Task<DialogResult> ShowDialogAsync(string message, string caption, DialogButton button, DialogImage image)
    {
        var dialog = new ContentDialog()
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

        var result = await dialog.ShowAsync();

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
}