using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class NavigationModel : BindableBase
{
    public ReactivePropertySlim<string?> HeaderTitle { get; }

    public NavigationModel()
    {
        HeaderTitle = new ReactivePropertySlim<string?>().AddTo(Disposable);
    }
}
