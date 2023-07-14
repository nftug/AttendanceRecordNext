using System.Windows;

namespace Presentation.Helpers;

public class DialogHelper : IDialogHelper
{
    public DialogResult ShowDialog(string message, string caption, DialogButton button, DialogImage image)
    {
        var result = ModernWpf.MessageBox.Show(message, caption, (MessageBoxButton)button, (MessageBoxImage)image);
        return result != null ? (DialogResult)result : DialogResult.None;
    }
}
