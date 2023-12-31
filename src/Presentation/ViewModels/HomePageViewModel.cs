﻿using System.Reactive.Linq;
using Presentation.Helpers;
using Presentation.Models;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class HomePageViewModel : ViewModelBase
{
    private readonly WorkTimeModel _model;
    private readonly MainWindowModel _mainWindowModel;
    private readonly StatusFormatModel _statusFormatModel;

    public ReadOnlyReactivePropertySlim<TimeSpan> TotalWorkTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> TotalRestTime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> Overtime { get; }
    public ReadOnlyReactivePropertySlim<TimeSpan> MonthlyOvertime { get; }
    public ReadOnlyReactivePropertySlim<bool> IsResting { get; }
    public ReadOnlyReactivePropertySlim<bool> IsOngoing { get; }
    public ReactivePropertySlim<DateTime> NowDateTime { get; }

    public AsyncReactiveCommand<object?> ToggleWorkCommand { get; }
    public AsyncReactiveCommand<object?> ToggleRestCommand { get; }
    public AsyncReactiveCommand<object?> CopyFormattedTextCommand { get; }

    public HomePageViewModel(
        WorkTimeModel model,
        MainWindowModel mainWindowModel,
        IDialogHelper dialogHelper,
        WorkTimeAlarmModel workTimeAlarmModel,
        RestTimeAlarmModel restTimeAlarmModel,
        StatusFormatModel statusFormatModel
    )
        : base(dialogHelper)
    {
        _model = model;
        _mainWindowModel = mainWindowModel;
        _statusFormatModel = statusFormatModel;

        TotalWorkTime = _model.TotalWorkTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        TotalRestTime = _model.TotalRestTime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        Overtime = _model.Overtime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        MonthlyOvertime = _model.MonthlyOvertime.ToReadOnlyReactivePropertySlim().AddTo(Disposable);

        IsResting = _model.IsResting.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        IsOngoing = _model.IsOngoing.ToReadOnlyReactivePropertySlim().AddTo(Disposable);
        NowDateTime = new ReactivePropertySlim<DateTime>(DateTime.Now);

        Observable.CombineLatest(_model.IsWorking, _model.IsResting, (working, resting) => (working, resting))
            .Subscribe(x =>
            {
                var stateNameSuffix =
                    x.working ? " - 勤務中" : x.resting ? " - 休憩中" : null;
                _mainWindowModel.WindowTitle.Value = $"{_mainWindowModel.AppName}{stateNameSuffix}";
            })
            .AddTo(Disposable);

        ToggleWorkCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                if (_model.IsOngoing.Value)
                {
                    var ans = await _dialogHelper.ShowDialogAsync(
                        "本日の勤務を終了しますか？",
                        "退勤",
                        DialogButton.YesNo, DialogImage.Question
                    );
                    if (ans != Helpers.DialogResult.Yes) return;
                }

                await CatchErrorAsync(_model.ToggleWorkAsync);
            })
            .AddTo(Disposable);

        ToggleRestCommand = _model.IsOngoing
            .ToAsyncReactiveCommand()
            .WithSubscribe(async _ => await CatchErrorAsync(_model.ToggleRestAsync))
            .AddTo(Disposable);

        CopyFormattedTextCommand = new AsyncReactiveCommand<object?>()
            .WithSubscribe(async _ =>
            {
                try
                {
                    _statusFormatModel.CopyFormattedTextToClipboard();
                    await _dialogHelper.ShowDialogAsync(
                        "クリップボードに現時点の記録のテキストをコピーしました。",
                        "記録をクリップボードにコピー"
                    );
                }
                catch (FormatException)
                {
                    await _dialogHelper.ShowDialogAsync(
                        "記録のフォーマット設定が不正です。",
                        "エラー"
                    );
                }
            });

        _model.Timer
            .ObserveOnUIDispatcher()
            .Subscribe(_ => NowDateTime.Value = DateTime.Now)
            .AddTo(Disposable);
    }
}
