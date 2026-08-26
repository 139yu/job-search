using Vision.Enums;

namespace Vision.Models;

/// <summary>
/// 相机设备信息模型。
/// 描述一台相机设备的静态属性信息，通常在枚举设备后填充。
/// </summary>
public class CameraInfo
{
    /// <summary>
    /// 相机品牌类型（如海康）。
    /// </summary>
    public CameraEnum CameraType { get; set; }

    /// <summary>
    /// 相机名称（用户自定义或厂商默认名称）。
    /// </summary>
    public string CameraName { get; set; }

    /// <summary>
    /// 相机序列号，用于唯一标识一台设备。
    /// </summary>
    public string SerialNum { get; set; }

    /// <summary>
    /// 相机像元尺寸（单位：微米），用于像素与物理尺寸之间的换算。
    /// </summary>
    public double PixelSize { get; set; } = 3.45;

    /// <summary>
    /// 相机型号。
    /// </summary>
    public string TypeModel { get; set; }

    /// <summary>
    /// 相机接口类型（如 GigE、USB3.0、Camera Link 等）。
    /// </summary>
    public string InterfaceType { get; set; }
}