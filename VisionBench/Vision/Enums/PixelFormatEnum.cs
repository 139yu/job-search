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
    Mono12Packed,

    /// <summary>
    /// 14 位灰度图像，每像素 2 字节（低 14 位有效）。
    /// </summary>
    Mono14,

    /// <summary>
    /// 16 位灰度图像，每像素 2 字节。
    /// </summary>
    Mono16,

    /// <summary>
    /// 8 位彩色图像，RGB 顺序，每像素 3 字节。
    /// </summary>
    RGB8,

    /// <summary>
    /// 8 位彩色图像，BGR 顺序，每像素 3 字节。
    /// </summary>
    BGR8,

    /// <summary>
    /// 8 位 Bayer 原始格式，GR 相位，每像素 1 字节，需去马赛克。
    /// </summary>
    BayerGR8,

    /// <summary>
    /// 8 位 Bayer 原始格式，RG 相位，每像素 1 字节，需去马赛克。
    /// </summary>
    BayerRG8,

    /// <summary>
    /// 8 位 Bayer 原始格式，GB 相位，每像素 1 字节，需去马赛克。
    /// </summary>
    BayerGB8,

    /// <summary>
    /// 8 位 Bayer 原始格式，BG 相位，每像素 1 字节，需去马赛克。
    /// </summary>
    BayerBG8,

    /// <summary>
    /// 未知或暂不支持的像素格式，用于映射兜底。
    /// </summary>
    Unknown
}