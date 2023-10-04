using Presentation.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Shared;

public abstract class ContentDialogViewModelBase : ViewModelBase
{
    public ReactivePropertySlim<string?> Title { get; }
    public ReactivePropertySlim<string?> Message { get; }

    protected ContentDialogViewModelBase(IDialogHelper dialogHelper) : base(dialogHelper)
    {
        Title = new ReactivePropertySlim<string?>().AddTo(Disposable);
        Message = new ReactivePropertySlim<string?>().AddTo(Disposable);
    }
}
