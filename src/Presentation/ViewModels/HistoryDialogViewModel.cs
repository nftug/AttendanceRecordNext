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

    public ReadOnlyReactiveCollection<HistoryItemViewModel> Items { get; }
    public ReactivePropertySlim<HistoryItemViewModel?> SelectedItem { get; }
    public ReactivePropertySlim<DateTime> CurrentMonth { get; }

    public AsyncReactiveCommand<object?> LoadItemsCommand { get; }

    public HistoryDialogViewModel(IDialogHelper dialogHelper, HistoryListModel model)
        : base(dialogHelper)
    {
        _model = model;

        Items = _model.Items
            .ToReadOnlyReactiveCollection(x => new HistoryItemViewModel(_dialogHelper, x))
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
