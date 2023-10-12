using Presentation.Helpers;

namespace Presentation.Shared;

public abstract class ViewModelBase : BindableBase
{
    protected readonly IDialogHelper _dialogHelper;

    protected ViewModelBase(IDialogHelper dialogHelper)
    {
        _dialogHelper = dialogHelper;
    }

    protected async Task<bool> CatchErrorAsync(Func<Task> func)
    {
        try
        {
            await func();
            return true;
        }
        catch (Exception e)
        {
            await _dialogHelper.ShowDialogAsync(e.Message, "エラー");
            return false;
        }
    }
}
