using Microsoft.Win32;

namespace Presentation.Helpers;

public class FileDialogHelper : IFileDialogHelper
{
    public string? OpenFileDialog(FileDialogType dialogType, string filer, string? title = null)
    {
        FileDialog fileDialog = dialogType switch
        {
            FileDialogType.Open => new OpenFileDialog(),
            FileDialogType.Save => new SaveFileDialog(),
            _ => new OpenFileDialog()
        };
        fileDialog.Filter = filer;
        fileDialog.Title = title;
        fileDialog.RestoreDirectory = true;

        bool result = fileDialog.ShowDialog() ?? false;
        return !result ? null : fileDialog.FileName;
    }
}