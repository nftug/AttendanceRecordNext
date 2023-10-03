using ModernWpf.Controls;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Presentation.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class MainWindowViewModel : MainWindowViewModelBase
{
    private readonly WorkTimeModel _workTimeModel;
    private readonly AppConfigModel _appConfigModel;
    private readonly NavigationModel _navigationModel;

    public ReadOnlyReactivePropertySlim<string?> Title { get; }
    public ReactivePropertySlim<string?> HeaderTitle { get; }

    public ReactiveCommandSlim<NavigationViewSelectionChangedEventArgs> NavigationChangedCommand { get; }
    public ReactiveCommandSlim<Type> NavigateCommand { get; }

    private static IReadOnlyDictionary<NavigationItem, Type> _pages = new Dictionary<NavigationItem, Type>()
        {
            { NavigationItem.Home, typeof(HomePage) },
            { NavigationItem.History, typeof(HistoryPage) },
            { NavigationItem.None, typeof(BlankPage) }
        };

    public enum NavigationItem
    {
        [StringValue("ホーム")]
        Home,
        [StringValue("履歴")]
        History,
        [StringValue("設定")]
        Setting,
        None
    }

    public MainWindowViewModel(WorkTimeModel workTimeModel, IDialogHelper dialogHelper, AppConfigModel appConfigModel, NavigationModel navigationModel)
        : base(dialogHelper)
    {
        _workTimeModel = workTimeModel;
        _appConfigModel = appConfigModel;
        _navigationModel = navigationModel;

        HeaderTitle = _navigationModel.HeaderTitle.ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);
        Title = _appConfigModel.Title.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        LoadedCommand
            .Subscribe(async _ => await CatchErrorAsync(_workTimeModel.LoadDataAsync))
            .AddTo(Disposable);

        NavigateCommand = new ReactiveCommandSlim<Type>().AddTo(Disposable);

        NavigationChangedCommand = new ReactiveCommandSlim<NavigationViewSelectionChangedEventArgs>()
            .WithSubscribe(args =>
            {
                if (args == null) return;

                var selectedItem = (NavigationViewItem)args.SelectedItem;

                // Tag取得
                string? itemName = selectedItem.Tag?.ToString();

                Type? pageType = null;

                if (Enum.TryParse(itemName, out NavigationItem item))
                {
                    // ヘッダーの設定
                    _navigationModel.HeaderTitle.Value =
                    HeaderTitle.Value = item.GetStringValue();
                    // 遷移先のページを取得
                    _pages.TryGetValue(item, out pageType);
                }
                else
                {
                    HeaderTitle.Value = string.Empty;
                }

                // ページ遷移
                NavigateCommand.Execute(pageType ?? _pages[NavigationItem.None]);
            });
    }
}
