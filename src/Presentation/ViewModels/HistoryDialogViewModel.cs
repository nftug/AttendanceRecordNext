using System.Collections.Specialized;
using System.Reactive.Linq;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class HistoryDialogViewModel : ViewModelBase, IDialogAware
{
    private readonly HistoryListModel _model;

    public ReactiveCollection<HistoryItemViewModel> Items { get; }
    public ReactivePropertySlim<HistoryItemViewModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }

    public AsyncReactiveCommand<object?> LoadItemsCommand { get; }

    public HistoryDialogViewModel(IDialogHelper dialogHelper, HistoryListModel model)
        : base(dialogHelper)
    {
        _model = model;
        Items = new ReactiveCollection<HistoryItemViewModel>().AddTo(Disposable);

        _model.Items.ToCollectionChanged()
            .Where(x => x.Action == NotifyCollectionChangedAction.Add && x.Value != null)
            .Subscribe(x => Items.AddOnScheduler(new(dialogHelper, x.Value!)))
            .AddTo(Disposable);
        _model.Items.ToCollectionChanged()
            .Where(x => x.Action == NotifyCollectionChangedAction.Remove && x.Value != null)
            .Subscribe(x => Items.RemoveOnScheduler(Items.First(i => i.RecordedDate == x.Value!.RecordedDate)))
            .AddTo(Disposable);
        _model.Items.ToCollectionChanged()
            .Where(x => x.Action == NotifyCollectionChangedAction.Reset)
            .Subscribe(x => Items.ClearOnScheduler())
            .AddTo(Disposable);

        SelectedItem = new ReactivePropertySlim<HistoryItemViewModel?>().AddTo(Disposable);
        CurrentMonth = new ReactivePropertySlim<DateTime>(DateTime.Today).AddTo(Disposable);

        Items.ToCollectionChanged()
            .Where(x =>
                (x.Action == NotifyCollectionChangedAction.Remove
                && x.Value == SelectedItem.Value)
                || x.Action == NotifyCollectionChangedAction.Reset)
            .Subscribe(_ => SelectedItem.Value = null)
            .AddTo(Disposable);

        LoadItemsCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ => await _model.LoadMonthlyAsync(CurrentMonth.Value))
            .AddTo(Disposable);
    }

    public string Title => "履歴";

    public event Action<IDialogResult>? RequestClose;

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        LoadItemsCommand.Execute(null);
    }
}
