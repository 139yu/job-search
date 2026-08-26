using Vision.Enums;

namespace Vision.Models;

/// <summary>
/// 相机运行参数模型。
/// 保存相机的采集相关参数配置，可在采集前设置并下发到相机。
/// </summary>
public class CameraParam
{
    /// <summary>
    /// 曝光时间（单位：微秒）。
    /// </summary>
    public float ExposureTime { get; set; } = 5000f;

    /// <summary>
    /// 增益值。
    /// </summary>
    public float Gain { get; set; } = 1.0f;

    /// <summary>
    /// 像素格式（如 Mono8、Mono12 等）。
    /// </summary>
    public PixelFormatEnum PixelFormat { get; set; } = PixelFormatEnum.Mono8;

    /// <summary>
    /// 采集图像宽度（单位：像素）。
    /// </summary>
    public int ImageWidth { get; set; }

    /// <summary>
    /// 采集图像高度（单位：像素）。
    /// </summary>
    public int ImageHeight { get; set; }

    /// <summary>
    /// 采集区域起点 X 坐标（相对感光芯片原点，单位：像素）。
    /// </summary>
    public int StartX { get; set; } = 0;

    /// <summary>
    /// 采集区域起点 Y 坐标（相对感光芯片原点，单位：像素）。
    /// </summary>
    public int StartY { get; set; } = 0;

    /// <summary>
    /// 采集区域终点 X 坐标（单位：像素）。
    /// </summary>
    public int EndX { get; set; } = 2048;

    /// <summary>
    /// 采集区域终点 Y 坐标（单位：像素）。
    /// </summary>
    public int EndY { get; set; } = 2048;

    /// <summary>
    /// 采集超时时间，单位ms
    /// </summary>
    public int GrabTimeout { get; set; } = 1000;

    /// <summary>
    /// 水平翻转
    /// </summary>
    public bool ReverseX { get; set; } = true;

    /// <summary>
    /// 垂直翻转
    /// </summary>
    public bool ReverseY { get; set; } = true;
}