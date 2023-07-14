using System.ComponentModel;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;

namespace Presentation.Shared;

public abstract class BindableBase : IDisposable, INotifyPropertyChanged
{
    private bool disposedValue;
    protected bool preventDispose;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected CompositeDisposable Disposable { get; } = new();

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        if (!disposedValue && !preventDispose)
        {
            Disposable.Dispose();
            Dispose(disposing: true);

            disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }

    // For dummy method to suppress warnings
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
