using Commons.Base;

namespace CommonUI.Service;

public interface IBusyService
{
    Task<T> RunAsync<T>(BusyRequest request,Func<IProgress<BusyProgress>,CancellationToken,T> work);
    Task RunAsync(BusyRequest request,Action<IProgress<BusyProgress>,CancellationToken> work);
}