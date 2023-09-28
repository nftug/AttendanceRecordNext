using Infrastructure.Shared;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.Models;

public class AppConfigModel : BindableBase
{
    private readonly IAppConfig _appConfig;

    private readonly ReactivePropertySlim<string> _title;
    public ReadOnlyReactivePropertySlim<string?> Title { get; }

    private readonly ReactivePropertySlim<string> _version;
    public ReadOnlyReactivePropertySlim<string?> Version { get; }

    public AppConfigModel(IAppConfig appConfig)
    {
        _appConfig = appConfig;

        _title = new ReactivePropertySlim<string>(_appConfig.AppName).AddTo(Disposable);
        Title = _title.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        _version = new ReactivePropertySlim<string>(_appConfig.AppVersion).AddTo(Disposable);
        Version = _version.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
    }
}
