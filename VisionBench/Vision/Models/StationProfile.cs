using Vision.Camera;

namespace Vision.Models;

public class StationProfile
{
    public StationEnum StationName { get; set; }
    public CameraProfile Camera { get; set; }
    public bool IsBind { get; set; } = false;
}