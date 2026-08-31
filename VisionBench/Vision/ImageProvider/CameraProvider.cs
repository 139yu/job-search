using Vision.Base;
using Vision.Events;

namespace Vision.ImageProvider;

public class CameraProvider : IImageProvider
{
    public event EventHandler<FrameReadyEventArgs>? FrameReady;
    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public bool IsRunning { get; }
}