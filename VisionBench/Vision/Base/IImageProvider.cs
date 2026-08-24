using Vision.Events;

namespace Vision.Base;

/// <summary>
/// 图像提供器接口。
/// 抽象图像来源（相机实时采集、本地文件读取等），
/// 统一向订阅方推送就绪的图像帧。
/// </summary>
public interface IImageProvider
{
    /// <summary>
    /// 帧就绪事件。每当产生一帧新的图像时触发。
    /// </summary>
    event EventHandler<FrameReadyEventArgs> FrameReady;

    /// <summary>
    /// 启动图像提供器，开始产生并推送图像帧。
    /// </summary>
    void Start();

    /// <summary>
    /// 停止图像提供器，停止产生并推送图像帧。
    /// </summary>
    void Stop();

    /// <summary>
    /// 获取图像提供器当前是否处于运行状态。
    /// </summary>
    bool IsRunning { get; }
}