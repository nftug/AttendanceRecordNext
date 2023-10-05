using Presentation.Helpers;
using Presentation.Models;
using Presentation.Services;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly MainWindowModel _model;
    private readonly InitAppService _init;

    public ReadOnlyReactivePropertySlim<string?> HeaderTitle { get; }
    public ReadOnlyReactivePropertySlim<string?> WindowTitle { get; }

    public MainWindowViewModel(IDialogHelper dialogHelper, MainWindowModel model, InitAppService init)
        : base(dialogHelper)
    {
        _model = model;
        _init = init;

        HeaderTitle = _model.HeaderTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        WindowTitle = _model.WindowTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        LoadedCommand
            .Subscribe(async _ => await CatchErrorAsync(_init.InitAppAsync))
            .AddTo(Disposable);
    }
}
