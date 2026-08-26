using Vision.Enums;

namespace Vision.Models;

public class CameraFrame
{
    public byte[] PixelData { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    /// <summary>
    /// 字节行数
    /// </summary>
    public int Stride { get; set; }
    
    public PixelFormatEnum PixelFormat { get; set; }
}