using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly WorkTimeModel _workTimeModel;
    private readonly NavigationModel _navigationModel;

    public ReadOnlyReactivePropertySlim<string?> HeaderTitle { get; }
    public ReadOnlyReactivePropertySlim<string?> WindowTitle { get; }

    public MainWindowViewModel(WorkTimeModel workTimeModel, IDialogHelper dialogHelper, NavigationModel navigationModel)
        : base(dialogHelper)
    {
        _workTimeModel = workTimeModel;
        _navigationModel = navigationModel;

        HeaderTitle = _navigationModel.HeaderTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        WindowTitle = _navigationModel.WindowTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        LoadedCommand
            .Subscribe(async _ => await CatchErrorAsync(_workTimeModel.LoadDataAsync))
            .AddTo(Disposable);
    }
}
