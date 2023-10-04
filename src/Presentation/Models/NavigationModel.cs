using Infrastructure.Shared;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class NavigationModel : BindableBase
{
    private readonly IAppConfig _appConfig;

    public ReactivePropertySlim<string?> HeaderTitle { get; }
    public ReactivePropertySlim<string?> WindowTitle { get; }
    public string AppName { get; }

    public NavigationModel(IAppConfig appConfig)
    {
        _appConfig = appConfig;
        AppName = _appConfig.AppName;

        HeaderTitle = new ReactivePropertySlim<string?>().AddTo(Disposable);
        WindowTitle = new ReactivePropertySlim<string?>(_appConfig.AppName).AddTo(Disposable);
    }
}
