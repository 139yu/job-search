using HalconDotNet;
using Vision.Enums;

namespace Vision.Calibration;

public sealed class CalibrationSnapshot
{
    public const int CurrentSchemaVersion = 1;

    /// <summary>
    /// 未标定快照，
    /// </summary>
    public static CalibrationSnapshot None { get; } = new() { Kind = CalibrationKindEnum.None };

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    /// <summary>版本标识，同时用于文件名与结果追溯。标定类快照不可为空。</summary>
    public string Version { get; init; } = "";

    public CalibrationKindEnum Kind { get; init; }

    public DateTime CalibrateAt { get; init; }

    /// <summary>相机指纹（分辨率/像元/焦距等拼接），用于换相机或改分辨率后提示标定失效。</summary>
    public string CameraFingerprint { get; init; } = "";

    /// <summary>标定时的图像宽度（像素）。与指纹一起判断“分辨率变了标定还成不成立”。</summary>
    public int ImageWidth { get; init; }

    public int ImageHeight { get; init; }

    /// <summary>ScaleOnly：每像素毫米数。CameraPlane 时为空。</summary>
    public double? MillimetersPerPixel { get; set; }

    /// <summary>CameraPlane：area_scan_division 的 8 元组 [Focus,Kappa,Sx,Sy,Cx,Cy,W,H]（米/无量纲/米…）。</summary>
    public double[] CameraParameters { get; init; } = Array.Empty<double>();

    /// <summary>CameraPlane：测量平面 → 世界坐标的 7 元位姿，由标定板某一张图的位姿取原点得到。</summary>
    public double[] WorldPose { get; init; } = Array.Empty<double>();

    /// <summary>标定板描述文件名，用于复核“当时用的是哪块板”。</summary>
    public string PlateDescriptor { get; init; }

    /// <summary>重投影 RMS（像素）。只作质量参考，不作判定门槛。</summary>
    public double ReprojectionError { get; init; }

    /// <summary>是否已标定。为 false 时所有物理量口径都不可用。</summary>
    public bool IsCalibrated => Kind != CalibrationKindEnum.None;

    public bool SupportsWorldPlane =>
        Kind is CalibrationKindEnum.ScaleOnly or CalibrationKindEnum.CameraPlane;

    /// <summary>物理量单位。当前固定毫米（ImagePointsToWorldPlane 的 scale 参数即 "mm"）。</summary>
    public MeasureUnitEnum WorldUnit => MeasureUnitEnum.Millimeter;

    /// <summary>
    /// 取出halcon相机参数元组
    /// </summary>
    /// <returns></returns>
    public HTuple CreateCameraParam()
    {
        EnsureKind(CalibrationKindEnum.CameraPlane);
        return new HTuple(CameraParameters);
    }

    /// <summary>取出世界位姿元组。<b>调用方必须释放</b>。</summary>
    public HTuple CreateWorldPose()
    {
        EnsureKind(CalibrationKindEnum.CameraPlane);
        return new HTuple(WorldPose);
    }

    public bool MatchCamera(string? fingerprint)
    {
        if (string.IsNullOrWhiteSpace(fingerprint) || string.IsNullOrWhiteSpace(CameraFingerprint))
            return true;
        return string.Equals(fingerprint, CameraFingerprint, StringComparison.Ordinal);
    }

    public void Validate()
    {
        if (Kind == CalibrationKindEnum.None)
            return;
        if (string.IsNullOrWhiteSpace(Version))
            throw new ArgumentException("标定快照缺少版本号", nameof(Version));
        switch (Kind)
        {
            case CalibrationKindEnum.ScaleOnly:
                if (MillimetersPerPixel is null or <= 0)
                    throw new ArgumentException("ScaleOnly标定参数不能为负");
                break;
            case CalibrationKindEnum.CameraPlane:
                if (CameraParameters.Length != 8)
                    throw new ArgumentException("标定参数错误");
                break;
            case CalibrationKindEnum.HandEye:
                throw new ArgumentException("当前未实现");
            default:
                throw new ArgumentException("未知参数");
        }
    }

    private void EnsureKind(CalibrationKindEnum expected)
    {
        if (Kind != expected)
            throw new InvalidOperationException($"当前标定类型是 {Kind}，无法获取 {expected} 的参数");
    }
}