using Prism.Ioc;
using Prism.Services.Dialogs;

namespace Presentation.Services;

public class CustomDialogService : DialogService, ICustomDialogService
{
    public CustomDialogService(IContainerExtension containerExtension) : base(containerExtension)
    {
    }

    // Reference: https://stackoverflow.com/a/64425931

    public void ShowOrphan(string name, IDialogParameters? parameters, Action<IDialogResult>? callback, string? windowName)
    {
        parameters ??= new DialogParameters();
        var dialogWindow = CreateDialogWindow(windowName);
        ConfigureDialogWindowEvents(dialogWindow, callback);
        ConfigureDialogWindowContent(name, dialogWindow, parameters);

        dialogWindow.Owner = null;
        dialogWindow.Show();
    }

    // Reference: https://miri2370.hatenablog.com/entry/2021/10/11/204251

    public Task<ButtonResult> ShowDialogAsync(string name, IDialogParameters? parameters, string? windowName)
        => WaitForButtonResultAsync(tcs => ShowDialog(name, parameters, r => tcs.SetResult(r.Result), windowName));

    public Task<ButtonResult> ShowAsync(string name, IDialogParameters? parameters, string? windowName)
        => WaitForButtonResultAsync(tcs => Show(name, parameters, r => tcs.SetResult(r.Result), windowName));

    public Task<ButtonResult> ShowOrphanAsync(string name, IDialogParameters? parameters, string? windowName)
        => WaitForButtonResultAsync(tcs => ShowOrphan(name, parameters, r => tcs.SetResult(r.Result), windowName));

    private static Task<ButtonResult> WaitForButtonResultAsync(Action<TaskCompletionSource<ButtonResult>> callback)
    {
        var tcs = new TaskCompletionSource<ButtonResult>();
        callback(tcs);
        return tcs.Task;
    }
}

public interface ICustomDialogService : IDialogService
{
    void ShowOrphan(string name, IDialogParameters? parameters = null, Action<IDialogResult>? callback = null, string? windowName = null);
    Task<ButtonResult> ShowDialogAsync(string name, IDialogParameters? parameters = null, string? windowName = null);
    Task<ButtonResult> ShowAsync(string name, IDialogParameters? parameters = null, string? windowName = null);
    Task<ButtonResult> ShowOrphanAsync(string name, IDialogParameters? parameters = null, string? windowName = null);
}