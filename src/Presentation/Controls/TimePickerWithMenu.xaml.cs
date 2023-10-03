using System.Windows;

namespace Presentation.Controls
{
    /// <summary>
    /// TimePickerWithMenu.xaml の相互作用ロジック
    /// </summary>
    public partial class TimePickerWithMenu : System.Windows.Controls.UserControl
    {
        public DateTime? SelectedDateTime
        {
            get => (DateTime?)GetValue(SelectedDateTimeProperty);
            set { SetValue(SelectedDateTimeProperty, value); }
        }

        public static readonly DependencyProperty SelectedDateTimeProperty =
             DependencyProperty.Register(
                 nameof(SelectedDateTime),
                 typeof(DateTime?),
                 typeof(TimePickerWithMenu),
                 new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)
             );

        public TimePickerWithMenu()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedDateTime = null;
        }
    }
}
