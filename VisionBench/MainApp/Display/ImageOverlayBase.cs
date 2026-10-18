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
    public string Name { get; }
    public bool Visible { get; set; }
    public event EventHandler? Invalidated;
    /// <summary>通知显示层：本层内容变了，请重绘。</summary>
    protected void Invalidate() => Invalidated?.Invoke(this, EventArgs.Empty);
    public abstract void Render(HWindow window, ImageCanvasContext context);

    protected ImageOverlayBase(string name)
    {
        this.Name = name;
    }
}