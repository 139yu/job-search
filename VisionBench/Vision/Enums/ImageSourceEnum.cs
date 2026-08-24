namespace Vision.Enums;

/// <summary>
/// 图像来源类型枚举。
/// 标识一帧图像的来源，用于区分相机实时采集与本地文件读取。
/// </summary>
public enum ImageSourceEnum
{
    /// <summary>
    /// 图像来自本地文件。
    /// </summary>
    File,

    /// <summary>
    /// 图像来自相机实时采集。
    /// </summary>
    Camera
}