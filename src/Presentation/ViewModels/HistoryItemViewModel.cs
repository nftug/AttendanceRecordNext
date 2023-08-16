using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;

namespace Presentation.ViewModels;

public class HistoryItemViewModel : ViewModelBase
{
    private readonly HistoryItemModel _model;

    public ReadOnlyReactivePropertySlim<DateTime> RecordedDate => _model.RecordedDate;

    public HistoryItemViewModel(IDialogHelper dialogHelper, HistoryItemModel model) : base(dialogHelper)
    {
        _model = model;
    }
}
