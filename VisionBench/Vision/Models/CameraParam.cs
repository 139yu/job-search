using Vision.Enums;

namespace Vision.Models;

/// <summary>
/// 相机运行参数模型。
/// 保存相机的采集相关参数配置，可在采集前设置并下发到相机。
/// </summary>
public class CameraParam: BindableBase
{
    /// <summary>
    /// 曝光时间（单位：微秒）。
    /// </summary>
    private float _exposureTime = 5000f;

    public float ExposureTime
    {
        get  => _exposureTime;
        set => SetProperty(ref _exposureTime, value);
    }
    

    /// <summary>
    /// 增益值。
    /// </summary>
    private float _gain = 1.0f;

    public float Gain
    {
        get => _gain;
        set => SetProperty(ref _gain, value);
    }

    /// <summary>
    /// 采集图像宽度（单位：像素）。
    /// </summary>
    public int? ImageWidth { get; set; }

    /// <summary>
    /// 采集图像高度（单位：像素）。
    /// </summary>
    public int? ImageHeight { get; set; }

    /// <summary>
    /// 采集区域起点 X 坐标（相对感光芯片原点，单位：像素）。
    /// </summary>
    public int? StartX { get; set; } = 0;

    /// <summary>
    /// 采集区域起点 Y 坐标（相对感光芯片原点，单位：像素）。
    /// </summary>
    public int? StartY { get; set; } = 0;

    /// <summary>
    /// 采集区域终点 X 坐标（单位：像素）。
    /// </summary>
    public int? EndX { get; set; } = 2048;

    /// <summary>
    /// 采集区域终点 Y 坐标（单位：像素）。
    /// </summary>
    public int? EndY { get; set; } = 2048;

    /// <summary>
    /// 采集超时时间，单位ms
    /// </summary>
    public int? GrabTimeout { get; set; } = 1000;

    /// <summary>
    /// 水平翻转
    /// </summary>
    private bool _reverseX = true;

    public bool ReverseX
    {
        get => _reverseX;
        set => SetProperty(ref _reverseX, value);
    }

    /// <summary>
    /// 垂直翻转
    /// </summary>
    private bool _reverseY = true;

    public bool ReverseY
    {
        get => _reverseY;
        set => SetProperty(ref _reverseY, value);
    }
    
    /// <summary>
    /// 相机像元尺寸（单位：微米），用于像素与物理尺寸之间的换算。
    /// </summary>
    private double? _pixelSize;

    public double? PixelSize
    {
        get => _pixelSize;
        set => SetProperty(ref _pixelSize, value);
    }
}