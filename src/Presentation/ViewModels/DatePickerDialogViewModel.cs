using Presentation.Helpers;
using Presentation.Shared;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Presentation.ViewModels;

public class DatePickerDialogViewModel : ContentDialogViewModelBase
{
    public ReactivePropertySlim<DateTime> SelectedDate { get; }

    public DatePickerDialogViewModel(IDialogHelper dialogHelper, DateTime? defaultDate = default) : base(dialogHelper)
    {
        defaultDate ??= DateTime.Now;
        SelectedDate = new ReactivePropertySlim<DateTime>((DateTime)defaultDate).AddTo(Disposable);
    }
}
