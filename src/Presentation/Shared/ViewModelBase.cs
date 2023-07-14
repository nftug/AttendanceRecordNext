using Presentation.Helpers;

namespace Presentation.Shared;

public abstract class ViewModelBase : BindableBase
{
    protected readonly IDialogHelper _dialogHelper;

    protected ViewModelBase(IDialogHelper dialogHelper)
    {
        _dialogHelper = dialogHelper;
    }

    protected async Task<bool> CatchErrorAsync(Func<Task> func, string? message = null)
    {
        try
        {
            await func();
            return true;
        }
        catch (Exception e)
        {
            message ??= "An error has occurred during process.";
            _dialogHelper.ShowDialog(
               $"{message}\nError message: {e.Message}",
               "Error",
               image: DialogImage.Error
           );
            return false;
        }
    }
}
