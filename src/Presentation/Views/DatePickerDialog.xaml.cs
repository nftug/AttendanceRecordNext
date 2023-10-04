using ModernWpf.Controls;
using Presentation.Helpers;
using Presentation.ViewModels;

namespace Presentation.Views;

/// <summary>
/// DatePickerDialog.xaml の相互作用ロジック
/// </summary>
public partial class DatePickerDialog : ContentDialog
{
    public DatePickerDialog()
    {
        InitializeComponent();
    }
}

public class DatePickerDialogHelper : IContentDialogHelper<DatePickerDialogViewModel>
{
    public async Task<Helpers.DialogResult> ShowAsync(DatePickerDialogViewModel viewModel, string? title = null, string? message = null)
    {
        viewModel.Title.Value = title ?? "日付の選択";
        viewModel.Message.Value = message ?? "日付を選択してください。";

        var dialog = new DatePickerDialog()
        {
            DataContext = viewModel,
            PrimaryButtonText = "OK",
            CloseButtonText = "キャンセル"
        };

        var result = await dialog.ShowAsync();

        return result switch
        {
            ContentDialogResult.Primary => Helpers.DialogResult.OK,
            _ => Helpers.DialogResult.Cancel
        };
    }
}