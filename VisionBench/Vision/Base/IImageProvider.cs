using Vision.Events;

namespace Vision.Base;

/// <summary>
/// 图像提供器接口。
/// 抽象图像来源（相机实时采集、本地文件读取等），统一向订阅方推送就绪的图像帧。
///
/// <para><b>帧的所有权：</b>每一次 <see cref="FrameReady"/> 推送的
/// <see cref="FrameReadyEventArgs.Image"/> 归<b>订阅者</b>所有，订阅者用完后必须释放。
/// 提供器保证同一个实例只交给一个订阅者（多订阅者时各自拿到独立实例），
/// 因此订阅者之间不会互相释放掉对方正在使用的图像。</para>
///
/// <para><b>Start / Stop：</b>控制"是否持续产帧"。相机来源对应连续采集的开关；
/// 一次性来源（如本地文件）在 Start 时推一帧，推送完成后 <see cref="IsRunning"/> 为 false。</para>
///
/// <para><b>线程：</b><see cref="FrameReady"/> 由提供器的内部线程触发。
/// 订阅方若要操作界面，必须自行切回 UI 线程，并且<b>必须用异步投递（BeginInvoke）</b>——
/// 同步等待会阻塞提供器的产帧线程，进而在"停止/关闭"路径上造成互等死锁。</para>
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