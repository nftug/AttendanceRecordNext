using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly WorkTimeModel _workTimeModel;
    private readonly AppConfigModel _appConfigModel;
    private readonly NavigationModel _navigationModel;

    public ReadOnlyReactivePropertySlim<string?> Title { get; }
    public ReadOnlyReactivePropertySlim<string?> HeaderTitle { get; }

    public MainWindowViewModel(WorkTimeModel workTimeModel, IDialogHelper dialogHelper, AppConfigModel appConfigModel, NavigationModel navigationModel)
        : base(dialogHelper)
    {
        _workTimeModel = workTimeModel;
        _appConfigModel = appConfigModel;
        _navigationModel = navigationModel;

        HeaderTitle = _navigationModel.HeaderTitle.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        Title = _appConfigModel.Title.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        LoadedCommand
            .Subscribe(async _ => await CatchErrorAsync(_workTimeModel.LoadDataAsync))
            .AddTo(Disposable);
    }
}
