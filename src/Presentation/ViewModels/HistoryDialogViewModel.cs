using Presentation.Helpers;
using Presentation.Shared;
using Prism.Services.Dialogs;

namespace Presentation.ViewModels;

public class HistoryDialogViewModel : ViewModelBase, IDialogAware
{
    public HistoryDialogViewModel(IDialogHelper dialogHelper) : base(dialogHelper)
    {
    }

    public string Title => "履歴";

    public event Action<IDialogResult>? RequestClose;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }
}
