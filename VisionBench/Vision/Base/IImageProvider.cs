using Vision.Events;

namespace Vision.Base;

public interface IImageProvider
{
    event EventHandler<FrameReadyEventArgs> FrameReady;
    void Start();
    void Stop();
    bool IsRunning { get; }
}