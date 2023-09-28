using System.Reactive.Linq;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly WorkTimeModel _workTimeModel;
   
    public MainWindowViewModel(WorkTimeModel workTimeModel, IDialogHelper dialogHelper)
        : base(dialogHelper)
    {
        _workTimeModel = workTimeModel;

        LoadedCommand
            .Subscribe(async _ => await CatchErrorAsync(_workTimeModel.LoadDataAsync))
            .AddTo(Disposable);
    }
}
