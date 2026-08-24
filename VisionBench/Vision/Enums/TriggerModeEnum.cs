namespace Vision.Enums;

/// <summary>
/// 相机触发模式枚举。
/// 定义相机图像采集的触发方式。
/// </summary>
public enum TriggerModeEnum
{
    /// <summary>
    /// 软件触发：由上位机发送软件指令触发采图。
    /// </summary>
    SoftTrigger,

    /// <summary>
    /// 硬件触发：由外部硬件信号（如 PLC、传感器）触发采图。
    /// </summary>
    HardWare,

    /// <summary>
    /// 自由运行：相机按设定帧率连续自动采图，无需外部触发。
    /// </summary>
    FreeRun
}