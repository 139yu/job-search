using Vision.Enums;

namespace Vision.Models;

public class CameraFrame
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Memory<byte> ImageData { get; set; }
    public ImageLayoutEnum ImageLayout { get; set; }
}