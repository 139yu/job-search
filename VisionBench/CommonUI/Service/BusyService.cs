using System.Windows;
using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using CommonUI.Helper;
using CommonUI.ViewModels;
using CommonUI.Views;

namespace CommonUI.Service;

public class BusyService: IBusyService
{
    private static readonly NLog.Logger _logger = Log.For<BusyService>(LogModule.App);
    private int _isRunning;
    public async Task<T> RunAsync<T>(BusyRequest request, Func<IProgress<BusyProgress>, CancellationToken, T> work)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if(dispatcher == null || !dispatcher.CheckAccess())
            throw new InvalidOperationException("BusyService.RunAsync 必须在UI线程调用！");
        if (Interlocked.CompareExchange(ref _isRunning, 1, 0) != 0)
            throw new InvalidOperationException("已有耗时操作，请等待其结束！");
        using var cts = new CancellationTokenSource();
        var vm = new WaitingDialogViewModel(request);
        var abandoned = new TaskCompletionSource<bool>();
        WaitingDialog? window = null;
        try
        {
            var progress = new Progress<BusyProgress>(vm.UpdateProgress);
            var worker = Task.Run(() => work(progress, cts.Token));
            // 防闪烁：延迟内跑完就连窗口都不建，避免闪一下
            await Task.WhenAny(worker, Task.Delay(request.ShowDelayMs, cts.Token));
            if (worker.IsCompleted)
                return await worker;
            var dialog = new WaitingDialog()
            {
                DataContext = vm
            };
            if(ResolveOwner() is {} owner)
                dialog.Owner = owner;
            window = dialog;
            vm.CancelRequested += () =>
            {
                if (cts.IsCancellationRequested)
                    return;
                cts.Cancel();
                vm.EnterCancelling();
                ArmCancelTimeout(dialog, worker, abandoned, request.CancelTimeout, request.Title);
            };
            dialog.Loaded += (sender, args) =>
            {
                worker.ContinueWith(_ => dialog.Dispatcher.BeginInvoke(new Action(dialog.CloseByService)),
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            };
            dialog.ShowDialog();
            var finished = await Task.WhenAny(worker, abandoned.Task);
            if (finished != worker)
                throw new OperationCanceledException("耗时操作未在超时内响应取消", cts.Token);
            return await worker;
        }
        finally
        {
            if(window is {IsVisible : true})
                window.CloseByService();
            Interlocked.Exchange(ref _isRunning, 0);
        }
    }

    public Task RunAsync(BusyRequest request, Action<IProgress<BusyProgress>, CancellationToken> work)
    {
        return RunAsync<object>(request, (progress, ct) =>
        {
            work(progress, ct);
            return null;
        });
    }

    private void ArmCancelTimeout(WaitingDialog dialog, Task worker, TaskCompletionSource<bool> abandoned,
        int timeoutMs, string title)
    {
        _ = Task.Delay(timeoutMs).ContinueWith(_ =>
        {
            if (worker.IsCompleted)
                return;
            _logger.Warn($"[BusyService] 超时取消，已放弃：title={title},timeout={timeoutMs}");
            abandoned.SetResult(true);
            GrowlHelper.Warning("操作可能仍在后台执行，请稍后再试。");
            dialog.Dispatcher.BeginInvoke(new Action(dialog.CloseByService));
        },TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    private static Window? ResolveOwner()
    {
        return Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? Application.Current.MainWindow;
    }
}