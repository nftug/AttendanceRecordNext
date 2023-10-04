using Presentation.Shared;

namespace Presentation.Helpers;

public interface IContentDialogHelper<TViewModel>
    where TViewModel : ContentDialogViewModelBase
{
    Task<DialogResult> ShowAsync(TViewModel viewModel, string? title = null, string? message = null);
}
