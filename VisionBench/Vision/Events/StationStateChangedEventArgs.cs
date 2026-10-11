using Vision.Enums;

namespace Vision.Events;

public class StationStateChangedEventArgs : EventArgs
{
    public StationEnum StationName { get; set; }
    public StationStateEnum OldState { get; set; }
    public StationStateEnum NewState { get; set; }

    public StationStateChangedEventArgs(StationEnum stationName, StationStateEnum oldState, StationStateEnum newState)
    {
        this.StationName = stationName;
        this.OldState = oldState;
        this.NewState = newState;
    }
}