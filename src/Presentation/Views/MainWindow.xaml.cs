using ModernWpf.Controls;
using Presentation.Models;
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
        Setting,
        None
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IReadOnlyDictionary<NavigationItem, Type> _pages = new Dictionary<NavigationItem, Type>()
        {
            { NavigationItem.Home, typeof(HomePage) },
            { NavigationItem.History, typeof(HistoryPage) },
            { NavigationItem.None, typeof(BlankPage) }
        };

        private readonly NavigationModel _navigationModel;

        public MainWindow(NavigationModel navigationModel)
        {
            InitializeComponent();

            _navigationModel = navigationModel;
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
                    _navigationModel.HeaderTitle.Value = item.GetStringValue();
                    // 遷移先のページを取得
                    _pages.TryGetValue(item, out pageType);
                }
                else
                {
                    _navigationModel.HeaderTitle.Value = string.Empty;
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
