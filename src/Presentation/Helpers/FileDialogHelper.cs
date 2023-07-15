using FileDialog = Microsoft.Win32.FileDialog;

namespace Presentation.Helpers;

public class FileDialogHelper : IFileDialogHelper
{
    public string? OpenFileDialog(FileDialogType dialogType, string filer, string? title = null)
    {
        FileDialog fileDialog = dialogType switch
        {
            FileDialogType.Open => new Microsoft.Win32.OpenFileDialog(),
            FileDialogType.Save => new Microsoft.Win32.SaveFileDialog(),
            _ => new Microsoft.Win32.OpenFileDialog()
        };
        fileDialog.Filter = filer;
        fileDialog.Title = title;
        fileDialog.RestoreDirectory = true;

        bool result = fileDialog.ShowDialog() ?? false;
        return !result ? null : fileDialog.FileName;
    }
}