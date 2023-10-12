using System.Windows;

namespace Presentation.Helpers;

public enum DialogButton
{
    OK = MessageBoxButton.OK,
    OKCancel = MessageBoxButton.OKCancel,
    YesNo = MessageBoxButton.YesNo,
    YesNoCancel = MessageBoxButton.YesNoCancel
}

public enum DialogImage
{
    None = MessageBoxImage.None,
    Information = MessageBoxImage.Information,
    Question = MessageBoxImage.Question,
    Warning = MessageBoxImage.Warning,
    Error = MessageBoxImage.Error
}

public enum DialogResult
{
    None = MessageBoxResult.None,
    OK = MessageBoxResult.OK,
    Cancel = MessageBoxResult.Cancel,
    Yes = MessageBoxResult.Yes,
    No = MessageBoxResult.No,
    Primary,
    Secondary
}

public interface IDialogHelper
{
    Task<DialogResult> ShowDialogAsync(
        string message,
        string caption,
        DialogButton button = DialogButton.OK,
        DialogImage image = DialogImage.None
    );

    Task<DialogResult> ShowCustomDialogAsync(
        string message,
        string caption,
        string primaryButtonText,
        string? secondaryButtonText = null,
        string? closeButtonText = null,
        DialogResult defaultResult = DialogResult.None
    );
}
