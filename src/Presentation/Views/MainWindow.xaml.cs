using ModernWpf.Controls;
using Presentation.ViewModels;
using System.Windows;

namespace Presentation.Views
{
    public enum NavigationItem
    {
        [StringValue("ホーム")]
        Home,
        [StringValue("履歴")]
        History,
        [StringValue("設定")]
        Settings,
        None
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly IReadOnlyDictionary<NavigationItem, Type> _pages =
            new Dictionary<NavigationItem, Type>()
            {
                { NavigationItem.Home, typeof(HomePage) },
                { NavigationItem.History, typeof(HistoryPage) },
                { NavigationItem.Settings, typeof(SettingsPage) },
                { NavigationItem.None, typeof(BlankPage) }
            };

        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _viewModel.ActivateCommand.Subscribe(_ => Activate());

            DataContext = _viewModel;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;

                // Tag取得
                string? itemName = selectedItem.Tag?.ToString();

                Type? pageType = null;

                if (Enum.TryParse(itemName, out NavigationItem item))
                {
                    // ヘッダーの設定
                    _viewModel.HeaderTitle.Value = item.GetStringValue();
                    // 遷移先のページを取得
                    _pages.TryGetValue(item, out pageType);
                }
                else
                {
                    _viewModel.HeaderTitle.Value = string.Empty;
                }

                // ページ遷移
                ContentFrame.Navigate(pageType ?? _pages[NavigationItem.None]);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
