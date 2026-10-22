using HalconDotNet;

namespace MainApp.Display;

/// <summary>
/// 叠加层的通用基类，省掉 <see cref="IImageOverlay.Invalidated"/> 的样板代码。
/// <para>
/// 具体叠加层继承本类，重写 <see cref="Name"/> 与 <see cref="Render"/>；
/// 内容变化时调用 <see cref="Invalidate"/> 即可触发重绘。
/// </para>
/// </summary>
public abstract class ImageOverlayBase : IImageOverlay
{
    /// <summary>
    /// 叠加层名称，构造时确定、之后不可变，作为层在 <see cref="ImageCanvas"/> 中的身份。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 是否可见。为 false 时画布跳过本层绘制，但保留注册与顺序。
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>内容变化通知；重写 <see cref="Render"/> 后调用 <see cref="Invalidate"/> 触发重绘。</summary>
    public event EventHandler? Invalidated;

    /// <summary>通知显示层：本层内容变了，请重绘。</summary>
    protected void Invalidate() => Invalidated?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// 把本层绘制到窗口上。每帧底图绘制后都会调用，因此实现必须轻量：只画，不算、不阻塞、不做 I/O。
    /// </summary>
    /// <param name="window">Halcon 窗口，绘制目标。</param>
    /// <param name="context">当前帧的图像尺寸与屏幕/图像坐标换算。</param>
    public abstract void Render(HWindow window, ImageCanvasContext context);

    /// <summary>
    /// 以给定名称创建叠加层。名称重复会在注册时视为同一层并被原位替换，所以应保证全局唯一。
    /// </summary>
    /// <param name="name">叠加层名称。</param>
    protected ImageOverlayBase(string name)
    {
        this.Name = name;
    }
}