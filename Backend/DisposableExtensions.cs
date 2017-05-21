using System.Diagnostics.Contracts;

namespace System.Reactive.Disposables
{
    public static class DisposableExtensions
    {
        public static T DisposeWith<T>(this T disposable, CompositeDisposable disposer)
            where T : IDisposable
        {
            Contract.Requires(disposer != null);
            Contract.Requires(disposable != null);
            
            disposer.Add(disposable);
            return disposable;
        }
    }
}
