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

    public ReadOnlyReactivePropertySlim<string?> Title { get; }

    public MainWindowViewModel(WorkTimeModel workTimeModel, IDialogHelper dialogHelper, AppConfigModel appConfigModel)
        : base(dialogHelper)
    {
        _workTimeModel = workTimeModel;
        _appConfigModel = appConfigModel;

        LoadedCommand
            .Subscribe(async _ => await CatchErrorAsync(_workTimeModel.LoadDataAsync))
            .AddTo(Disposable);

        Title = _appConfigModel.Title.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
    }
}
