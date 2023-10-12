using System.Text;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using static Domain.Config.AppConfig;

namespace Presentation.Models;

public class StatusFormatModel : BindableBase
{
    private readonly WorkTimeModel _workTimeModel;
    private readonly SettingsModel _settingsModel;

    private readonly ReactivePropertySlim<string?> _formattedText;
    public ReadOnlyReactivePropertySlim<string?> FormattedText { get; }

    private StatusFormatConfig StatusFormatConfig => _settingsModel.Config.Value.StatusFormat;

    public StatusFormatModel(WorkTimeModel workTimeModel, SettingsModel settingsModel)
    {
        _workTimeModel = workTimeModel;
        _settingsModel = settingsModel;

        _formattedText = new ReactivePropertySlim<string?>().AddTo(Disposable);
        FormattedText = _formattedText.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
    }

    public void CopyFormattedTextToClipboard()
    {
        try
        {
            var builder = new StringBuilder(StatusFormatConfig.StatusFormat);

            builder
                .Replace("{daily_work}", FormatTimeSpan(_workTimeModel.TotalWorkTime.Value))
                .Replace("{daily_rest}", FormatTimeSpan(_workTimeModel.TotalRestTime.Value))
                .Replace("{daily_over}", FormatTimeSpan(_workTimeModel.Overtime.Value))
                .Replace("{monthly_over}", FormatTimeSpan(_workTimeModel.MonthlyOvertime.Value));

            _formattedText.Value = builder.ToString();

            Clipboard.SetText(_formattedText.Value);
        }
        catch (FormatException e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
            throw;
        }
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        string result = string.Empty;
        if (timeSpan < TimeSpan.Zero) result = "-";
        result += string.Format($"{"{0:"}{StatusFormatConfig.TimeSpanFormat}{"}"}", timeSpan);
        return result;
    }
}
