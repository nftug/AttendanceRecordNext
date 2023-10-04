using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Presentation.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace Presentation.ViewModels;

public class HistoryPageViewModel : ViewModelBase
{
    private readonly HistoryListModel _model;

    public ReadOnlyReactiveCollection<HistoryItemViewModel> Items { get; }
    public ReactivePropertySlim<HistoryItemViewModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }

    public AsyncReactiveCommand<object?> PreviousMonthCommand { get; }
    public AsyncReactiveCommand<object?> NextMonthCommand { get; }
    public AsyncReactiveCommand<object?> NowMonthCommand { get; }
    public AsyncReactiveCommand<object?> LoadItemsCommand { get; }

    public AsyncReactiveCommand<object?> SaveCurrentItemCommand { get; }
    public AsyncReactiveCommand<object?> DeleteCurrentItemCommand { get; }
    public AsyncReactiveCommand<object?> NewItemCommand { get; }

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
            .WithSubscribe(async _ => await _model.LoadPreviousMonthAsync());

        NextMonthCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.LoadNextMonthAsync());

        NowMonthCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.LoadNowMonthAsync());

        SaveCurrentItemCommand = SelectedItem
            .Select(v => v != null)
            .ToAsyncReactiveCommand()
            .WithSubscribe(async _ => await SelectedItem.Value!.SaveItemCommand.ExecuteAsync(null))
            .AddTo(Disposable);

        DeleteCurrentItemCommand = SelectedItem
            .Select(v => v != null)
            .ToAsyncReactiveCommand()
            .WithSubscribe(async _ => await SelectedItem.Value!.DeleteItemCommand.ExecuteAsync(null))
            .AddTo(Disposable);

        // TODO: 日付の設定を修正
        NewItemCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                DatePickerDialog dialog = new();
                var result = await dialog.ShowAsync();

                _model.AddNewItem(DateTime.Now.AddDays(-1));
            })
            .AddTo(Disposable);
    }
}
