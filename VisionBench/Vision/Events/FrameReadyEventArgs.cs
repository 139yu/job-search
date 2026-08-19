using HalconDotNet;
using Vision.Enums;

namespace Vision.Events;

public class FrameReadyEventArgs : EventArgs
{
    public HImage Image { get; set; }
    public ImageSourceEnum SourceName { get; set; }
}