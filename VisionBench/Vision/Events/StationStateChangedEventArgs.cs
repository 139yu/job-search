using Vision.Camera;
using Vision.Enums;

namespace Vision.Events;

public class StationStateChangedEventArgs : EventArgs
{
    public StationEnum StationName { get; set; }
    public StationConnectionState OldState { get; set; }
    public StationConnectionState NewState { get; set; }

    public StationStateChangedEventArgs(StationEnum stationName, StationConnectionState oldState, StationConnectionState newState)
    {
        this.StationName = stationName;
        this.OldState = oldState;
        this.NewState = newState;
    }
}