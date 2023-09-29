using ModernWpf.Controls;
using System.Windows;

namespace Presentation.Views
{
    public enum NavigationIcon
    {
        Home,
        // History,
        // Setting,
        None
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IReadOnlyDictionary<NavigationIcon, Type> _pages = new Dictionary<NavigationIcon, Type>()
        {
            { NavigationIcon.Home, typeof(HomePage) },
            { NavigationIcon.None, typeof(HomePage) }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;
                
                // Tag取得
                string? iconName = selectedItem.Tag?.ToString();

                // ヘッダー設定
                sender.Header = iconName;

                if (Enum.TryParse(iconName, out NavigationIcon icon))
                {
                    // 対応するページを表示
                    ContentFrame.Navigate(_pages[icon]);
                }
                else
                {
                    // 空ページ表示
                    ContentFrame.Navigate(_pages[NavigationIcon.None]);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
    }
}
