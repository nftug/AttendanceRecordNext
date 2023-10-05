using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace Presentation.ViewModels;

public class HistoryPageViewModel : ViewModelBase
{
    private readonly HistoryListModel _model;
    private readonly IContentDialogHelper<DatePickerDialogViewModel> _datePickerDialog;

    public ReadOnlyReactiveCollection<HistoryItemViewModel> Items { get; }
    public ReactivePropertySlim<HistoryItemViewModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> MonthlyOvertime { get; }

    public AsyncReactiveCommand<object?> PreviousMonthCommand { get; }
    public AsyncReactiveCommand<object?> NextMonthCommand { get; }
    public AsyncReactiveCommand<object?> NowMonthCommand { get; }
    public AsyncReactiveCommand<object?> LoadItemsCommand { get; }

    public AsyncReactiveCommand<object?> SaveCurrentItemCommand { get; }
    public AsyncReactiveCommand<object?> DeleteCurrentItemCommand { get; }
    public AsyncReactiveCommand<object?> SelectByDateCommand { get; }

    public HistoryPageViewModel(
        IDialogHelper dialogHelper,
        HistoryListModel model,
        IContentDialogHelper<DatePickerDialogViewModel> datePickerDialog
    )
        : base(dialogHelper)
    {
        _model = model;
        _datePickerDialog = datePickerDialog;

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
        MonthlyOvertime = _model.MonthlyOvertime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

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

        SelectByDateCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                DateTime defaultDate = CurrentMonth.Value;
                var viewModel = new DatePickerDialogViewModel(_dialogHelper, defaultDate);
                var result = await _datePickerDialog.ShowAsync(viewModel);
                if (result != Helpers.DialogResult.OK) return;

                await _model.SelectByDateAsync(viewModel.SelectedDate.Value);
            })
            .AddTo(Disposable);
    }
}
