using Prism.Services.Dialogs;
using System.Windows;

namespace Presentation.Views
{
    /// <summary>
    /// DialogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DialogWindow : Window, IDialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; } = null!;
    }
}
