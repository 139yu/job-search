namespace Vision.Enums;

/// <summary>
/// 像素格式枚举。
/// 定义相机输出的像素格式，Mono 表示单通道灰度图像，
/// 数字表示每个像素的位深（如 Mono8 即 8 位灰度）。
/// </summary>
public enum PixelFormatEnum
{
    /// <summary>
    /// 8 位灰度图像，每像素 1 字节。
    /// </summary>
    Mono8,

    /// <summary>
    /// 10 位灰度图像，每像素 2 字节（低 10 位有效）。
    /// </summary>
    Mono10,

    /// <summary>
    /// 10 位灰度图像，打包格式存储（像素数据位紧凑排列，节省存储空间）。
    /// </summary>
    Mono10Packed,

    /// <summary>
    /// 12 位灰度图像，每像素 2 字节（低 12 位有效）。
    /// </summary>
    Mono12,

    /// <summary>
    /// 12 位灰度图像，打包格式存储（像素数据位紧凑排列，节省存储空间）。
    /// </summary>
    Mono12Packed
}