using HalconDotNet;
using Vision.Enums;

namespace Vision.Events;

/// <summary>
/// 帧就绪事件参数。
/// 当图像提供器产生新的一帧图像时，随 FrameReady 事件传递给订阅方。
/// </summary>
public class FrameReadyEventArgs : EventArgs
{
    /// <summary>
    /// 就绪的图像帧（Halcon 图像对象）。
    /// </summary>
    public HImage Image { get; set; }

    /// <summary>
    /// 图像来源（相机采集 / 文件读取），用于标识当前帧的出处。
    /// </summary>
    public ImageSourceEnum SourceName { get; set; }
}