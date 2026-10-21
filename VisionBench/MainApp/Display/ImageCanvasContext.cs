using System.Windows;
using System.Windows.Media;
using HalconDotNet;

namespace MainApp.Display;

/// <summary>
/// 叠加层在绘制时能拿到的全部上下文。
///
/// <para>
/// 存在的意义是把叠加层与具体控件解耦：叠加层只依赖本类，
/// 因此不绑定 <c>HSmartWindowControlWPF</c>，也就能脱离界面单测（喂一个假的上下文即可）。
/// </para>
///
/// <para>
/// <b>坐标转换是 ROI / 测量的地基。</b>当前实现按"图像显示区域（HImagePart）"
/// 与控件大小做线性换算。若后续发现画出来的标记与图像对不齐，
/// <b>第一件事就是怀疑这里</b>（见 §6 未验证项）。
/// </para>
///
/// <para>
/// <b>坐标口径统一为 Halcon 窗口的物理像素。</b>类内部的控件尺寸取自
/// <see cref="ControlPixelWidth"/> / <see cref="ControlPixelHeight"/>，它们已经把 WPF 的
/// 设备无关单位乘上了 DPI 缩放；所以从外部传进来的屏幕坐标也必须是同一口径，
/// 详见 <see cref="TryScreenToImage"/>。
/// </para>
/// </summary>
public class ImageCanvasContext
{
    private readonly HSmartWindowControlWPF _control;

    /// <summary>当前帧的图像宽度，即 Halcon 的列数。由画布每帧构建上下文时写入，叠加层只读。</summary>
    public int ImageWidth { get; set; }

    /// <summary>当前帧的图像高度，即 Halcon 的行数。由画布每帧构建上下文时写入，叠加层只读。</summary>
    public int ImageHeight { get; set; }

    /// <summary>
    /// 构造上下文。只由画布在每帧更新时创建，叠加层不自己 new。
    /// </summary>
    /// <param name="control">承载图像的 Halcon 窗口控件，用于读显示区域与 DPI 缩放。</param>
    /// <param name="imageWidth">当前帧的图像宽度（列数）。</param>
    /// <param name="imageHeight">当前帧的图像高度（行数）。</param>
    internal ImageCanvasContext(HSmartWindowControlWPF control, int imageWidth, int imageHeight)
    {
        _control = control;
        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
    }

    /// <summary>
    /// 图像像素 → 屏幕像素的缩放系数。
    /// 叠加层想画"屏幕上固定大小"的元素时用它（例如始终 6px 的定位圆点）。
    /// <para>
    /// 取值 = 控件物理像素宽 / 显示区域列跨度；显示区域无效或控件尺寸不为正时返回 1。
    /// </para>
    /// </summary>
    public double Scale
    {
        get
        {
            if (!TryGetDisplayPart(out _, out var col1, out _, out var col2))
                return 1d;
            var width = ControlPixelWidth;
            var span = col2 - col1;
            return width <= 0 || span < 0 ? 1d : width / span;
        }
    }

    /// <summary>
    /// 屏幕坐标（相对控件左上角）→ 图像坐标（行、列）。
    /// 拿不到当前显示区域时返回 false。
    /// <para>
    /// <b>screen 的单位必须与 Halcon 窗口一致，也就是物理像素。</b>
    /// 换算里的控件尺寸走 <see cref="ControlPixelWidth"/>（已乘 DPI 缩放），
    /// 所以 WPF 鼠标事件给的设备无关坐标（DIP）不能直接传进来：
    /// 高 DPI 屏上要先乘 <c>VisualTreeHelper.GetDpi(control).DpiScaleX / DpiScaleY</c>，
    /// 否则画面一大一小，误差随缩放比放大。
    /// </para>
    /// <para>
    /// 返回的行列是浮点数，调用方自行取整；本方法不做图像边界裁剪，
    /// 落在图像外的坐标会原样算出。
    /// </para>
    /// </summary>
    /// <param name="screen">相对控件左上角的屏幕坐标，单位是 Halcon 窗口的物理像素。</param>
    /// <param name="row">图像行坐标（Halcon 的 Y 方向）。</param>
    /// <param name="col">图像列坐标（Halcon 的 X 方向）。</param>
    /// <returns>换算成功返回 true；控件尚未就绪、没有可用显示区域或控件尺寸不为正时返回 false，此时 row 与 col 置 0。</returns>
    public bool TryScreenToImage(Point screen, out double row, out double col)
    {
        row = 0;
        col = 0;
        if (!TryGetDisplayPart(out var row1, out var col1, out var row2, out var col2))
            return false;
        var width = ControlPixelWidth;
        var height = ControlPixelHeight;
        if (width <= 0 || height <= 0)
            return false;
        col = col1 + screen.X / width * (col2 - col1);
        row = row1 + screen.Y / height * (row2 - row1);
        return true;
    }

    /// <summary>
    /// 控件的物理像素宽。
    /// <para>
    /// WPF 的 <c>ActualWidth</c> 是设备无关单位（DIP），而 Halcon 窗口是按物理像素建的，
    /// 高 DPI 屏上两者不等，所以必须乘 DPI 缩放后才是 Halcon 那边的坐标口径。
    /// </para>
    /// </summary>
    private double ControlPixelWidth => _control.ActualWidth * DpiScaleX;

    /// <summary>控件的物理像素高。口径与 <see cref="ControlPixelWidth"/> 相同。</summary>
    private double ControlPixelHeight => _control.ActualHeight * DpiScaleY;

    /// <summary>
    /// X 方向的 DPI 缩放：1 个设备无关单位对应几个物理像素（100% 为 1，150% 为 1.5）。
    /// <para>
    /// 取自控件所在渲染链；控件还没进窗口等读不到的情况下按 1 处理，让绘制继续跑完。
    /// </para>
    /// </summary>
    private double DpiScaleX
    {
        get
        {
            try
            {
                return VisualTreeHelper.GetDpi(_control).DpiScaleX;
            }
            catch (Exception e)
            {
                return 1d;
            }
        }
    }

    /// <summary>Y 方向的 DPI 缩放。取值方式与兜底行为同 <see cref="DpiScaleX"/>。</summary>
    private double DpiScaleY
    {
        get
        {
            try
            {
                return VisualTreeHelper.GetDpi(_control).DpiScaleY;
            }
            catch (Exception e)
            {
                return 1d;
            }
        }
    }

    /// <summary>
    /// 取当前"图像在窗口中被显示的区域"，返回 (row1, col1, row2, col2)。
    /// 尚未设置过图像、或控件还没准备好时返回 false。
    /// <para>
    /// 区域来自控件的 <c>HImagePart</c>（<c>Rect</c>，字段是 X / Y / Width / Height，
    /// 注意 X 对应列、Y 对应行）。每次调用都重新读取，因此窗口缩放或平移后不需要额外的通知机制。
    /// </para>
    /// </summary>
    /// <param name="row1">显示区域左上角的行坐标。</param>
    /// <param name="col1">显示区域左上角的列坐标。</param>
    /// <param name="row2">显示区域右下角的行坐标。</param>
    /// <param name="col2">显示区域右下角的列坐标。</param>
    /// <returns>取到有效区域返回 true；宽或高不为正、以及读取抛异常时返回 false，四个坐标置 0。</returns>
    private bool TryGetDisplayPart(out double row1, out double col1, out double row2, out double col2)
    {
        row1 = row2 = col1 = col2 = 0d;
        try
        {
            var part = _control.HImagePart;
            if (part.Width <= 0 || part.Height <= 0)
                return false;
            col1 = part.X;
            row1 = part.Y;
            col2 = part.X + part.Width;
            row2 = part.Y + part.Height;
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}
