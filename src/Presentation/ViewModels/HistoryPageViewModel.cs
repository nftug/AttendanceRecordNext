using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class HistoryPageViewModel : ViewModelBase
{
    private readonly HistoryListModel _model;

    public ReadOnlyReactiveCollection<HistoryItemViewModel> Items { get; }
    public ReactivePropertySlim<HistoryItemViewModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }

    public AsyncReactiveCommand<object?> PreviousMonthCommand { get; }
    public AsyncReactiveCommand<object?> NextMonthCommand { get; }
    public AsyncReactiveCommand<object?> LoadItemsCommand { get; }

    public HistoryPageViewModel(IDialogHelper dialogHelper, HistoryListModel model)
        : base(dialogHelper)
    {
        _model = model;

        Items = _model.Items
            .ToReadOnlyReactiveCollection(x => new HistoryItemViewModel(_dialogHelper, x))
            .AddTo(Disposable);
        SelectedItem = _model.SelectedItem
            .ToReactivePropertySlimAsSynchronized(
                x => x.Value,
                convert: x => x != null ? new HistoryItemViewModel(_dialogHelper, x) : null,
                convertBack: x => x?.Model
            )
            .AddTo(Disposable);

        CurrentMonth = _model.CurrentMonth.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);

        LoadItemsCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.LoadMonthlyAsync())
            .AddTo(Disposable);

        PreviousMonthCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                CurrentMonth.Value = CurrentMonth.Value.AddMonths(-1);
                await LoadItemsCommand.ExecuteAsync(null);
            });

        NextMonthCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                CurrentMonth.Value = CurrentMonth.Value.AddMonths(1);
                await LoadItemsCommand.ExecuteAsync(null);
            });
    }
}
