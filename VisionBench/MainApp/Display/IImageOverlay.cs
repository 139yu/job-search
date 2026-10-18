using HalconDotNet;

namespace MainApp.Display;

/// <summary>
/// 图像叠加层。
///
/// <para>
/// 显示层的扩展点：ROI 框、测量标注、算法结果区域/轮廓、文字说明……
/// 任何"画在图像之上"的东西都实现本接口，注册到 <see cref="ImageCanvas"/> 即可，
/// <b>不需要改动显示层本身</b>。
/// </para>
///
/// <para>
/// <b>实现约束：</b><see cref="Render"/> 会在<b>每一帧图像更新后</b>被调用
/// （因为 Halcon 窗口的底图绘制会清空窗口，叠加层无法只画一次）。
/// 因此实现里只做绘制：不阻塞、不做耗时计算、不访问设备、不做 I/O。
/// 需要计算的东西应该在别处算好，放进本层的字段里。
/// </para>
/// </summary>
public interface IImageOverlay
{
    /// <summary>叠加层名称，用于去重、界面开关与日志。</summary>
    string Name { get; }
    /// <summary>是否可见。为 false 时 <see cref="ImageCanvas"/> 会跳过本层的绘制。</summary>
    bool Visible { get; set; }
    /// <summary>
    /// 本层内容发生变化、需要重绘时触发。
    /// <see cref="ImageCanvas"/> 会订阅它并安排一次重绘——例如 ROI 拖动时每移动一次就触发一次。
    /// </summary>
    event EventHandler? Invalidated;
    /// <summary>
    /// 把本层绘制到窗口上。运行在 UI 线程，且由 <see cref="ImageCanvas"/> 统一调用。
    /// </summary>
    /// <param name="window">Halcon 窗口，绘制目标。</param>
    /// <param name="context">当前图像与坐标换算的上下文。</param>
    void Render(HWindow window,ImageCanvasContext context);
}