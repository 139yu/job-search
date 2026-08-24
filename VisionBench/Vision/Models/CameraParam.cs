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
    public int ExposureTime { get; set; }

    /// <summary>
    /// 增益值。
    /// </summary>
    public int Gain { get; set; }

    /// <summary>
    /// 像素格式（如 Mono8、Mono12 等）。
    /// </summary>
    public PixelFormatEnum PixelFormat { get; set; }

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
    public int StartX { get; set; }

    /// <summary>
    /// 采集区域起点 Y 坐标（相对感光芯片原点，单位：像素）。
    /// </summary>
    public int StartY { get; set; }

    /// <summary>
    /// 采集区域终点 X 坐标（单位：像素）。
    /// </summary>
    public int EndX { get; set; }

    /// <summary>
    /// 采集区域终点 Y 坐标（单位：像素）。
    /// </summary>
    public int EndY { get; set; }

    /// <summary>
    /// 触发模式（软件触发 / 硬件触发 / 自由运行）。
    /// </summary>
    public TriggerModeEnum TriggerMode { get; set; }
}