using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Commons.Enums;
using Commons.Logging;
using HalconDotNet;
using MainApp.Helper;

namespace MainApp.Display;

/// <summary>
/// 图像画布：把 <see cref="HImage"/> 帧与已注册的叠加层绘制到 <see cref="HSmartWindowControlWPF"/>。
///
/// <para>
/// 生产端可在任意线程调用 <see cref="SetImage"/> 提交新帧，绘制统一排队到创建画布的那个 UI 线程；
/// 叠加层用 <see cref="AddOverlay"/> 注册，绘制顺序即注册顺序。
/// </para>
///
/// <para>
/// Halcon 窗口每次绘制底图都会清空窗口，所以每帧都按「底图 → 叠加层」完整重画一遍，
/// 这也是 <see cref="IImageOverlay.Render"/> 必须保持轻量的原因。
/// </para>
/// </summary>
public class ImageCanvas : IDisposable
{
    private static readonly NLog.Logger _logger = Log.For<ImageCanvas>(LogModule.App);
    /// <summary>承载 Halcon 窗口的控件：既是绘制目标，也是坐标换算与 DPI 信息的来源。</summary>
    private readonly HSmartWindowControlWPF _control;
    /// <summary>创建画布时所在线程的调度器；所有 Halcon 绘制都投递到它，保证单线程访问窗口。</summary>
    private readonly Dispatcher _dispatcher;
    /// <summary>查询「是否以灰度显示」的外部偏好；每次绘制底图时读一次，不改动帧数据。</summary>
    private readonly Func<bool> _isGrayDisplay;

    /// <summary>
    /// 待绘制的帧。生产端比 UI 快时，旧帧在这里被顶替并释放。
    /// </summary>
    private readonly LatestSlot<HImage> _pendingFrame = new();

    /// <summary>当前已显示在窗口上的帧；新帧绘制时整体替换，旧帧随之释放。</summary>
    private readonly LatestSlot<HImage> _currentFrame = new();
    /// <summary>已注册的叠加层，按列表顺序绘制。</summary>
    private readonly List<IImageOverlay> _overlays = new();
    /// <summary>保护 <see cref="_renderQueued"/> 的同步锁；<see cref="SetImage"/> 可能从任意线程进入。</summary>
    private readonly object _gate = new();
    /// <summary>是否已有一笔绘制排在 UI 队列里；置位期间新到的帧只替换待绘制帧，不再重复投递。</summary>
    private bool _renderQueued;
    /// <summary>上次适配窗口时的图像宽度，与 <see cref="_fitHeight"/> 一起用于判断是否需要重新适配。</summary>
    private int _fitWidth = -1;
    /// <summary>上次适配窗口时的图像高度；-1 表示尚未适配过任何帧。</summary>
    private int _fitHeight = -1;

    /// <summary>
    /// 创建画布。<b>必须在 UI 线程构造</b>——调度器取自当前线程，后续绘制都回到这里执行。
    /// </summary>
    /// <param name="control">显示图像的 Halcon 窗口控件。</param>
    /// <param name="isGrayDisplay">查询是否把三通道图像按灰度显示；只影响显示效果，不改动帧本身。</param>
    public ImageCanvas(HSmartWindowControlWPF control, Func<bool> isGrayDisplay)
    {
        _control = control;
        _isGrayDisplay = isGrayDisplay;
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    /// <summary>
    /// 当前窗口是否有图像帧
    /// </summary>
    public bool HasImage => _currentFrame.Peek() is not null;

    /// <summary>
    /// 已注册的叠加层
    /// </summary>
    public IReadOnlyList<IImageOverlay> Overlays => _overlays;

    /// <summary>
    /// 提交一帧新图像。可从任意线程调用。
    /// <para>
    /// 帧的所有权随调用转交给画布：未被绘制就被下一帧顶替的帧会在槽里直接释放。
    /// 绘制请求按批合并，生产速度快于 UI 时队列也不会堆积，只会保留最新一帧。
    /// </para>
    /// </summary>
    /// <param name="image">新帧；为 null 时忽略。</param>
    public void SetImage(HImage image)
    {
        if (image is null)
            return;
        _pendingFrame.Put(image)?.Dispose();
        bool post;
        lock (_gate)
        {
            post = !_renderQueued;
            _renderQueued = true;
        }

        // 期间到的帧只在槽里互相替换，UI队列不会堆积
        if (post)
            _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(RenderPending));
    }

    /// <summary>
    /// 图像尺寸变化时，把窗口视图重新适配到整幅图像。
    /// 尺寸与上次相同时直接跳过，避免每帧都重置用户手动做的缩放与平移。
    /// </summary>
    /// <param name="image">本次要绘制的帧。</param>
    private void EnsureFit(HImage image)
    {
        HOperatorSet.GetImageSize(image,out var width,out var height );
        if (width.I == _fitWidth && height.I == _fitHeight)
            return;
        _fitHeight = height.I;
        _fitWidth = width.I;
        try
        {
            _control.SetFullImagePart(image);
        }
        catch (Exception e)
        {
            _logger.Error(e,"适配图像到窗口失败");
        }
    }

    /// <summary>
    /// 把当前帧重新适配到整个窗口，用于用户缩放/平移后的视图复位。
    /// 没有当前帧时静默返回。
    /// </summary>
    public void ResetView()
    {
        var image = _currentFrame.Peek();
        if (image is null)
            return;
        try
        {
            _control.SetFullImagePart(image);
        }
        catch (Exception e)
        {
            _logger.Error(e,"重置页面失败");
        }
        Redraw();
    }

    /// <summary>
    /// 把一次重绘排到 UI 队列末尾，而不是立即绘制。
    /// 适合需要等布局、DPI 或控件状态稳定后再画的场景（例如窗口尺寸刚变化时）。
    /// </summary>
    public void RequestRedrawDeferred() => _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(Redraw));

    /// <summary>
    /// 取控件上某一点对应的图像坐标与灰度，用于悬停读值。
    /// </summary>
    /// <param name="controlPoint">相对控件左上角的 WPF 设备无关坐标（DIP）；方法内部自行乘 DPI 缩放。</param>
    /// <param name="row">命中的图像行坐标。</param>
    /// <param name="col">命中的图像列坐标。</param>
    /// <param name="gray">该点灰度：单通道直接取值，三通道按 0.299R + 0.578G + 0.114B 折算。</param>
    /// <returns>
    /// 成功返回 true；没有当前帧、取不到显示区域、坐标落在图像外或读取灰度失败时返回 false，
    /// 此时三个输出参数均为 0。
    /// </returns>
    public bool TryGetPixel(Point controlPoint,out int row,out int col,out int gray)
    {
        row = col = gray = 0;
        var image = _currentFrame.Peek();
        if (image is null)
            return false;
        HOperatorSet.GetImageSize(image, out var width, out var height);
        var context = new ImageCanvasContext(_control, width, height);
        var dpi = VisualTreeHelper.GetDpi(_control);
        var physical = new Point(controlPoint.X * dpi.DpiScaleX, controlPoint.Y * dpi.DpiScaleY);
        if (!context.TryScreenToImage(physical, out var imageRow, out var imageCol))
            return false;
        var c = (int)Math.Round(imageCol);
        var r = (int)Math.Round(imageRow);
        if (c < 0 || r < 0 || c >= width.I || r >= height.I)
            return false;
        HOperatorSet.GetGrayval(image,r,c,out var grayval);
        try
        {
            // mono8/mono16 返回 INTEGER 元组，DArr 会抛 HTupleAccessException，ToDArr 两种类型都适用
            var channels = grayval.ToDArr();
            gray = (int) (channels.Length >= 3 ? 0.299 * channels[0] + 0.578 * channels[1] + 0.114 * channels[2] : channels[0]);
        }
        catch (Exception e)
        {
            _logger.Warn(e, "读取像素灰度失败");
            return false;
        }
        finally
        {
            grayval.Dispose();
        }

        row = r;
        col = c;
        return true;
    }
    
    /// <summary>
    /// 清空页面并释放当前帧
    /// </summary>
    public void Clear()
    {
        _pendingFrame.Take()?.Dispose();
        _currentFrame.Take()?.Dispose();
        try
        {
            _control.HalconWindow.ClearWindow();
        }
        catch (Exception e)
        {
            _logger.Error(e, "清空 Halcon 窗口失败！");
        }
    }

    /// <summary>
    /// 把当前显示中的帧写入磁盘，保存格式由文件扩展名决定。
    /// </summary>
    /// <param name="path">目标文件路径。</param>
    /// <returns>写入成功返回 true；没有当前帧或路径为空时返回 false。</returns>
    public bool SaveCurrentImage(string path)
    {
        var image = _currentFrame.Peek();
        if (image is null || string.IsNullOrWhiteSpace(path))
            return false;
        image.WriteImage(ResolveFormat(path), 0, path);
        return true;
    }

    /// <summary>
    /// 由文件扩展名推导 Halcon 需要的格式名：无扩展名按 png，jpeg 归一为 jpg，其余原样使用。
    /// </summary>
    private static string ResolveFormat(string path)
    {
        var extension = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
        return extension switch
        {
            "" => "png",
            "jpeg" => "jpg",
            _ => extension
        };
    }

    /// <summary>
    /// 注册一个叠加层。<see cref="IImageOverlay.Name"/> 是层的身份，同名视为同一层：
    /// 用新实例原位替换旧的（保留绘制顺序），并记一条 Warn——同名多半是两个组件撞名。
    /// 同一个实例重复注册直接返回，不做替换。
    /// </summary>
    public void AddOverlay(IImageOverlay overlay)
    {
        if (overlay is null)
            return;
        var index = _overlays.FindIndex(o => o.Name == overlay.Name);
        if (index < 0)
            _overlays.Add(overlay);
        else
        {
            var old = _overlays[index];
            if (ReferenceEquals(old, overlay))
                return;
            old.Invalidated -= OnOverlayInvalidated;
            _overlays[index] = overlay;
        }

        overlay.Invalidated += OnOverlayInvalidated;
        RequestRedraw();
    }

    /// <summary>
    /// 注销叠加层：解除失效事件订阅并安排重绘；未注册过的实例直接忽略。
    /// </summary>
    public void RemoveOverlay(IImageOverlay overlay)
    {
        if (overlay is null || !_overlays.Remove(overlay))
            return;
        overlay.Invalidated -= OnOverlayInvalidated;
        RequestRedraw();
    }

    /// <summary>任一叠加层声明内容变化时，安排一次重绘。</summary>
    private void OnOverlayInvalidated(object? sender, EventArgs e) => RequestRedraw();

    /// <summary>
    /// 请求用当前帧重绘图像层与全部叠加层。可从任意线程调用。
    /// </summary>
    public void RequestRedraw()
    {
        if (_dispatcher.CheckAccess())
        {
            Redraw();
            return;
        }

        _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(Redraw));
    }

    /// <summary>
    /// UI 线程入口：清掉排队标记，取走待绘制帧替换当前帧，然后重绘。
    /// 没有待绘制帧时（例如已被 <see cref="Clear"/> 清空）直接返回。
    /// </summary>
    private void RenderPending()
    {
        lock (_gate)
        {
            _renderQueued = false;
        }

        var frame = _pendingFrame.Take();
        if (frame is null)
            return;
        _currentFrame.Put(frame)?.Dispose();
        Redraw();
    }

    /// <summary>
    /// 用当前帧完整重绘一次：取窗口 → 适配视图 → 画底图 → 画叠加层。
    /// 只能在 UI 线程调用；没有当前帧时清空窗口。
    /// </summary>
    private void Redraw()
    {
        HWindow window;
        try
        {
            window = _control.HalconWindow;
        }
        catch (Exception e)
        {
            _logger.Warn(e, "取 Halcon 窗口失败，跳过本次绘制");
            return;
        }

        var image = _currentFrame.Peek();
        if (image is null)
        {
            window.ClearWindow();
            return;
        }

        EnsureFit(image);
        DrawImageLayer(window, image);
        DrawOverlays(window, image);
    }

    /// <summary>图像层。灰度显示是查看偏好，不改动帧本身。</summary>
    private void DrawImageLayer(HWindow window, HImage image)
    {
        if (_isGrayDisplay() && image.CountChannels() == 3)
        {
            var gray = image.Rgb1ToGray();
            try
            {
                window.DispObj(gray);
            }
            finally
            {
                gray.Dispose();
            }

            return;
        }

        window.DispObj(image);
    }

    /// <summary>
    /// 按注册顺序绘制所有可见叠加层，并共用同一份上下文。
    /// 单层抛异常只记日志，不影响其余层继续绘制。
    /// </summary>
    private void DrawOverlays(HWindow window, HImage image)
    {
        if (_overlays.Count == 0)
            return;
        HOperatorSet.GetImageSize(image, out var width, out var height);
        var context = new ImageCanvasContext(_control, width, height);
        foreach (var overlay in _overlays)
        {
            if (!overlay.Visible)
                continue;
            try
            {
                overlay.Render(window, context);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"叠加层：{overlay.Name}绘制失败");
            }
        }
    }

    /// <summary>
    /// 释放画布：解除全部叠加层订阅，清空窗口并释放待绘制帧与当前帧。
    /// </summary>
    public void Dispose()
    {
        foreach (var overlay in _overlays.ToList())
        {
            overlay.Invalidated -= OnOverlayInvalidated;
        }
        _overlays.Clear();
        Clear();
    }
}