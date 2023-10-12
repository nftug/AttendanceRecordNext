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
        _formattedText.Value =
            GetFormattedText(StatusFormatConfig.StatusFormat, StatusFormatConfig.TimeSpanFormat)
            ?? throw new FormatException();

        Clipboard.SetText(_formattedText.Value);
    }

    internal string? GetFormattedText(string format, string timeSpanFormat)
    {
        try
        {
            if (timeSpanFormat is not { Length: > 0 }) return null;

            var builder = new StringBuilder(format);
            builder
                .Replace("{daily_work}", GetFormattedTimeSpan(_workTimeModel.TotalWorkTime.Value, timeSpanFormat))
                .Replace("{daily_rest}", GetFormattedTimeSpan(_workTimeModel.TotalRestTime.Value, timeSpanFormat))
                .Replace("{daily_over}", GetFormattedTimeSpan(_workTimeModel.Overtime.Value, timeSpanFormat))
                .Replace("{monthly_over}", GetFormattedTimeSpan(_workTimeModel.MonthlyOvertime.Value, timeSpanFormat));

            return builder.ToString();
        }
        catch (FormatException e)
        {
            System.Diagnostics.Debug.WriteLine(e.Message);
            return null;
        }
    }

    internal static string? GetFormattedTimeSpan(TimeSpan timeSpan, string timeSpanFormat)
    {
        if (timeSpanFormat is not { Length: > 0 }) return null;

        try
        {
            string result = string.Empty;
            if (timeSpan < TimeSpan.Zero) result = "-";
            result += string.Format($"{"{0:"}{timeSpanFormat}{"}"}", timeSpan);
            return result;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
