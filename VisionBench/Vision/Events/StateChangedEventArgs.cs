using Vision.Enums;

namespace Vision.Events;

public class StateChangedEventArgs : EventArgs
{
    public CameraStateEnum OldState { get; set; }
    public CameraStateEnum NewState { get; set; }

    public StateChangedEventArgs(CameraStateEnum oldState, CameraStateEnum newState)
    {
        this.OldState = oldState;
        this.NewState = newState;
    }
}