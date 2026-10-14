using Commons.Base;
using Commons.Enums;
using Commons.Logging;
using HalconDotNet;
using Vision.Base;
using Vision.Enums;
using Vision.Events;
using Vision.Halcon;

namespace Vision.ImageProvider;

public sealed class CameraImageProvider : IImageProvider
{
    private static readonly NLog.Logger _logger = Log.For<CameraImageProvider>(LogModule.Camera);
    private readonly object _gate = new();
    private ICameraDevice? _device;
    private CancellationTokenSource? _cts;
    private Task? _pump;
    public event EventHandler<FrameReadyEventArgs>? FrameReady;
    public bool IsRunning => CurrentDevice()?.State == CameraStateEnum.Grabbing;
    public void Start()
    {
        var device = CurrentDevice() ?? throw new BusinessException("工位相机未连接，无法开始采集");
        device.StartAcquisition();
    }

    public void Stop()
    {
        var device = CurrentDevice();
        if (device is null)
            return;
        device.StopAcquisition();
        // 不依赖设备实现自己清队列：换一个品牌若没在 StopAcquisition 里清，
        // 停止后仍会推出残留帧（画面再跳一下）。清理放这里，对所有设备实现都成立。
        device.ClearFrame();
    }

    internal void Attach(ICameraDevice device)
    {
        CancellationTokenSource newCts;
        CancellationTokenSource oldCts;
        lock (_gate)
        {
            if (ReferenceEquals(_device, device) && _pump is { IsCompleted: false })
                return;
            oldCts = _cts;
            newCts = new CancellationTokenSource();
            _device = device;
            _pump = Task.Run(() => Pump(newCts.Token, newCts));
        }

        try
        {
            oldCts?.Cancel();
        }
        catch (Exception e)
        {}
    }

    internal void Detach()
    {
        CancellationTokenSource? cts;
        lock (_gate)
        {
            cts = _cts;
            _cts = null;
            _pump = null;
            _device = null;
        }

        try
        {
            cts?.Cancel();
        }
        catch (Exception e)
        {
            
        }
    }

    private void Pump(CancellationToken token,CancellationTokenSource cts)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var device = CurrentDevice();
                if (device is null || !device.TryGetFrame(out var frame))
                {
                    token.WaitHandle.WaitOne(1);
                    continue;
                }

                HImage? image = null;
                try
                {
                    image = CameraFrameConverter.ToHImage(frame);
                    // 分发成功，图像资源所有权已经交给订阅方，本方法不得再修改它
                    if (FrameDistributor.Dispatch(FrameReady, this, image, ImageSourceEnum.Camera))
                        image = null;
                }
                catch (Exception e)
                {
                    _logger.Error(e, "推送图像帧失败");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e, "图像泵异常退出");
        }
        finally
        {
            cts.Dispose();
        }
    }
    private ICameraDevice? CurrentDevice()
    {
        lock (_gate)
        {
            return _device;
        }
    }
}