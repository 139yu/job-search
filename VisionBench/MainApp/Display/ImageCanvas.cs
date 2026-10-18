using System.IO;
using System.Windows.Threading;
using Commons.Enums;
using Commons.Logging;
using HalconDotNet;
using MainApp.Helper;

namespace MainApp.Display;

public class ImageCanvas : IDisposable
{
    private static readonly NLog.Logger _logger = Log.For<ImageCanvas>(LogModule.App);
    private readonly HSmartWindowControlWPF _control;
    private readonly Dispatcher _dispatcher;
    private readonly Func<bool> _isGrayDisplay;

    /// <summary>
    /// 待绘制的帧。生产端比 UI 快时，旧帧在这里被顶替并释放。
    /// </summary>
    private readonly LatestSlot<HImage> _pendingFrame = new();

    private readonly LatestSlot<HImage> _currentFrame = new();
    private readonly List<IImageOverlay> _overlays = new();
    private readonly object _gate = new();
    private bool _renderQueued;

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

    public bool SaveCurrentImage(string path)
    {
        var image = _currentFrame.Peek();
        if (image is null || string.IsNullOrWhiteSpace(path))
            return false;
        image.WriteImage(ResolveFormat(path), 0, path);
        return true;
    }

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

    public void RemoveOverlay(IImageOverlay overlay)
    {
        if (overlay is null || !_overlays.Remove(overlay))
            return;
        overlay.Invalidated -= OnOverlayInvalidated;
        RequestRedraw();
    }

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

        DrawImageLayer(window, image);
        DrawOverlays(window, image);
    }

    /// <summary>图像层。灰度显示是查看偏好，不改动帧本身。</summary>
    private void DrawImageLayer(HWindow window, HImage image)
    {
        if (!_isGrayDisplay() && image.CountChannels() == 3)
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