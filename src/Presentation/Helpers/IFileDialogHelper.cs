namespace Presentation.Helpers;

public enum FileDialogType
{
    Open,
    Save
}

public interface IFileDialogHelper
{
    string? OpenFileDialog(FileDialogType dialogType, string filer, string? title = null);
}
