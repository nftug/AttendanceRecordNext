using Infrastructure.Shared;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class MainWindowModel : BindableBase
{
    private readonly IAppInfo _appConfig;

    public ReactivePropertySlim<string?> HeaderTitle { get; }
    public ReactivePropertySlim<string?> WindowTitle { get; }
    public string AppName { get; }

    public MainWindowModel(IAppInfo appConfig)
    {
        _appConfig = appConfig;
        AppName = _appConfig.AppName;

        HeaderTitle = new ReactivePropertySlim<string?>().AddTo(Disposable);
        WindowTitle = new ReactivePropertySlim<string?>(_appConfig.AppName).AddTo(Disposable);
    }
}
